#include "mainwindow.h"
#include "ui_mainwindow.h"

#include <QNetworkAccessManager>
#include <QNetworkReply>
#include <QNetworkRequest>
#include <QUrl>
#include <QUrlQuery>
#include <QJsonDocument>
#include <QJsonObject>
#include <QJsonArray>
#include <QTimer>
#include <QSettings>
#include <QCache>

MainWindow::MainWindow(QWidget *parent)
    : QMainWindow(parent)
    , ui(new Ui::MainWindow)
    , m_geocodeManager(new QNetworkAccessManager(this))
    , m_weatherManager(new QNetworkAccessManager(this))
    , m_yandexApiKey("0656ec11-3d9f-4aa6-9fbb-4aa832e82ae4")
    , m_diskCache(new QSettings("WeatherApp", "Cache", this))
    , m_timeoutTimer(new QTimer(this))
{
    ui->setupUi(this);

    m_timeoutTimer->setSingleShot(true);
    m_timeoutTimer->setInterval(10000);
    connect(m_timeoutTimer, &QTimer::timeout, this, &MainWindow::onTimeout);

    m_memoryCache.setMaxCost(10);

    connect(ui->buttonFindCity, &QPushButton::clicked,
            this, &MainWindow::onSearchButtonClicked);

    connect(ui->lineEditCity, &QLineEdit::returnPressed,
            this, &MainWindow::onSearchButtonClicked);

    ui->groupBoxWeather->setVisible(false);
    ui->labelLoading->setVisible(false);
}

MainWindow::~MainWindow()
{
    delete ui;
}

void MainWindow::onSearchButtonClicked()
{
    ui->statusbar->clearMessage();
    QString city = ui->lineEditCity->text().trimmed();
    if (city.isEmpty()) {
        showError("Введите название города");
        return;
    }

    ui->groupBoxWeather->setVisible(false);
    ui->labelLoading->setVisible(true);

    WeatherData cachedData;
    if (loadFromCache(city, cachedData)) {
        updateUI(cachedData);
        ui->labelLoading->setVisible(false);
        return;
    }

    m_timeoutTimer->start();
    QString geocodeUrl = buildYandexGeocodeUrl(city);
    QUrl url(geocodeUrl);
    QNetworkRequest request(url);
    QNetworkReply *reply = m_geocodeManager->get(request);

    m_pendingRequests[reply] = city;

    connect(reply, &QNetworkReply::finished,
            this, [this, reply]() { onGeocodeReply(reply); });
}

QString MainWindow::buildYandexGeocodeUrl(const QString &cityName)
{
    QUrl url("https://geocode-maps.yandex.ru/1.x/");
    QUrlQuery query;

    query.addQueryItem("apikey", m_yandexApiKey);
    query.addQueryItem("geocode", cityName);
    query.addQueryItem("format", "json");
    query.addQueryItem("lang", "ru_RU");
    query.addQueryItem("results", "1");

    url.setQuery(query);
    return url.toString();
}

bool MainWindow::parseYandexGeocode(const QByteArray &jsonData,
                                    double &lat, double &lon,
                                    QString &displayName)
{
    QJsonDocument doc = QJsonDocument::fromJson(jsonData);
    if (doc.isNull() || !doc.isObject()) {
        qDebug() << "Ошибка: неверный JSON от геокодера";
        return false;
    }

    QJsonObject root = doc.object();
    QJsonObject response = root["response"].toObject();
    QJsonObject geoObjectCollection = response["GeoObjectCollection"].toObject();
    QJsonArray featureMember = geoObjectCollection["featureMember"].toArray();

    if (featureMember.isEmpty()) {
        qDebug() << "Город не найден";
        return false;
    }

    QJsonObject firstFeature = featureMember[0].toObject();
    QJsonObject geoObject = firstFeature["GeoObject"].toObject();

    displayName = geoObject["name"].toString();

    QString posStr = geoObject["Point"].toObject()["pos"].toString();
    QStringList coords = posStr.split(" ");

    if (coords.size() == 2) {
        lon = coords[0].toDouble();
        lat = coords[1].toDouble();
        return true;
    }

    return false;
}

void MainWindow::onGeocodeReply(QNetworkReply *reply)
{
    m_timeoutTimer->stop();

    QString cityName = m_pendingRequests.value(reply);
    m_pendingRequests.remove(reply);

    if (reply->error() != QNetworkReply::NoError) {
        showError("Город \"" + cityName + "\": " + reply->errorString());
        reply->deleteLater();

        if (m_pendingRequests.isEmpty() && m_pendingWeatherRequests.isEmpty()) {
            ui->labelLoading->setVisible(false);
        }
        return;
    }

    QByteArray data = reply->readAll();

    double lat = 0, lon = 0;
    QString displayName;

    if (!parseYandexGeocode(data, lat, lon, displayName)) {
        showError("Город \"" + cityName + "\" не найден");
        reply->deleteLater();

        if (m_pendingRequests.isEmpty() && m_pendingWeatherRequests.isEmpty()) {
            ui->labelLoading->setVisible(false);
        }
        return;
    }

    m_diskCache->setValue("coords_" + cityName, QPointF(lat, lon));

    QString weatherUrl = buildWeatherUrl(lat, lon);

    QUrl url(weatherUrl);
    QNetworkRequest request(url);
    QNetworkReply *weatherReply = m_weatherManager->get(request);

    m_pendingWeatherRequests[weatherReply] = displayName.isEmpty() ? cityName : displayName;

    connect(weatherReply, &QNetworkReply::finished,
            this, [this, weatherReply]() { onWeatherReply(weatherReply); });

    reply->deleteLater();
}

QString MainWindow::buildWeatherUrl(double lat, double lon)
{
    QUrl url("https://api.open-meteo.com/v1/forecast");
    QUrlQuery query;
    query.addQueryItem("latitude", QString::number(lat, 'f', 6));
    query.addQueryItem("longitude", QString::number(lon, 'f', 6));
    query.addQueryItem("current_weather", "true");
    query.addQueryItem("hourly", "temperature_2m,relativehumidity_2m,windspeed_10m");
    query.addQueryItem("timezone", "auto");
    query.addQueryItem("forecast_days", "1");
    url.setQuery(query);
    return url.toString();
}

bool MainWindow::parseWeather(const QByteArray &jsonData, WeatherData &data)
{
    QJsonDocument doc = QJsonDocument::fromJson(jsonData);
    if (doc.isNull() || !doc.isObject()) {
        qDebug() << "Ошибка: неверный JSON от погодного API";
        return false;
    }

    QJsonObject root = doc.object();
    QJsonObject current = root["current_weather"].toObject();

    if (current.isEmpty()) {
        qDebug() << "Нет данных о текущей погоде";
        return false;
    }

    data.temperature = current["temperature"].toDouble();
    data.windSpeed = current["windspeed"].toDouble();
    data.windDirection = getWindDirection(current["winddirection"].toDouble());

    int weatherCode = current["weathercode"].toInt();
    switch (weatherCode) {
    case 0: data.condition = "Ясно"; break;
    case 1: data.condition = "В основном ясно"; break;
    case 2: data.condition = "Переменная облачность"; break;
    case 3: data.condition = "Пасмурно"; break;
    case 45: case 48: data.condition = "Туман"; break;
    case 51: case 53: case 55: data.condition = "Морось"; break;
    case 61: case 63: case 65: data.condition = "Дождь"; break;
    case 71: case 73: case 75: data.condition = "Снег"; break;
    case 95: data.condition = "Гроза"; break;
    default: data.condition = "Неизвестно";
    }

    QJsonObject hourly = root["hourly"].toObject();
    QJsonArray humidityArray = hourly["relativehumidity_2m"].toArray();
    if (!humidityArray.isEmpty()) {
        data.humidity = humidityArray[0].toInt();
    }

    return true;
}

void MainWindow::onWeatherReply(QNetworkReply *reply)
{
    QString cityName = m_pendingWeatherRequests.value(reply);
    m_pendingWeatherRequests.remove(reply);

    if (reply->error() != QNetworkReply::NoError) {
        showError("Погода для \"" + cityName + "\": " + reply->errorString());
        reply->deleteLater();

        if (m_pendingRequests.isEmpty() && m_pendingWeatherRequests.isEmpty()) {
            ui->labelLoading->setVisible(false);
        }
        return;
    }

    QByteArray data = reply->readAll();

    WeatherData weather;
    weather.cityName = cityName;

    if (parseWeather(data, weather)) {
        weather.lastUpdate = QDateTime::currentDateTime();
        saveToCache(cityName, weather);
        updateUI(weather);
    } else {
        showError("Не удалось получить данные о погоде для \"" + cityName + "\"");
    }

    reply->deleteLater();

    if (m_pendingRequests.isEmpty() && m_pendingWeatherRequests.isEmpty()) {
        m_timeoutTimer->stop();
        ui->labelLoading->setVisible(false);
    }
}

void MainWindow::updateUI(const WeatherData& data)
{
    ui->labelCity->setText(data.cityName);
    ui->labelTemperature->setText(QString::number(data.temperature) + "°C");
    ui->labelCondition->setText(data.condition);
    ui->labelWind->setText("Ветер: " + QString::number(data.windSpeed) + " м/с, " + data.windDirection);
    ui->groupBoxWeather->setVisible(true);
}

void MainWindow::showError(const QString& message)
{
    ui->statusbar->showMessage(message);
}

bool MainWindow::loadFromCache(const QString &city, WeatherData &data)
{
    WeatherData *cached = m_memoryCache.object(city);
    if (cached) {
        qDebug() << "Загружено из памяти:" << city;
        data = *cached;
        return true;
    }

    QString key = getCacheKey(city);
    if (m_diskCache->contains(key)) {
        QByteArray cachedData = m_diskCache->value(key).toByteArray();
        QJsonDocument doc = QJsonDocument::fromJson(cachedData);

        if (!doc.isNull() && doc.isObject()) {
            QJsonObject obj = doc.object();
            data.cityName = obj["cityName"].toString();
            data.temperature = obj["temperature"].toDouble();
            data.condition = obj["condition"].toString();
            data.windSpeed = obj["windSpeed"].toDouble();
            data.humidity = obj["humidity"].toInt();
            data.lastUpdate = QDateTime::fromString(obj["lastUpdate"].toString(), Qt::ISODate);

            int secondsPassed = data.lastUpdate.secsTo(QDateTime::currentDateTime());
            if (secondsPassed < CACHE_TIMEOUT_MINUTES * 60) {
                qDebug() << "Загружено с диска:" << city << "(свежее)";
                m_memoryCache.insert(city, new WeatherData(data));
                return true;
            } else {
                qDebug() << "Данные для" << city << "устарели (" << secondsPassed / 60 << "минут)";
                m_diskCache->remove(key);
            }
        }
    }

    return false;
}

void MainWindow::saveToCache(const QString &city, const WeatherData &data)
{
    // Сохраняем в память
    m_memoryCache.insert(city, new WeatherData(data));

    // Сохраняем на диск в формате JSON
    QJsonObject obj;
    obj["cityName"] = data.cityName;
    obj["temperature"] = data.temperature;
    obj["condition"] = data.condition;
    obj["windSpeed"] = data.windSpeed;
    obj["humidity"] = data.humidity;
    obj["lastUpdate"] = data.lastUpdate.toString(Qt::ISODate);

    QJsonDocument doc(obj);
    QString key = getCacheKey(city);
    m_diskCache->setValue(key, doc.toJson());

    qDebug() << "Сохранено в кэш:" << city;
}

QString MainWindow::getCacheKey(const QString &city)
{
    return "weather_" + city.toLower();
}

QString MainWindow::getWindDirection(double degrees)
{
    if (degrees >= 337.5 || degrees < 22.5) return "северный";
    if (degrees >= 22.5 && degrees < 67.5) return "северо-восточный";
    if (degrees >= 67.5 && degrees < 112.5) return "восточный";
    if (degrees >= 112.5 && degrees < 157.5) return "юго-восточный";
    if (degrees >= 157.5 && degrees < 202.5) return "южный";
    if (degrees >= 202.5 && degrees < 247.5) return "юго-западный";
    if (degrees >= 247.5 && degrees < 292.5) return "западный";
    if (degrees >= 292.5 && degrees < 337.5) return "северо-западный";
    return "неизвестно";
}

void MainWindow::onTimeout()
{
    showError("Превышено время ожидания ответа от сервера");

    ui->labelLoading->setVisible(false);
}
