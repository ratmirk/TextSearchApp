# TextSearchApp

### Пример простого API сервиса c поиском по текстам документов с примером использования Elasticsearch
###
### Проекты:

| Проект                 | Описание                |
|------------------------|-------------------------|
| TextSearchApp.Data     | DB и Elasticsearch      |
| TextSearchApp.Host     | Основной проект сервиса | 
| TextSearchApp.Tests    | Тесты                   | 

### Запуск Elasticsearch
Для запуска Elasticsearch нужен Docker
использовать файл docker-compose.yml

### Seed Db
Настройка, запускающая заполнение БД из csv файла - commandLineArgs в launchSettings.json.
Если там значение "seed", будет запуск сервиса с подготовительным этапом заполнения БД и индексации в Elasticsearch,
в противном случае будет просто старт хоста.

