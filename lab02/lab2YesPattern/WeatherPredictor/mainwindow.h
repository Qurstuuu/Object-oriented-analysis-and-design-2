#ifndef MAINWINDOW_H
#define MAINWINDOW_H

#include "weatherdata.h"
#include <QMainWindow>
#include <QObject>
#include <QNetworkAccessManager>
#include <QNetworkReply>
#include <QCache>
#include <QSettings>
#include <QTimer>

QT_BEGIN_NAMESPACE
namespace Ui {
class MainWindow;
}
QT_END_NAMESPACE

class WeatherFacade;

class MainWindow : public QMainWindow
{
    Q_OBJECT

public:
    MainWindow(QWidget *parent = nullptr);
    ~MainWindow();

private slots:
    void onSearchButtonClicked();
    void updateUI(const WeatherData& data);
    void showError(const QString& message);
    void onLoadingStarted();
    void onLoadingFinished();

private:
    WeatherFacade *m_weatherFacade;
    Ui::MainWindow *ui;
};
#endif // MAINWINDOW_H
