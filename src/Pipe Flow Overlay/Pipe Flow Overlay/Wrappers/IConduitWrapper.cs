using UnityEngine;

namespace PipeFlowOverlay.Wrappers
{
    internal interface IConduitWrapper
    {
        GameObject GameObject { get; }
        int Cell { get; }
        IConduitFlowWrapper GetFlowManager();
        IUtilityNetworkMgr GetNetworkManager();
        bool IsNullOrDestroyed();
    }
}
