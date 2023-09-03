namespace PipeFlowOverlay.Wrappers
{
    internal interface IConduitFlowWrapper
    {
        event System.Action OnConduitsRebuilt;
        ConduitType ConduitType { get; }
        string GetFlow(int cell);
    }
}
