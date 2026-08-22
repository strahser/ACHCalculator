using ACHCalculator.Core;

Console.OutputEncoding = System.Text.Encoding.UTF8;

void SafeClear()
{
    try { Console.Clear(); } catch { Console.WriteLine("\n---\n"); }
}

while (true)
{
    SafeClear();
    Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
    Console.WriteLine("║      ACH Calculator — Кратности воздухообмена (ISO/GMP)     ║");
    Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
    Console.WriteLine();
    Console.WriteLine("  1. Расчёт кратности воздухообмена");
    Console.WriteLine("  2. Справка по классам чистоты");
    Console.WriteLine("  0. Выход");
    Console.WriteLine();
    Console.Write("Выберите действие: ");

    var choice = Console.ReadLine()?.Trim();

    switch (choice)
    {
        case "1":
            RunAirChangeCalculator();
            break;
        case "2":
            ShowClassesReference();
            break;
        case "0":
            return;
        default:
            Console.WriteLine("Неверный выбор. Нажмите любую клавишу...");
            Console.ReadKey();
            break;
    }
}

void RunAirChangeCalculator()
{
    SafeClear();
    Console.WriteLine("═══ Расчёт кратности воздухообмена ═══\n");

    var classes = AirChangeCalculator.GetAvailableClasses();
    Console.WriteLine("Доступные классы чистоты:");
    for (int i = 0; i < classes.Count; i++)
    {
        var eq = AirChangeCalculator.GetIsoEquivalent(classes[i]);
        var eqStr = eq != null ? $" ({eq})" : "";
        Console.WriteLine($"  {i + 1,2}. {classes[i]}{eqStr}");
    }
    Console.Write("\nВыберите класс (номер или название): ");
    var classInput = Console.ReadLine()?.Trim() ?? "";

    string selectedClass;
    if (int.TryParse(classInput, out int classIdx) && classIdx >= 1 && classIdx <= classes.Count)
        selectedClass = classes[classIdx - 1];
    else if (classes.Contains(classInput))
        selectedClass = classInput;
    else
    {
        Console.WriteLine("Неверный класс. Нажмите любую клавишу...");
        Console.ReadKey();
        return;
    }

    Console.Write("Площадь помещения (м²): ");
    if (!double.TryParse(Console.ReadLine(), out double area) || area <= 0)
    {
        Console.WriteLine("Неверное значение. Нажмите любую клавишу...");
        Console.ReadKey();
        return;
    }

    Console.Write("Высота потолка (м): ");
    if (!double.TryParse(Console.ReadLine(), out double height) || height <= 0)
    {
        Console.WriteLine("Неверное значение. Нажмите любую клавишу...");
        Console.ReadKey();
        return;
    }

    var room = new AirChangeCalculator.RoomParameters(area, height, 20, 50);
    var achRange = AirChangeCalculator.GetACHRange(selectedClass);
    var requiredFlow = AirChangeCalculator.CalculateRequiredFlow(room, selectedClass);
    var pressure = AirChangeCalculator.GetRecommendedPressure(selectedClass);
    var isoEq = AirChangeCalculator.GetIsoEquivalent(selectedClass);

    SafeClear();
    Console.WriteLine($"═══ Результаты расчёта: {selectedClass} ═══\n");
    if (isoEq != null)
        Console.WriteLine($"  Эквивалент ISO:     {isoEq}");
    Console.WriteLine($"  Площадь:            {area:F1} м²");
    Console.WriteLine($"  Высота:             {height:F2} м");
    Console.WriteLine($"  Объём:              {room.Volume:F1} м³");
    Console.WriteLine();
    Console.WriteLine($"  Кратность:          {achRange.Min:F0}–{achRange.Max:F0} раз/час");
    Console.WriteLine($"  Требуемый расход:   {requiredFlow:F0} м³/ч");
    Console.WriteLine($"  Избыточное давление: {pressure:F0} Па");
    Console.WriteLine();

    Console.Write("Фактический расход воздуха (м³/ч) [Enter = пропустить]: ");
    var flowInput = Console.ReadLine()?.Trim();
    if (!string.IsNullOrEmpty(flowInput) && double.TryParse(flowInput, out double actualFlow))
    {
        var result = AirChangeCalculator.Evaluate(room, selectedClass, actualFlow);
        Console.WriteLine();
        Console.WriteLine($"  Факт. расход:       {result.ActualFlow:F0} м³/ч");
        Console.WriteLine($"  Факт. кратность:    {result.ActualACH:F1} раз/час");
        Console.WriteLine($"  Достаточность:      {(result.FlowSufficient ? "✓ Достаточно" : "✗ НЕДОСТАТОЧНО")}");
        if (!result.FlowSufficient)
            Console.WriteLine($"  Необходимо добавить: {result.RequiredFlow - result.ActualFlow:F0} м³/ч");
    }

    Console.WriteLine("\nНажмите любую клавишу...");
    Console.ReadKey();
}

void ShowClassesReference()
{
    SafeClear();
    Console.WriteLine("═══ Справка по классам чистоты ═══\n");

    Console.WriteLine("  ISO 14644-1:");
    Console.WriteLine("  ┌─────────┬──────────────────┬──────────┬──────────┐");
    Console.WriteLine("  │ Класс   │ Частиц/м³ (≥0.5) │ Крат/ч   │ ΔP (Па)  │");
    Console.WriteLine("  ├─────────┼──────────────────┼──────────┼──────────┤");
    Console.WriteLine("  │ ISO 3   │ 35               │ 300–600  │ 25       │");
    Console.WriteLine("  │ ISO 4   │ 352              │ 150–300  │ 20       │");
    Console.WriteLine("  │ ISO 5   │ 3 520            │ 60–150   │ 15       │");
    Console.WriteLine("  │ ISO 6   │ 35 200           │ 30–60    │ 15       │");
    Console.WriteLine("  │ ISO 7   │ 352 000          │ 15–30    │ 10       │");
    Console.WriteLine("  │ ISO 8   │ 3 520 000        │ 5–15     │ 10       │");
    Console.WriteLine("  └─────────┴──────────────────┴──────────┴──────────┘");

    Console.WriteLine();
    Console.WriteLine("  EU GMP Annex 1:");
    Console.WriteLine("  ┌──────────┬──────────────────┬──────────┬──────────┬────────────┐");
    Console.WriteLine("  │ Класс    │ Частиц/м³ (≥0.5) │ Крат/ч   │ ΔP (Па)  │ ISO экв.   │");
    Console.WriteLine("  ├──────────┼──────────────────┼──────────┼──────────┼────────────┤");
    Console.WriteLine("  │ Grade A  │ 3 520            │ 150–300  │ 20       │ ISO 4-5    │");
    Console.WriteLine("  │ Grade B  │ 3 520            │ 60–150   │ 15       │ ISO 5      │");
    Console.WriteLine("  │ Grade C  │ 352 000          │ 15–30    │ 10       │ ISO 7      │");
    Console.WriteLine("  │ Grade D  │ 3 520 000        │ 5–15     │ 10       │ ISO 8      │");
    Console.WriteLine("  └──────────┴──────────────────┴──────────┴──────────┴────────────┘");

    Console.WriteLine("\nНажмите любую клавишу...");
    Console.ReadKey();
}
