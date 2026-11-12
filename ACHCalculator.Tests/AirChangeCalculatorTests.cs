using ACHCalculator.Core;
using NUnit.Framework;

namespace ACHCalculator.Tests;

[TestFixture]
public class AirChangeCalculatorTests
{
    [Test]
    public void GetRequiredACH_ISO7_Returns60()
    {
        double ach = AirChangeCalculator.GetRequiredACH("ISO 7");
        Assert.That(ach, Is.EqualTo(60));
    }

    [Test]
    public void GetRequiredACH_ISO6_Returns60()
    {
        double ach = AirChangeCalculator.GetRequiredACH("ISO 6");
        Assert.That(ach, Is.EqualTo(60));
    }

    [Test]
    public void GetRequiredACH_ISO8_Returns20()
    {
        double ach = AirChangeCalculator.GetRequiredACH("ISO 8");
        Assert.That(ach, Is.EqualTo(20));
    }

    [Test]
    public void GetRequiredACH_ISO5_Returns240()
    {
        double ach = AirChangeCalculator.GetRequiredACH("ISO 5");
        Assert.That(ach, Is.EqualTo(240));
    }

    [Test]
    public void GetRequiredACH_ISO3_Returns300()
    {
        double ach = AirChangeCalculator.GetRequiredACH("ISO 3");
        Assert.That(ach, Is.EqualTo(300));
    }

    [Test]
    public void GetRequiredACH_Unknown_Returns10()
    {
        double ach = AirChangeCalculator.GetRequiredACH("ISO 99");
        Assert.That(ach, Is.EqualTo(10));
    }

    [Test]
    public void GetRecommendedPressure_ISO7_Returns10()
    {
        double p = AirChangeCalculator.GetRecommendedPressure("ISO 7");
        Assert.That(p, Is.EqualTo(10));
    }

    [Test]
    public void GetRecommendedPressure_ISO3_Returns25()
    {
        double p = AirChangeCalculator.GetRecommendedPressure("ISO 3");
        Assert.That(p, Is.EqualTo(25));
    }

    [Test]
    public void GetRecommendedPressure_Unknown_Returns10()
    {
        double p = AirChangeCalculator.GetRecommendedPressure("ISO 99");
        Assert.That(p, Is.EqualTo(10));
    }

    [Test]
    public void CalculateRequiredFlow_ISO7_100sqm_3m_Returns18000()
    {
        var room = new AirChangeCalculator.RoomParameters(100, 3.0, 20, 50);
        double flow = AirChangeCalculator.CalculateRequiredFlow(room, "ISO 7");
        // 60 ACH * 300 m³ = 18 000 m³/h
        Assert.That(flow, Is.EqualTo(18000).Within(1));
    }

    [Test]
    public void CalculateRequiredFlow_ISO5_50sqm_3m_Returns36000()
    {
        var room = new AirChangeCalculator.RoomParameters(50, 3.0, 20, 50);
        double flow = AirChangeCalculator.CalculateRequiredFlow(room, "ISO 5");
        // 240 ACH * 150 m³ = 36 000 m³/h
        Assert.That(flow, Is.EqualTo(36000).Within(1));
    }

    [Test]
    public void RoomParameters_Volume_CalculatesCorrectly()
    {
        var room = new AirChangeCalculator.RoomParameters(100, 3.5, 20, 50);
        Assert.That(room.Volume, Is.EqualTo(350).Within(0.01));
    }

    [Test]
    public void Evaluate_SufficientFlow_ReturnsTrue()
    {
        var room = new AirChangeCalculator.RoomParameters(100, 3.0, 20, 50);
        // ISO 7: 60 ACH * 300 m³ = 18 000 m³/h required
        var result = AirChangeCalculator.Evaluate(room, "ISO 7", 20000);

        Assert.That(result.FlowSufficient, Is.True);
        Assert.That(result.RequiredFlow, Is.EqualTo(18000).Within(1));
        Assert.That(result.ActualFlow, Is.EqualTo(20000));
        Assert.That(result.ActualACH, Is.EqualTo(20000.0 / 300).Within(0.1));
    }

    [Test]
    public void Evaluate_InsufficientFlow_ReturnsFalse()
    {
        var room = new AirChangeCalculator.RoomParameters(100, 3.0, 20, 50);
        var result = AirChangeCalculator.Evaluate(room, "ISO 7", 3000);

        Assert.That(result.FlowSufficient, Is.False);
        Assert.That(result.RequiredFlow, Is.EqualTo(18000).Within(1));
    }

    [Test]
    public void Evaluate_BorderlineFlow_Within10Percent_ReturnsTrue()
    {
        var room = new AirChangeCalculator.RoomParameters(100, 3.0, 20, 50);
        // 90% от 18 000 = 16 200
        var result = AirChangeCalculator.Evaluate(room, "ISO 7", 16200);

        Assert.That(result.FlowSufficient, Is.True,
            "Запас 10% — расход 16 200 >= 16 200 считается достаточным");
    }

    [Test]
    public void Evaluate_PressureDiff_MatchesIsoClass()
    {
        var room = new AirChangeCalculator.RoomParameters(100, 3.0, 20, 50);

        var iso7 = AirChangeCalculator.Evaluate(room, "ISO 7", 5000);
        Assert.That(iso7.PressureDiff, Is.EqualTo(10));

        var iso5 = AirChangeCalculator.Evaluate(room, "ISO 5", 20000);
        Assert.That(iso5.PressureDiff, Is.EqualTo(15));

        var iso3 = AirChangeCalculator.Evaluate(room, "ISO 3", 100000);
        Assert.That(iso3.PressureDiff, Is.EqualTo(25));
    }

    [Test]
    public void Evaluate_IsoClass_Preserved()
    {
        var room = new AirChangeCalculator.RoomParameters(100, 3.0, 20, 50);
        var result = AirChangeCalculator.Evaluate(room, "ISO 7", 5000);
        Assert.That(result.IsoClass, Is.EqualTo("ISO 7"));
    }

    [Test]
    public void GetAvailableClasses_Returns10Classes()
    {
        var classes = AirChangeCalculator.GetAvailableClasses();
        Assert.That(classes, Has.Count.EqualTo(10));
        Assert.That(classes, Does.Contain("ISO 3"));
        Assert.That(classes, Does.Contain("ISO 7"));
        Assert.That(classes, Does.Contain("EU GMP A"));
        Assert.That(classes, Does.Contain("EU GMP B"));
        Assert.That(classes, Does.Contain("EU GMP C"));
        Assert.That(classes, Does.Contain("EU GMP D"));
    }

    [Test]
    public void GetRequiredACH_EUGMPA_Returns240()
    {
        double ach = AirChangeCalculator.GetRequiredACH("EU GMP A");
        Assert.That(ach, Is.EqualTo(240));
    }

    [Test]
    public void GetRequiredACH_EUGMPB_Returns240()
    {
        double ach = AirChangeCalculator.GetRequiredACH("EU GMP B");
        Assert.That(ach, Is.EqualTo(240));
    }

    [Test]
    public void GetRequiredACH_EUGMPC_Returns60()
    {
        double ach = AirChangeCalculator.GetRequiredACH("EU GMP C");
        Assert.That(ach, Is.EqualTo(60));
    }

    [Test]
    public void GetRequiredACH_EUGMPD_Returns20()
    {
        double ach = AirChangeCalculator.GetRequiredACH("EU GMP D");
        Assert.That(ach, Is.EqualTo(20));
    }

    [Test]
    public void GetRecommendedPressure_EUGMPA_Returns15()
    {
        double p = AirChangeCalculator.GetRecommendedPressure("EU GMP A");
        Assert.That(p, Is.EqualTo(15));
    }

    [Test]
    public void GetRecommendedPressure_EUGMPB_Returns15()
    {
        double p = AirChangeCalculator.GetRecommendedPressure("EU GMP B");
        Assert.That(p, Is.EqualTo(15));
    }

    [Test]
    public void GetRecommendedPressure_EUGMPC_Returns10()
    {
        double p = AirChangeCalculator.GetRecommendedPressure("EU GMP C");
        Assert.That(p, Is.EqualTo(10));
    }

    [Test]
    public void GetRecommendedPressure_EUGMPD_Returns5()
    {
        double p = AirChangeCalculator.GetRecommendedPressure("EU GMP D");
        Assert.That(p, Is.EqualTo(5));
    }

    [Test]
    public void GetIsoEquivalent_EUGMPA_ReturnsISO5()
    {
        var eq = AirChangeCalculator.GetIsoEquivalent("EU GMP A");
        Assert.That(eq, Is.EqualTo("ISO 5"));
    }

    [Test]
    public void GetIsoEquivalent_EUGMPB_ReturnsISO5()
    {
        var eq = AirChangeCalculator.GetIsoEquivalent("EU GMP B");
        Assert.That(eq, Is.EqualTo("ISO 5"));
    }

    [Test]
    public void GetIsoEquivalent_EUGMPC_ReturnsISO7()
    {
        var eq = AirChangeCalculator.GetIsoEquivalent("EU GMP C");
        Assert.That(eq, Is.EqualTo("ISO 7"));
    }

    [Test]
    public void GetIsoEquivalent_EUGMPD_ReturnsISO8()
    {
        var eq = AirChangeCalculator.GetIsoEquivalent("EU GMP D");
        Assert.That(eq, Is.EqualTo("ISO 8"));
    }

    [Test]
    public void GetIsoEquivalent_Unknown_ReturnsNull()
    {
        var eq = AirChangeCalculator.GetIsoEquivalent("ISO 7");
        Assert.That(eq, Is.Null);
    }

    [Test]
    public void GetACHRange_EUGMPB_Returns240_600()
    {
        var (min, max) = AirChangeCalculator.GetACHRange("EU GMP B");
        Assert.That(min, Is.EqualTo(240));
        Assert.That(max, Is.EqualTo(600));
    }

    [Test]
    public void Evaluate_EUGMPA_SufficientFlow()
    {
        var room = new AirChangeCalculator.RoomParameters(100, 3.0, 20, 50);
        // EU GMP A: 240 ACH * 300 m³ = 72 000 m³/h
        var result = AirChangeCalculator.Evaluate(room, "EU GMP A", 80000);
        Assert.That(result.FlowSufficient, Is.True);
        Assert.That(result.RequiredFlow, Is.EqualTo(72000).Within(1));
        Assert.That(result.PressureDiff, Is.EqualTo(15));
    }

    [Test]
    public void Evaluate_EUGMPD_SufficientFlow()
    {
        var room = new AirChangeCalculator.RoomParameters(100, 3.0, 20, 50);
        // EU GMP D: 20 ACH * 300 m³ = 6000 m³/h
        var result = AirChangeCalculator.Evaluate(room, "EU GMP D", 7000);
        Assert.That(result.FlowSufficient, Is.True);
        Assert.That(result.RequiredFlow, Is.EqualTo(6000).Within(1));
    }

    [Test]
    public void Evaluate_ZeroVolume_DoesNotThrow()
    {
        var room = new AirChangeCalculator.RoomParameters(0, 0, 20, 50);
        Assert.DoesNotThrow(() =>
        {
            var result = AirChangeCalculator.Evaluate(room, "ISO 7", 0);
            Assert.That(result.ActualACH, Is.EqualTo(0));
        });
    }
}
