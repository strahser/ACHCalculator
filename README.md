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

## Синхронизация с AirHeatBalance

Нормативные кратности приведены к общей базе воздушно-теплового баланса
(`AirHeatBalance/Core/Data/roomCategories.json`, нормы 2026-10-26):
ISO 5 (GMP A) — 240–600, ISO 6–7 — 60–240, ISO 8 (GMP D) — 20–60.
GMP A/B = ISO 5, GMP C = ISO 7, GMP D = ISO 8.
Источник: `ProjectsPalns/AirHeatBalance/_reference/2026-10-26_ventilation-lighting-norms.md`.

## Сборка и тесты

```
dotnet build ACHCalculator.slnx
dotnet test ACHCalculator.Tests
dotnet run --project ACHCalculator.CLI
```
