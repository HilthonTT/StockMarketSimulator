using Modules.Stocks.Domain.Services;

namespace Domain.UnitTests.Stocks;

public sealed class GeometricBrownianMotionTests
{
    private const double OneSecond = 1 / GeometricBrownianMotion.TradingSecondsPerDay;

    [Fact]
    public void NextPrice_ShouldOnlyApplyDrift_WhenShockIsZero()
    {
        // With Z = 0 and sigma = 0 the step is exp(mu * dt) - 1.
        decimal next = GeometricBrownianMotion.NextPrice(100m, 0.01, 0, stepInTradingDays: 1, maxStepChange: 1, standardNormal: 0);

        Assert.Equal(Math.Round(100m * (decimal)Math.Exp(0.01), 2), next);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(-10)]
    public void NextPrice_ShouldClampStepToMaxChange(double extremeShock)
    {
        decimal next = GeometricBrownianMotion.NextPrice(100m, 0, 0.5, OneSecond * 60, maxStepChange: 0.002, extremeShock);

        Assert.InRange(next, 99.80m, 100.20m);
    }

    [Fact]
    public void NextPrice_ShouldNeverReturnANonPositivePrice()
    {
        decimal next = GeometricBrownianMotion.NextPrice(0.01m, 0, 5, 1, maxStepChange: 0.99, standardNormal: -50);

        Assert.True(next > 0);
    }

    [Fact]
    public void SampleStandardNormal_ShouldHaveRoughlyZeroMeanAndUnitVariance()
    {
        var random = new Random(42);
        double[] samples = [.. Enumerable.Range(0, 20_000).Select(_ => GeometricBrownianMotion.SampleStandardNormal(random))];

        double mean = samples.Average();
        double variance = samples.Select(x => (x - mean) * (x - mean)).Average();

        Assert.InRange(mean, -0.05, 0.05);
        Assert.InRange(variance, 0.9, 1.1);
    }
}
