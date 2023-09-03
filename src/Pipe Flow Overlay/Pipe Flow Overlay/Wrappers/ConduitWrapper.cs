using UnityEngine;

namespace PipeFlowOverlay.Wrappers
{
    internal class ConduitWrapper : IConduitWrapper
    {
        private readonly Conduit _conduit;

        public ConduitWrapper(Conduit conduit)
        {
            _conduit = conduit;
        }

        public GameObject GameObject => _conduit.GameObject;

        public int Cell => _conduit.Cell;

        public IConduitFlowWrapper GetFlowManager()
        {
            return new ConduitFlowWrapper(_conduit.GetFlowManager());
        }

        public IUtilityNetworkMgr GetNetworkManager()
        {
            return _conduit.GetNetworkManager();
        }

        public bool IsNullOrDestroyed()
        {
            return _conduit.IsNullOrDestroyed();
        }
    }
}
