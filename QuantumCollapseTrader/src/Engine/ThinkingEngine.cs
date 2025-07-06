using System;
using NinjaTrader.NinjaScript.Strategies;

namespace SymbolicTrading.Engine
{
    public class ThinkingEngine
    {
        public IObservable<AxiomEvent> AxiomStream { get; } = new EmptyObservable<AxiomEvent>();
        public IObservable<GlyphPhase> GlyphStream { get; } = new EmptyObservable<GlyphPhase>();

        public void OnMarketTick(double entropy, double drift, DateTime time) { }
    }

    public class GlyphPhase
    {
        public string Symbol { get; set; }
    }

    public class AxiomEvent
    {
        public double BoostFactor { get; set; }
    }

    public class EmptyObservable<T> : IObservable<T>
    {
        public IDisposable Subscribe(IObserver<T> observer) => new EmptyDisposable();
        private class EmptyDisposable : IDisposable { public void Dispose() { } }
    }
}
