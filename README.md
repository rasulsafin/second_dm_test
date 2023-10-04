# Описание

> **Система управления документами**, СУД, DMS (англ. Document management system) — компьютерная система (или набор компьютерных программ), используемая для отслеживания и хранения электронных документов и/или образов (изображений и иных артефактов) бумажных документов.

Платформа **BRIO Mixed Reality System** визуализирует объекты цифрового мира, встраивая их в реальную физическую обстановку без пространственных нарушений. BRIO MRS предоставляет инструменты работы с цифровыми моделями здания и инженерных систем, непосредственно на строительной площадке в режиме реального времени. 

Основная задача проекта **BRIO MRS Document Management**, это реализовать хранение, обработку и преобразование данных, для эффективного управления процессами проектирования, строительства, эксплуатации и ликвидации объектов на всех стадиях жизненного цикла. В связи с тем, что кроме локального документооборота, BRIO MRS DM представляет пользователю возможность интеграции с другими системами, API которых не всегда поддерживается в рамках разработки на Unity, было решено вынести бизнес-логику в отдельный проект. Общение между клиентом (Unity) и сервером (.Net 5) происходит с помощью http-запросов. 

# Структура решения
## Разделяемые библиотеки 

| Название | Описание |
| ---  | ---     |
| Brio.Docs._Common_ | Общие интерфейсы, модели и enums, использующиеся как в интеграциях с внешними _СУС_, так и в работе с _Unity_.|
| Brio.Docs._Client_ | Проект содержит классы и интерфейсы, которые используются как на стороне _Unity_, так и на стороне _DM_. Позволяют скрыть слой передачи данных. В проекте определяются _Data Transfer Objects (Dtos)_, с которыми данные интерфейсы и работают. |
| Brio.Docs._Integration_ | Интерфейсы и классы для работы с внешней системой документооборота. Используются для интеграции _BRIO MRS DM_ с системами управления строительством. Интерфейсы для внешних подключений используют свой тип _Dto_. |
| Brio.Docs._External_ | Общие классы для работы с внешними системами документооборота и облачными сервисами. Служит для упрощения создания новых реализаций подключений. |

## Основные проекты

| Название | Описание |
| ---  | ---     |
| Brio.Docs._Api_ | Уровень принятия и обработки http-запросов выделен в отдельный проект для возможной безболезненной замены протокола. Принимает и обрабатывает запросы, приходящие от клиента (_Unity_).|
| Brio.Docs | Основные сервисы для работы с документооборотом. В данном проекте реализуются основные интерфейсы (_Brio.Docs.Client_), преобразовываются данные из _Dto_ в модели базы данных и формируются запросы к базе данных с использованием _Entity Framework (EF) Core_. |
| Brio.Docs._Database_ | Уровень базы данных. Хранит миграции, контекст базы данных и модели базы данных. Модели БД нужны для хранения данных в приемлемом для базы данных формате. Обычно такие модели содержат в себе внешние ключи, мосты для связей многие-ко-многим, разложение списков данных на составляющие. |
| Brio.Docs._Synchronizer_ | Синхронизатор. Отвечает за синхронизацию данных локальной базы данных с данными из внешнего документооборота на момент синхронизации.  |

## Внешние подключения

| Название | Описание |
| ---  | ---     |
| Brio.Docs.Connections._BIM360_ | Интеграция с системой управления строительством [BIM360](https://www.autodesk.com/bim-360/)  |
| Brio.Docs.Connections._GoogleDrive_ | Интеграция с файловой системой [GoogleDrive](https://www.google.com/intl/ru_ru/drive/) |
| Brio.Docs.Connections._LementPro_ | Интеграция с системой управления строительством [LementPro](https://www.lement.pro/ru/) |
| Brio.Docs.Connections._Tdms_ | Интеграция с системой управления строительством [Tdms](https://tdms.ru/) |
| Brio.Docs.Connections._YandexDisk_ | Интеграция с с файловой системой [YandexDisk](https://disk.yandex.ru/) |
| Brio.Docs.Connections._MrsPro_ | Интеграция с системой управления строительством [MrsPro](https://mrspro.ru/solutions/strojkontrol/) |

## Тесты

Все тесты расположены в отдельной папке с названием Tests. 

# Интеграция со сторонней системой документооборота
## Реализация

Для реализации интеграции сторонней системы с системой Brio.Docs необходимо создать проект **Brio.Docs.Connections.НазваниеСистемы** в папке Connections со следующими свойствами (версия x.x.x ставится текущая) и начальными зависимостями: 
```xml
 <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
    <RootNamespace>Brio.Docs.Connections.НазваниеСистемы</RootNamespace>
    <Authors>Brio MRS</Authors>
    <Product>Brio MRS Connection</Product>
    <Company>Brio MRS©</Company>
    <AssemblyVersion>X.X.X.X</AssemblyVersion>
    <Version>X.X.X</Version>
 </PropertyGroup>

<ItemGroup>
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="5.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="5.0.0" />
    <PackageReference Include="Newtonsoft.Json" Version="13.0.1" />
    <PackageReference Include="Serilog" Version="2.10.0" />
  </ItemGroup>

 <ItemGroup>
    <ProjectReference Include="..\Brio.Docs.Common\Brio.Docs.Common.csproj" />
    <ProjectReference Include="..\Brio.Docs.Integration\Brio.Docs.Integration.csproj" />
    <ProjectReference Include="..\Brio.Docs.External\Brio.Docs.External.csproj" />
  </ItemGroup>
```

  В проекте **Brio.Docs.Connections.НазваниеСистемы** необходимо реализовать следующие интерфейсы: 

- _IConnectionMeta_ (используется для создания объекта стороннего подключения);
- _IConnection_ (используется для постановления первичного подключения, отвечает за установление связи со сторонним api и создание нужных объектов для работы с ним);
- _IConnectionStorage_ (используется для загрузки\скачивания\удаления физических файлов в сторонней системе);

Для поддержки **синхронизации** данных, необходимо реализовать: 

- _AConnectionContext_ (Абстрактный класс, предоставляющий доступ с объектам синхронизации);
- _ISynchronizer\<ObjectiveExternalDto>_ (является прослойкой для работы с **Объектами Задач** между MRS.DM и сторонней системой);
- _ISynchronizer\<ProjectExternalDto>_ (является прослойкой для работы с **Проектами** между MRS.DM и сторонней системой);

Добавить статический класс расширение _НазваниеСистемыServiceCollectionExtensions_ с методом

```c#
public static IServiceCollection AddНазваниеСистемы(this IServiceCollection services)
{
        services.AddScoped<НазваниеСистемыConnection>();
        return services;
}
``` 

для работы с DI и прописать в нем добавление реализации _IConnection_ нужного типа (Scoped, Singleton, Transient). По стандартам Microsoft класс должен находится в неймспейсе _Microsoft.Extensions.DependencyInjection_.

Любой проект интеграции обязан быть покрыт тестами. Проект с тестами создается в папке Tests и называется **Brio.Docs.Connections.НазваниеСистемы.Tests**. В тестовом проекте так же стоит прописать namespace и текущую версию:
```xml
 <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <RootNamespace>Brio.Docs.Connections.НазваниеСистемы.Tests</RootNamespace>
    <Version>x.x.x</Version>
  </PropertyGroup>
```

## Регистрация внешнего подключения в MRS и первая синхронизация данных

Для работы с _интеграцией\внешним подключением_ в тестовом режиме, необходимо создать тестового пользователя и привязать к нему интеграцию. 

Необходимо: 

1. Зарегистрировать интеграцию командой: `GET /ConnectionTypes/register`
2. Если все было сделано правильно, то команда `GET /ConnectionTypes` выдаст список возможных подключений, в котором будет находится новая интеграция. Для дальнейших действий необходимо запомнить _{ConnectionTypeId}_ только что созданого типа внешнего подключения;
3. Создать пользователя командой `POST /Users` и контентом вида
    ```json
    {
        "login": "наименованиесистемы",
        "password": "123",
        "name": "Фамилия Имя"
    }
    ```
    Где _**наименованиесистемы**_ это имя интеграции, к которой будет привязан пользователь. Написанное маленькими буквами, слитно. **_Фамилию и Имя_** пользователя необходимо заменить на настоящие Имя и Фамилию, чтобы во время демонстрации функционала в тестовом режиме интеграция выглядела презентабельно. Всем тестовым пользователям изначально присваивается **_пароль_** 123, чтобы его было легко запомнить и просто вводить во время ручного тестирования.

    Если пользователь создался успешно, то нужно запомнить его _{User.Id}_
4. Создать подключение командой `POST /Connections` и контентом
    ```json
        {
            "connectionTypeID": { "id" : ConnectionTypeId },
            "userID": { "id" : UserId },
            "authFieldValues": {
                "password": "passwordValue",
                "login": "loginValue"
            }
        }
    ```
    Где **_ConnectionTypeId_** это тип внешнего подключения, а **_UserId_** идентификатор пользователя. Значения **_authFieldValues_** могут разниться в зависимости от внешнего подключения.
5. Командой `GET /Connections/connect/{UserId}` необходимо подключиться к внешней системе.
6. Командой `GET /Connections/synchronization/{UserId}` провести первую синхронизацию.