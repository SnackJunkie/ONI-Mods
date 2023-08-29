using PeterHan.PLib.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace PipeFlowOverlay
{
    internal class PipeFlowOverlayIconController : KMonoBehaviour
    {
        private Image _image;

        internal bool IconIsDirty { get; set; }
        internal ConduitType ConduitType { get; set; }
        internal ConduitFlow.SOAInfo SOAInfo { get; set; }
        internal int Cell { get; set; }

        protected override void OnPrefabInit()
        {
            _image = gameObject.AddOrGet<Image>();
        }

        private void Update()
        {
            if (IconIsDirty)
            {
                IconIsDirty = false;
                ConduitFlow.FlowDirections flowDirections = SOAInfo.GetPermittedFlowDirections(Cell);
                Debug.Log(flowDirections.ToString());
            }
        }
    }
}
