#ifndef WEATHERFACADE_H
#define WEATHERFACADE_H

#include <QObject>
#include <QNetworkAccessManager>
#include <QNetworkReply>
#include <QCache>
#include <QSettings>
#include <QTimer>
#include "weatherdata.h"

class WeatherFacade : public QObject
{
    Q_OBJECT

public:
    explicit WeatherFacade(QObject *parent = nullptr);
    void requestWeather(const QString &cityName);
    void setApiKey(const QString &key) { m_yandexApiKey = key; }

signals:
    void weatherReceived(const WeatherData &data);
    void errorOccurred(const QString &message);
    void loadingStarted();
    void loadingFinished();

private slots:
    void onGeocodeReply(QNetworkReply *reply);
    void onWeatherReply(QNetworkReply *reply);
    void onTimeout();

private:
    QString buildYandexGeocodeUrl(const QString &cityName); //  яндекс геокодер
    bool parseYandexGeocode(const QByteArray &jsonData, double &lat, double &lon, QString &displayName);

    QString buildWeatherUrl(double lat, double lon);
    bool parseWeather(const QByteArray &jsonData, WeatherData &data);
    QString getWindDirection(double degrees);

    bool loadFromCache(const QString &city, WeatherData &data);
    void saveToCache(const WeatherData &data);
    QString getCacheKey(const QString &city);

    QNetworkAccessManager *m_geocodeManager;
    QNetworkAccessManager *m_weatherManager;

    QMap<QNetworkReply*, QString> m_pendingRequests;
    QMap<QNetworkReply*, QString> m_pendingWeatherRequests;
    QString m_currentCity;

    QString m_yandexApiKey;

    QCache<QString, WeatherData> m_memoryCache;
    QSettings *m_diskCache;
    QTimer *m_timeoutTimer;

    static const int TIMEOUT_MS = 15000;
    static const int CACHE_TIMEOUT_MINUTES = 30;

};

#endif // WEATHERFACADE_H
