#ifndef MAINWINDOW_H
#define MAINWINDOW_H

#include <QMainWindow>
#include <QObject>
#include <QNetworkAccessManager>
#include <QNetworkReply>
#include <QCache>
#include <QSettings>
#include <QTimer>
#include "weatherdata.h"

QT_BEGIN_NAMESPACE
namespace Ui {
class MainWindow;
}
QT_END_NAMESPACE

class MainWindow : public QMainWindow
{
    Q_OBJECT

public:
    MainWindow(QWidget *parent = nullptr);
    ~MainWindow();

private slots:
    void onSearchButtonClicked();
    void onGeocodeReply(QNetworkReply*);
    void onWeatherReply(QNetworkReply*);
    void onTimeout();

private:
    QString buildYandexGeocodeUrl(const QString &cityName); //  яндекс геокодер
    bool parseYandexGeocode(const QByteArray &jsonData, double &lat, double &lon, QString &displayName);

    QString buildWeatherUrl(double lat, double lon); // Open-Meteo
    bool parseWeather(const QByteArray &jsonData, WeatherData &data);
    QString getWindDirection(double degrees);

    void updateUI(const WeatherData& data);
    void showError(const QString& message);

    bool loadFromCache(const QString& city, WeatherData& data);
    void saveToCache(const QString& city, const WeatherData& data);
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

    Ui::MainWindow *ui;
};
#endif // MAINWINDOW_H
