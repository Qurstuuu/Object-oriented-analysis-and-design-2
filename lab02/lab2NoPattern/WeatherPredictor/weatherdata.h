#ifndef WEATHERDATA_H
#define WEATHERDATA_H

#include <QString>
#include <QDateTime>
#include <QPixmap>
#include <QMetaType>

struct WeatherData
{
    // Основные данные
    QString cityName;        // Название города
    double temperature;      // Температура в градусах
    QString condition;       // Состояние погоды ("Солнечно", "Дождь" и т.д.)
    QString description;     // Подробное описание ("легкий дождь с грозой")

    // Дополнительные параметры
    int humidity;            // Влажность (%)
    double windSpeed;        // Скорость ветра (м/с)
    QString windDirection;   // Направление ветра ("северный", "юго-западный")
    int pressure;            // Давление (гПа)

    // Визуальные данные
    QPixmap icon;            // Иконка погоды
    QString iconCode;        // Код иконки от API (для загрузки)

    // Служебные поля
    QDateTime lastUpdate;    // Время последнего обновления

    // Конструктор по умолчанию (инициализируем значениями по умолчанию)
    WeatherData()
        : temperature(0.0)
        , humidity(0)
        , windSpeed(0.0)
        , pressure(0)
    {
    }

    // Конструктор с параметрами (удобно для тестирования)
    WeatherData(const QString& city, double temp, const QString& cond)
        : cityName(city)
        , temperature(temp)
        , condition(cond)
        , humidity(0)
        , windSpeed(0.0)
        , pressure(0)
    {
    }

    // Проверка, что данные валидны (не пустой город)
    bool isValid() const {
        return !cityName.isEmpty();
    }

    // Сброс всех данных
    void clear() {
        cityName.clear();
        temperature = 0.0;
        condition.clear();
        description.clear();
        humidity = 0;
        windSpeed = 0.0;
        windDirection.clear();
        pressure = 0;
        icon = QPixmap();
        iconCode.clear();
        lastUpdate = QDateTime();
    }

    // Оператор сравнения (для тестирования)
    bool operator==(const WeatherData& other) const {
        return cityName == other.cityName &&
               qFuzzyCompare(temperature, other.temperature) &&
               condition == other.condition;
    }
};

Q_DECLARE_METATYPE(WeatherData)

#endif // WEATHERDATA_H
