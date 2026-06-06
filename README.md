<div align="center">
<img src="https://raw.githubusercontent.com/vpt-student-projects/edu-centralized-monitoring/main/TechInventory/techinventory.ico" width="80" height="80" alt="TechInventory Icon"/>
🖥 TechInventory
Система централизованного мониторинга и управления IT-инфраструктурой образовательного учреждения
Show Image
Show Image
Show Image
Show Image
Show Image
<br/>

Дипломный проект — десктопное WPF-приложение для автоматизации учёта IT-оборудования в техникуме:
иерархическое дерево кабинетов, плиточный интерфейс, диспетчеризация заявок и аналитические отчёты на Python.

<br/>
Show Image
 
Show Image
 
Show Image
 
Show Image
</div>

📋 Содержание

О проекте
Возможности
Стек технологий
Архитектура
Структура проекта
Быстрый старт
База данных
Отчёты Python
Юнит-тесты
Роли пользователей
Скриншоты


🎯 О проекте
TechInventory решает реальную проблему образовательных учреждений: учёт IT-оборудования в техникумах до сих пор ведётся в бумажных журналах или разрозненных Excel-таблицах. Это приводит к потере истории перемещений, неоперативной обработке заявок на ремонт и невозможности быстро получить актуальную статистику для закупок.
Система обеспечивает:
ПроблемаРешениеРазрозненный учёт в ExcelЦентрализованная БД с иерархией «корпус → этаж → кабинет»Потеря истории перемещенийТаблица MovementHistory с датой и причиной каждого перемещенияУстная подача заявокМодуль тикетов со статусами «Новая / В работе / Завершена»Ручная инвентаризацияPython-скрипты генерируют Excel-отчёты автоматическиНет данных для закупокОтчёт по сломанному оборудованию с процентами по типам

✨ Возможности
🗂 Иерархический учёт техники

Дерево навигации Корпус → Этаж → Кабинет в левой панели
Плиточное отображение устройств в выбранном кабинете (стиль Windows 11)
На каждой плитке: название, статус, ответственный преподаватель
Красная рамка + значок ⚠ на плитке при наличии открытой заявки

📋 Детальная карточка устройства

Технические характеристики (процессор, ОЗУ, видеокарта, накопитель)
Вкладка История перемещений — откуда, куда, когда
Вкладка Заявки — все тикеты по данному устройству
Назначение ответственного пользователя (Admin)

🔧 Модуль заявок (тикетов)

Создание заявки с выбором приоритета (🔴 Высокий / 🟡 Средний / 🟢 Низкий)
Лента заявок с фильтрацией по приоритету, статусу, кабинету
Закрытие заявки автоматически восстанавливает статус устройства «Работает»
Фильтр «Только мои устройства» для преподавателей

📊 Аналитические отчёты

Для закупки — список сломанного оборудования с % по типам
Инвентаризация — полная опись всех устройств по кабинетам
Статистика заявок — сводка по статусам и приоритетам
Генерация в .xlsx через Python + pandas + openpyxl

👥 Управление пользователями

Роли: Admin и Teacher
Регистрация, редактирование, удаление пользователей
Хеширование паролей SHA-256
Сессия сохраняется между запусками (session.dat)

📁 Управление кабинетами и справочниками

CRUD для кабинетов (название, этаж, корпус, описание)
Справочники типов устройств, статусов устройств и статусов заявок
Автоматическое обновление дерева после изменений


🛠 Стек технологий
<div align="center">
СлойТехнологияНазначениеUIWPF (.NET 8)Десктопный интерфейсПаттернMVVMРазделение логики и представленияБДSQLite 3Локальное хранилище данныхДоступ к даннымADO.NET (System.Data.SQLite)Запросы без ORMАналитикаPython 3.10+Генерация отчётовОтчётыpandas + openpyxlExcel-файлыБезопасностьSHA-256Хеширование паролейТестыxUnitЮнит-тестирование
</div>

🏗 Архитектура
┌─────────────────────────────────────────────────────────┐
│                    TechInventory (WPF)                  │
│  ┌──────────────┐  ┌──────────────┐  ┌───────────────┐  │
│  │    Views     │  │  ViewModels  │  │    Helpers    │  │
│  │  (XAML UI)   │◄─┤   (MVVM)    │  │ Logger/Hash   │  │
│  └──────────────┘  └──────┬───────┘  └───────────────┘  │
└──────────────────────────┼──────────────────────────────┘
                           │ AppServices
┌──────────────────────────▼──────────────────────────────┐
│                   Inventory.Core                        │
│  ┌──────────────┐  ┌──────────────┐  ┌───────────────┐  │
│  │  Interfaces  │  │   Services   │  │    Models     │  │
│  │ IDevice/     │  │ DeviceService│  │ Device/Ticket │  │
│  │ ITicket/...  │  │ TicketService│  │ Room/User/... │  │
│  └──────────────┘  └──────────────┘  └───────────────┘  │
│  ┌──────────────┐  ┌──────────────┐                     │
│  │ Repositories │  │  Constants   │                     │
│  │ DeviceRepo   │  │TicketStatuses│                     │
│  │ TicketRepo   │  └──────────────┘                     │
│  │ RoomRepo/... │                                       │
│  └──────┬───────┘                                       │
└─────────┼───────────────────────────────────────────────┘
          │ ADO.NET
┌─────────▼───────────┐     ┌──────────────────────────┐
│    SQLite (БД)      │     │   Python Scripts         │
│  inventory.db       │◄────┤  report_generator.py     │
│                     │     │  pandas / openpyxl       │
└─────────────────────┘     └──────────────────────────┘

📁 Структура проекта
edu-centralized-monitoring/
│
├── 📦 Inventory.Core/                  # Серверная часть (бизнес-логика)
│   ├── Common/Exceptions/
│   │   └── BusinessException.cs        # Исключения бизнес-логики
│   ├── Constants/
│   │   └── TicketStatuses.cs           # Константы статусов заявок
│   ├── Interfaces/                     # Интерфейсы репозиториев и сервисов
│   │   ├── IDeviceRepository.cs
│   │   ├── IDeviceService.cs
│   │   ├── ITicketRepository.cs
│   │   ├── ITicketService.cs
│   │   ├── IRoomRepository.cs
│   │   ├── IDictionaryRepository.cs
│   │   ├── IMovementRepository.cs
│   │   └── IUserRepository.cs
│   ├── Models/                         # Модели данных
│   │   ├── Device.cs
│   │   ├── Ticket.cs
│   │   ├── Room.cs
│   │   ├── User.cs
│   │   ├── DictionaryItem.cs
│   │   └── MovementHistory.cs
│   ├── Repositories/                   # Реализация доступа к данным (ADO.NET)
│   │   ├── BaseRepository.cs
│   │   ├── DeviceRepository.cs
│   │   ├── TicketRepository.cs
│   │   ├── RoomRepository.cs
│   │   ├── DictionaryRepository.cs
│   │   ├── MovementRepository.cs
│   │   └── UserRepository.cs
│   ├── Services/                       # Бизнес-сервисы
│   │   ├── DeviceService.cs
│   │   └── TicketService.cs
│   ├── AppServices.cs                  # Точка входа для DI
│   └── DatabaseMigrator.cs             # Авто-миграции БД
│
├── 🖥 TechInventory/                   # Клиентская часть (WPF)
│   ├── Helpers/
│   │   ├── BoolToVisibilityConverter.cs
│   │   ├── StatusToColorConverter.cs
│   │   ├── StringToVisibilityConverter.cs
│   │   ├── Logger.cs
│   │   ├── PasswordHasher.cs
│   │   └── RelayCommand.cs
│   ├── ViewModels/
│   │   ├── MainViewModel.cs
│   │   ├── DeviceCardViewModel.cs
│   │   ├── TicketListViewModel.cs
│   │   ├── LoginViewModel.cs
│   │   └── ...NodeViewModel.cs
│   ├── Views/
│   │   ├── MainWindow.xaml(.cs)
│   │   ├── LoginWindow.xaml(.cs)
│   │   ├── DeviceCardWindow.xaml(.cs)
│   │   ├── TicketsWindow.xaml(.cs)
│   │   ├── CreateTicketWindow.xaml(.cs)
│   │   ├── MoveDeviceWindow.xaml(.cs)
│   │   ├── ReportWindow.xaml(.cs)
│   │   ├── RoomsManagementWindow.xaml(.cs)
│   │   ├── DictionaryWindow.xaml(.cs)
│   │   ├── UsersWindow.xaml(.cs)
│   │   └── RegisterWindow.xaml(.cs)
│   ├── Scripts/
│   │   └── report_generator.py         # Python-скрипт отчётов
│   ├── App.xaml(.cs)                   # Точка входа WPF
│   └── techinventory.ico               # Иконка приложения
│
└── 🧪 Inventory.Tests/                 # Юнит-тесты (xUnit)
    └── ...Tests.cs                     # 10 тестов бизнес-логики

🚀 Быстрый старт
Требования

Windows 10 / 11
.NET 8.0 SDK
Python 3.10+ (для отчётов)
Visual Studio 2022 (рекомендуется)

Установка
1. Клонировать репозиторий
bashgit clone https://github.com/vpt-student-projects/edu-centralized-monitoring.git
cd edu-centralized-monitoring
2. Установить зависимости Python
bashpip install pandas openpyxl
3. Открыть решение в Visual Studio
edu-centralized-monitoring.sln
4. Собрать и запустить
F5 или Ctrl+F5

✅ База данных inventory.db создаётся автоматически при первом запуске.
✅ Миграции (добавление новых колонок) применяются автоматически при старте.

Первый вход
После запуска создайте пользователя через форму регистрации или используйте тестовые данные:
ЛогинПарольРольadminadmin123Adminteacherteacher123Teacher

⚠️ Тестовые данные нужно заполнить в inventory.db вручную или через скрипт инициализации.


🗄 База данных
Схема базы данных SQLite:
sql-- Кабинеты
CREATE TABLE Rooms (
    RoomID      INTEGER PRIMARY KEY AUTOINCREMENT,
    Name        TEXT NOT NULL,          -- "220", "216"
    Floor       INTEGER NOT NULL,
    Building    TEXT NOT NULL,
    Description TEXT
);

-- Устройства
CREATE TABLE Devices (
    DeviceID         INTEGER PRIMARY KEY AUTOINCREMENT,
    Name             TEXT NOT NULL,
    TypeID           INTEGER NOT NULL,  -- FK → Dictionary
    Specs            TEXT,
    StatusID         INTEGER NOT NULL,  -- FK → Dictionary
    CurrentRoomID    INTEGER NOT NULL,  -- FK → Rooms
    PositionInRoom   INTEGER,
    AssignedToUserID INTEGER            -- FK → Users
);

-- Заявки
CREATE TABLE Tickets (
    TicketID    INTEGER PRIMARY KEY AUTOINCREMENT,
    DeviceID    INTEGER NOT NULL,       -- FK → Devices
    RoomID      INTEGER,                -- FK → Rooms
    Description TEXT NOT NULL,
    Priority    INTEGER DEFAULT 3,      -- 1=высокий, 2=средний, 3=низкий
    StatusID    INTEGER NOT NULL,       -- FK → Dictionary (5=Новая, 6=В работе, 7=Завершена)
    CreatedAt   DATETIME,
    ClosedAt    DATETIME
);

-- История перемещений
CREATE TABLE MovementHistory (
    HistoryID  INTEGER PRIMARY KEY AUTOINCREMENT,
    DeviceID   INTEGER NOT NULL,
    OldRoomID  INTEGER NOT NULL,
    NewRoomID  INTEGER NOT NULL,
    MoveDate   DATETIME,
    Reason     TEXT
);

-- Справочники (типы устройств, статусы)
CREATE TABLE Dictionary (
    ID       INTEGER PRIMARY KEY AUTOINCREMENT,
    Category TEXT NOT NULL,   -- DeviceType | DeviceStatus | TicketStatus
    Value    TEXT NOT NULL
);

-- Пользователи
CREATE TABLE Users (
    UserID       INTEGER PRIMARY KEY AUTOINCREMENT,
    Login        TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,         -- SHA-256 в Base64
    FullName     TEXT,
    Role         TEXT DEFAULT 'Teacher' -- Admin | Teacher
);
Стандартные значения справочника
IDКатегорияЗначение1DeviceTypeСистемный блок2DeviceTypeМонитор3DeviceStatusРаботает4DeviceStatusСломан5TicketStatusНовая6TicketStatusВ работе7TicketStatusЗавершена

📊 Отчёты Python
Скрипт Scripts/report_generator.py запускается из C# через Process.Start.
Типы отчётов
bash# Отчёт для закупки (сломанное оборудование)
python report_generator.py purchase "путь/к/inventory.db" "путь/к/папке/Reports"

# Инвентаризационный отчёт
python report_generator.py inventory "путь/к/inventory.db" "путь/к/папке/Reports"

# Статистика по заявкам
python report_generator.py tickets "путь/к/inventory.db" "путь/к/папке/Reports"
Пример содержимого purchase_report.xlsx
Тип устройстваСтатусКоличество% сломанныхУстройстваСистемный блокСломан330%ПК-01, ПК-05, ПК-12МониторСломан110%МН-03

🧪 Юнит-тесты
Проект Inventory.Tests содержит 10 юнит-тестов на xUnit, покрывающих бизнес-логику ядра:
Inventory.Tests/
├── DeviceServiceTests.cs       # Тесты сервиса устройств
├── TicketServiceTests.cs       # Тесты сервиса заявок
└── ...
Запуск тестов
bash# Через .NET CLI
dotnet test

# Через Visual Studio
Test → Run All Tests (Ctrl+R, A)
Покрытие тестами
КомпонентТестыСценарииDeviceService✅Перемещение, обновление статуса, ненайденное устройствоTicketService✅Создание заявки, закрытие, сброс статуса устройстваБизнес-исключения✅BusinessException при невалидных данных

👥 Роли пользователей
ВозможностьAdminTeacherПросмотр устройств и кабинетов✅✅Просмотр заявок✅✅Фильтр «Только мои устройства»—✅Создание заявок✅—Закрытие заявок✅—Перемещение устройств✅—Назначение ответственных✅—Управление кабинетами✅—Управление пользователями✅—Управление справочниками✅—Генерация отчётов✅—

🖼 Скриншоты

Добавьте скриншоты приложения в папку docs/screenshots/ и раскомментируйте блок ниже.

<!--
### Главное окно — плиточный интерфейс
![Главное окно](docs/screenshots/main-window.png)

### Карточка устройства
![Карточка устройства](docs/screenshots/device-card.png)

### Лента заявок
![Лента заявок](docs/screenshots/tickets.png)

### Отчёты
![Отчёты](docs/screenshots/reports.png)
-->

📄 Лицензия
Проект распространяется под лицензией MIT.

<div align="center">
Дипломный проект · Специальность: Разработка программного обеспечения
Разработано с использованием C# · WPF · SQLite · Python
Show Image
</div>
