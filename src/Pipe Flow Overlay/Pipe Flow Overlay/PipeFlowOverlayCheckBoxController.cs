using PeterHan.PLib.Options;
using PeterHan.PLib.UI;

namespace PipeFlowOverlay
{
    internal class PipeFlowOverlayCheckBoxController : KMonoBehaviour
    {
        private MultiToggle _toggle;

        protected override void OnSpawn()
        {
            _toggle = GetComponent<MultiToggle>();
            _toggle.onClick += CheckBoxChecked;
            int state = PipeFlowOverlaySettings.Instance.ShowOverlay ? PCheckBox.STATE_CHECKED : PCheckBox.STATE_UNCHECKED;
            if (_toggle.CurrentState != state)
                PCheckBox.SetCheckState(gameObject, state);
        }

        private void CheckBoxChecked()
        {
            int state = _toggle.CurrentState != PCheckBox.STATE_UNCHECKED ? PCheckBox.STATE_UNCHECKED : PCheckBox.STATE_CHECKED;
            PCheckBox.SetCheckState(gameObject, state);
            PipeFlowOverlaySettings.Instance.SetShowOverlay(state == PCheckBox.STATE_CHECKED);
            POptions.WriteSettings(PipeFlowOverlaySettings.Instance);
        }
    }
}
