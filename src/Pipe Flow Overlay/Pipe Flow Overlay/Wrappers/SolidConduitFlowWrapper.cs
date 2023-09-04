using System.Collections.Concurrent;

namespace PipeFlowOverlay.Wrappers
{
    internal class SolidConduitFlowWrapper : IConduitFlowWrapper
    {
        internal static SolidConduitFlowWrapper Instance = new SolidConduitFlowWrapper();
        internal static ConcurrentDictionary<int, ConduitFlow.FlowDirections> FlowDirections = new ConcurrentDictionary<int, ConduitFlow.FlowDirections>();

        public ConduitType ConduitType => ConduitType.Solid;

        public event System.Action OnConduitsRebuilt
        {
            add => Game.Instance.solidConduitFlow.onConduitsRebuilt += value;
            remove => Game.Instance.solidConduitFlow.onConduitsRebuilt -= value;
        }

        public string GetFlow(int cell)
        {
            SolidConduitFlow.Conduit conduit = Game.Instance.solidConduitFlow.GetConduit(cell);
            if (!FlowDirections.TryGetValue(conduit.idx, out ConduitFlow.FlowDirections flowDirections))
                return "none";

            ConduitFlow.FlowDirections src = GetFlowDirections(Game.Instance.solidConduitFlow.GetSOAInfo().GetSrcFlowDirection(conduit.idx));
            if (flowDirections == src)
                return "none";

            return flowDirections.ToString();
        }

        public static ConduitFlow.FlowDirections GetFlowDirections(SolidConduitFlow.FlowDirection flowDirection)
        {
            switch (flowDirection)
            {
                case SolidConduitFlow.FlowDirection.Up:
                    return ConduitFlow.FlowDirections.Up;
                case SolidConduitFlow.FlowDirection.Down:
                    return ConduitFlow.FlowDirections.Down;
                case SolidConduitFlow.FlowDirection.Left:
                    return ConduitFlow.FlowDirections.Left;
                case SolidConduitFlow.FlowDirection.Right:
                    return ConduitFlow.FlowDirections.Right;
                default:
                    return ConduitFlow.FlowDirections.None;
            }
        }
    }
}
