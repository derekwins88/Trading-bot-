using SymbolicTrading.Modules;
namespace SymbolicTrading.Engine;

/// <summary>
/// Core reasoning engine responsible for orchestrating modules.
/// </summary>
public class ThinkingEngine
{
    private readonly AxiomRegistry _axioms;
    private readonly MarkovPredictor _predictor;
    private readonly ConflictMediator _mediator;

    public ThinkingEngine(AxiomRegistry axioms,
                          MarkovPredictor predictor,
                          ConflictMediator mediator)
    {
        _axioms = axioms;
        _predictor = predictor;
        _mediator = mediator;
    }

    /// <summary>
    /// Performs a reasoning step given market data and strategy state.
    /// </summary>
    public void ProcessTick(object marketData)
    {
        var prediction = _predictor.PredictNext(marketData);
        var axiom = _axioms.ResolveAxiom(prediction);
        _mediator.HandleConflicts(axiom);
    }
}
