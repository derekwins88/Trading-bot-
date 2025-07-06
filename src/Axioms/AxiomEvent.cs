namespace SymbolicTrading.Axioms
{
    public class AxiomEvent
    {
        public string Id { get; }
        public string? Data { get; }

        public AxiomEvent(string id, string? data = null)
        {
            Id = id;
            Data = data;
        }
    }
}
