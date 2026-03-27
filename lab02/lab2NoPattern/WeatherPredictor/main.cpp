#include <QApplication>
#include <QMetaType>
#include "mainwindow.h"
#include "weatherdata.h"


int main(int argc, char *argv[])
{
    QApplication a(argc, argv);
    qRegisterMetaType<WeatherData>("WeatherData");
    MainWindow w;
    w.show();
    return a.exec();
}
