# ACHCalculator — кратности воздухообмена

Отдельный проект расчёта кратностей воздухообмена для чистых помещений
(ISO 14644-1 / EU GMP Annex 1). Выделен из `AHUCalculator` по решению владельца
от 2026-08-22 (план: ProjectsPalns, `AHUCalculator/_current/2026-08-22_supply-exhaust-link-report-ui.md`, этап 7).

## Структура

- `ACHCalculator.Core` — библиотека `AirChangeCalculator`: таблицы кратностей и
  избыточного давления по классам (ISO 3–8, EU GMP A–D), ISO-эквиваленты,
  `Evaluate` (достаточность фактического расхода), `CalculateRequiredFlow`.
- `ACHCalculator.CLI` — интерактивный консольный калькулятор + справка по классам.
- `ACHCalculator.Tests` — NUnit, порт тестов `CleanroomCalculatorTests` (30 шт.).

## Сборка и тесты

```
dotnet build ACHCalculator.slnx
dotnet test ACHCalculator.Tests
dotnet run --project ACHCalculator.CLI
```
