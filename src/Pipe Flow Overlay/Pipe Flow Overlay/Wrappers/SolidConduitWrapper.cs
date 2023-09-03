using UnityEngine;

namespace PipeFlowOverlay.Wrappers
{
    internal class SolidConduitWrapper : IConduitWrapper
    {
        private readonly SolidConduit _solidConduit;

        public SolidConduitWrapper(SolidConduit solidConduit)
        {
            _solidConduit = solidConduit;
        }

        public GameObject GameObject => _solidConduit.gameObject;

        public int Cell => Grid.PosToCell(_solidConduit);

        public IConduitFlowWrapper GetFlowManager()
        {
            return SolidConduitFlowWrapper.Instance;
        }

        public IUtilityNetworkMgr GetNetworkManager()
        {
            return _solidConduit.GetNetworkManager();
        }

        public bool IsNullOrDestroyed()
        {
            return _solidConduit.IsNullOrDestroyed();
        }
    }
}
