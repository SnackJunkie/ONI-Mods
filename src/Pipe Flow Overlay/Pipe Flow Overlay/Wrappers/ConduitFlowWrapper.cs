namespace PipeFlowOverlay.Wrappers
{
    internal class ConduitFlowWrapper : IConduitFlowWrapper
    {
        private readonly ConduitFlow _conduitFlow;

        public ConduitFlowWrapper(ConduitFlow conduitFlow)
        {
            _conduitFlow = conduitFlow;
        }

        public ConduitType ConduitType => _conduitFlow.conduitType;

        public event System.Action OnConduitsRebuilt
        {
            add => _conduitFlow.onConduitsRebuilt += value;
            remove => _conduitFlow.onConduitsRebuilt -= value;
        }

        public string GetFlow(int cell)
        {
            return _conduitFlow.GetPermittedFlow(cell).ToString();
        }
    }
}
