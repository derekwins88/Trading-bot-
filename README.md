# QuantumCollapseTrader

This repository hosts **QuantumCollapseTrader**, an experimental NinjaTrader strategy that integrates symbolic AI modules. The project is organized as a .NET solution with modular components located under `src/` and tests under `tests/`.

## Structure

```
src/
  Strategies/    # Strategy entry points
  Engine/        # Core orchestration engine
  Modules/       # Individual reasoning components
```

## Modules
- **ThinkingEngine** – orchestrates predictions and axioms
- **AxiomRegistry** – stores and resolves axioms
- **MarkovPredictor** – basic Markov chain market predictor
- **ConflictMediator** – resolves conflicting signals

The `QuantumCollapseTrader` strategy constructs these modules and forwards market data to the `ThinkingEngine` each tick.

## Build

```bash
dotnet build QuantumCollapseTrader.sln
```

Run unit tests with:

```bash
dotnet test QuantumCollapseTrader.sln
```
