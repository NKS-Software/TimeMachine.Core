# TimeMachine.Core

Лёгкая библиотека для управления временем в .NET-приложениях.

## Что внутри

- **SharedTimeProvider** — единая точка получения текущего времени. Позволяет подменять `DateTime.UtcNow` в тестах без изменения кода.
- **Constants** — константы форматов дат (`yyyy-MM-dd`, `yyyy-MM-dd HH:mm:ss`).

## Зачем

В тестах часто нужно «заморозить» время или сдвинуть его. `SharedTimeProvider` позволяет это сделать без `DateTime.UtcNow` в каждом месте.

## Статус

Библиотека используется в [Prometeus.Core](https://github.com/NKS-Software/Prometeus.Core) для тестирования транзакций и счетов.
