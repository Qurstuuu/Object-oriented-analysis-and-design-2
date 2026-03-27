#ifndef WEATHERDATA_H
#define WEATHERDATA_H

#include <QString>
#include <QDateTime>
#include <QMetaType>

struct WeatherData
{
    QString cityName;
    double temperature; // в градусах по цельсию
    QString condition;

    int humidity;       // влажность
    double windSpeed;   // м/с
    QString windDirection; // словами

    QDateTime lastUpdate;    // когда в последний раз загружали в кэш

    WeatherData()
        : temperature(0.0)
        , humidity(0)
        , windSpeed(0.0)
    {
    }
};

Q_DECLARE_METATYPE(WeatherData)

#endif // WEATHERDATA_H
