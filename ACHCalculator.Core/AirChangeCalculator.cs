namespace ACHCalculator.Core;

/// <summary>
/// Расчёт кратностей воздухообмена для чистых помещений (ISO 14644-1 / EU GMP Annex 1).
/// Выделен из AHUCalculator (Services/CleanroomCalculator.cs) в отдельный проект.
/// </summary>
public static class AirChangeCalculator
{
    public record RoomParameters(
        double Area,
        double Height,
        double TargetTemp,
        double TargetRH
    )
    {
        public double Volume => Area * Height;
    }

    public record AirChangeResult(
        double RequiredACH,
        double RequiredFlow,
        double ActualFlow,
        double ActualACH,
        double PressureDiff,
        string IsoClass,
        bool FlowSufficient
    );

    // Синхронизировано с AirHeatBalance (roomCategories.json, нормы 2026-10-26):
    // ISO 5 (GMP A) 240–600; ISO 6–7 60–240; ISO 8 (GMP D) 20–60.
    private static readonly Dictionary<string, (double MinACH, double MaxACH)> IsoAchTable = new()
    {
        ["ISO 3"] = (300, 600),
        ["ISO 4"] = (150, 300),
        ["ISO 5"] = (240, 600),
        ["ISO 6"] = (60, 240),
        ["ISO 7"] = (60, 240),
        ["ISO 8"] = (20, 60),
        ["EU GMP A"] = (240, 600),
        ["EU GMP B"] = (240, 600),
        ["EU GMP C"] = (60, 240),
        ["EU GMP D"] = (20, 60),
    };

    private static readonly Dictionary<string, double> IsoPressureTable = new()
    {
        ["ISO 3"] = 25,
        ["ISO 4"] = 20,
        ["ISO 5"] = 15,
        ["ISO 6"] = 10,
        ["ISO 7"] = 10,
        ["ISO 8"] = 5,
        ["EU GMP A"] = 15,
        ["EU GMP B"] = 15,
        ["EU GMP C"] = 10,
        ["EU GMP D"] = 5,
    };

    private static readonly Dictionary<string, string> IsoEquivalents = new()
    {
        ["EU GMP A"] = "ISO 5",
        ["EU GMP B"] = "ISO 5",
        ["EU GMP C"] = "ISO 7",
        ["EU GMP D"] = "ISO 8",
    };

    public static double GetRequiredACH(string isoClass)
    {
        if (IsoAchTable.TryGetValue(isoClass, out var range))
            return range.MinACH;
        return 10;
    }

    public static (double Min, double Max) GetACHRange(string isoClass)
    {
        if (IsoAchTable.TryGetValue(isoClass, out var range))
            return range;
        return (5, 15);
    }

    public static double GetRecommendedPressure(string isoClass)
    {
        if (IsoPressureTable.TryGetValue(isoClass, out var p))
            return p;
        return 10;
    }

    public static string? GetIsoEquivalent(string gmpClass)
    {
        return IsoEquivalents.GetValueOrDefault(gmpClass);
    }

    public static double CalculateRequiredFlow(RoomParameters room, string isoClass)
    {
        double ach = GetRequiredACH(isoClass);
        return ach * room.Volume;
    }

    public static AirChangeResult Evaluate(RoomParameters room, string isoClass, double actualFlow)
    {
        double requiredACH = GetRequiredACH(isoClass);
        double requiredFlow = requiredACH * room.Volume;
        double actualACH = room.Volume > 0 ? actualFlow / room.Volume : 0;
        double pressureDiff = GetRecommendedPressure(isoClass);

        return new AirChangeResult(
            RequiredACH: requiredACH,
            RequiredFlow: requiredFlow,
            ActualFlow: actualFlow,
            ActualACH: actualACH,
            PressureDiff: pressureDiff,
            IsoClass: isoClass,
            FlowSufficient: actualFlow >= requiredFlow * 0.9
        );
    }

    public static List<string> GetAvailableClasses() => IsoAchTable.Keys.ToList();
}
