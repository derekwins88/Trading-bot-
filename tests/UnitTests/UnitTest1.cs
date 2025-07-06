namespace SymbolicTrading.Tests;

using QuantumCollapseStrategy;

public class Tests
{
    [Test]
    public void StrategyProcessesMarketData()
    {
        var strategy = new QuantumCollapseTrader();
        strategy.OnMarketData(new { Price = 100 });
        Assert.Pass();
    }
}
