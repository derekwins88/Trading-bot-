using SymbolicTrading.Engine;
using SymbolicTrading.Modules;

namespace QuantumCollapseStrategy;

/// <summary>
/// NinjaTrader strategy integrating symbolic AI modules.
/// </summary>
public class QuantumCollapseTrader
{
    private readonly ThinkingEngine _engine;

    public QuantumCollapseTrader()
    {
        // Construct engine with core modules
        var registry = new AxiomRegistry();
        var predictor = new MarkovPredictor();
        var mediator = new ConflictMediator();
        _engine = new ThinkingEngine(registry, predictor, mediator);
    }

    /// <summary>
    /// Entry point for each market tick.
    /// </summary>
    public void OnMarketData(object marketData)
    {
        // Forward tick data into the symbolic reasoning engine
        _engine.ProcessTick(marketData);
    }
}
