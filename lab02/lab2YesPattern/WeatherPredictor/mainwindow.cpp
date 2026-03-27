#include "mainwindow.h"
#include "ui_mainwindow.h"
#include "weatherdata.h"
#include "weatherfacade.h"

MainWindow::MainWindow(QWidget *parent)
    : QMainWindow(parent)
    , ui(new Ui::MainWindow)
    , m_weatherFacade(new WeatherFacade(this))
{
    ui->setupUi(this);

    m_weatherFacade->setApiKey("0656ec11-3d9f-4aa6-9fbb-4aa832e82ae4");

    connect(m_weatherFacade, &WeatherFacade::weatherReceived,
            this, &MainWindow::updateUI);
    connect(m_weatherFacade, &WeatherFacade::errorOccurred,
            this, &MainWindow::showError);
    connect(m_weatherFacade, &WeatherFacade::loadingStarted,
            this, &MainWindow::onLoadingStarted);
    connect(m_weatherFacade, &WeatherFacade::loadingFinished,
            this, &MainWindow::onLoadingFinished);

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
    QString city = ui->lineEditCity->text().trimmed();
    if (city.isEmpty()) {
        showError("Введите название города");
        return;
    }

    ui->groupBoxWeather->setVisible(false);
    ui->labelLoading->setVisible(true);
    m_weatherFacade->requestWeather(city);
}

void MainWindow::onLoadingStarted()
{
    ui->statusbar->clearMessage();
    ui->labelLoading->setVisible(true);
    ui->groupBoxWeather->setVisible(false);
    ui->statusbar->clearMessage();
}

void MainWindow::onLoadingFinished()
{
    ui->labelLoading->setVisible(false);
}

void MainWindow::updateUI(const WeatherData& data)
{
    ui->labelCity->setText(data.cityName);
    ui->labelTemperature->setText(QString::number(data.temperature) + "°C");
    ui->labelCondition->setText(data.condition);
    ui->labelWind->setText("Ветер: " + QString::number(data.windSpeed) + " м/с, " + data.windDirection);
    ui->groupBoxWeather->setVisible(true);
    ui->statusbar->clearMessage();
}

void MainWindow::showError(const QString& message)
{
    ui->statusbar->showMessage(message);
}
