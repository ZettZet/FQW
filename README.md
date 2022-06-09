# Микросервисный подход разработки приложений на примере анализа элементов заводнения

Квалификационная работа Кузнецова Фёдора Сергеевича, студента 47 группы направления "Прикладная математика и информатика"

# Структура проекта

## Пара общих слов

У каждого микросервиса в папке есть Dockerfile, позволяющий запускать контейнеры обособленно от всех остальных сервисов

## `envs`

Папка `envs` содержит файлы переменных сред для контейнеров Docker. В основном они предназначены для конфигурации баз данных, а так же предоставления строки подключения для драйверов баз данных.

## `geoprod_adapters`

Решение (solution, .sln) содержащий на момент написания ВКР единственный проект - адаптер для графиков GNUPlot

Потенциально, здесь может появиться проекты адаптеров для любого другого формата, в каком бы ни хотели получить результирующие данные, а позже объеденены с помощью инструмента [Project Tye](https://github.com/dotnet/tye)

Project Tye позволяет в автоматическом режиме интерпретировать каждый из множества проектов одного решения как отдельный микросервис

## `geoprod_com`

Микросервис, занимающийся расчетом компенсации

Зависит от сервисов [расчета жидкости](#geoprod_liq) и [расчета закачки воды](#geoprod_zak)

Написан на языке программирования [Go](https://go.dev/)

Имеет следующие зависимости:
 - [Gin](https://github.com/gin-gonic/gin) - веб фреймворк

## `geoprod_inpoint`

Микросервис, предоставляющий единую точку доступа к API всего приложения

Написан на языке программирования [Python](https://python.org)

Имеет следующие зависимости:
 - [FastAPI](https://github.com/tiangolo/fastapi) - веб фреймворк
 - [loguru](https://github.com/Delgan/loguru) - инструмент логгирования
 - [uvicorn](https://github.com/encode/uvicorn) - асинхронный веб сервер, среда исполнения асинхронного веб фреймворка
 - [requests](https://github.com/psf/requests) - библиотека http запросов

Вследствие использования FastAPI, сервис имеет самособираемую документацию, доступную [по ссылке](http://localhost:80/docs) (по умолчанию: http://localhost:80/docs)

## `geoprod_liq`

Микросервис, ведущий расчет добычи жидкости

Написан на языке программирования [TypeScript](https://www.typescriptlang.org/)

Имеет не стандартный рантайм ([Node.js](https://nodejs.org/)), а аналогичный [Deno](https://deno.land). Вследствие этого, зависимости не помещаются в папку `node_modules` и не берутся из стандартного пакетного менеджера `npm`, а устанавливаются, как явные ссылки на исходный код в домене `deno.land`

Выглядит это примерно следующим образом:

```ts
import { moment } from "https://deno.land/x/deno_ts_moment/mod.ts";
```

Имеет следующие зависимости:
 - [postgres](https://deno.land/x/postgresjs@v3.2.2/mod.js) - драйвер базы данных PostgreSQL
 - [moment](https://deno.land/x/deno_ts_moment@0.0.3/mod.ts) - бибилеотека работы со временем
 - [server](https://deno.land/std@0.139.0/http/server.ts) - http-сервер из стандартной бибилиотеки Deno
 - [serialize](https://deno.land/x/ts_serialize@v2.0.3/mod.ts) - инструмент сериализации и десериализации

## `geoprod_oil`

Микросервис, ведущий расчет добытой нефти

Написан на языке программирования [F#](https://fsharp.org/)

Имеет следующие зависимости:
 - [FSharp.SystemTextJson](https://github.com/Tarmil/FSharp.SystemTextJson) - раширение стандартного модуля обработки JSON с поддердкой типов F#
 - [Giraffe](https://github.com/giraffe-fsharp/Giraffe) - обвязка в функциональном стиле вокруг стандартного роутера ASP.NET
 - [Npgsql](https://github.com/npgsql/npgsql) - официальный драйвер PostgreSQL для .NET
 - [Npgsql.FSharp](https://github.com/Zaid-Ajaj/Npgsql.FSharp) - адаптер для F#
 - [ASP.NET](https://dotnet.microsoft.com/en-us/apps/aspnet) - веб фреймворк

## `geoprod_zak`

Микросервис, ведущий расчет закачки воды в пласт

Написан на языке программирования [Python](https://python.org)

Имеет следующие зависимости:
 - [FastAPI](https://github.com/tiangolo/fastapi) - веб фреймворк
 - [uvicorn](https://github.com/encode/uvicorn) - асинхронный веб сервер, среда исполнения асинхронного веб фреймворка
 - [sqlmodel](https://github.com/tiangolo/sqlmodel) - ORM на основе SQLAlchemy и Pydantic
 - [SQLAlchemy](https://github.com/sqlalchemy/sqlalchemy) - tool kit для работы с реляционными базами данных
 - [psycopg2-binary](https://www.psycopg.org/) - собранный бинарный пакет psycopg2, адаптера PostgreSQL для Python

## `init_{microservice_name}`

Каждая из папок, подчиняющаяся такому паттерну названия, содерджит в себе базовые SQL скрипты создания таблиц и вставки данных для каждого определенного микросервиса

## `docker-compose.yml`

Файл-инструкция по сборке ~~всего этого безобразия~~ каждого микросервиса в составе приложения
