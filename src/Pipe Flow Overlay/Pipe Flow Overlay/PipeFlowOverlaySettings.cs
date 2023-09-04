using Newtonsoft.Json;
using PeterHan.PLib.Options;
using System.Collections.Generic;

namespace PipeFlowOverlay
{
    [JsonObject(MemberSerialization.OptIn)]
    [ConfigFile(SharedConfigLocation: true)]
    public class PipeFlowOverlaySettings : IOptions
    {
        private static PipeFlowOverlaySettings _instance;

        [JsonProperty]
        public bool ShowOverlay { get; set; }

        [Option("STRINGS.UI.FRONTEND.PIPEFLOWOVERLAY.USEAFMARROWS", "STRINGS.UI.TOOLTIPS.PIPEFLOWOVERLAY.USEAFMARROWS")]
        [JsonProperty]
        public bool UseAFMArrows { get; set; }

        internal System.Type AFMCrossingCmp { get; set; }

        internal static PipeFlowOverlaySettings Instance => _instance ?? (_instance = POptions.ReadSettings<PipeFlowOverlaySettings>() ?? new PipeFlowOverlaySettings());
        internal static event System.Action ShowOverlayChanged;

        public PipeFlowOverlaySettings()
        {
            ShowOverlay = true;
        }

        internal void SetShowOverlay(bool showOverlay)
        {
            bool changed = ShowOverlay != showOverlay;
            ShowOverlay = showOverlay;
            if (changed)
                ShowOverlayChanged?.Invoke();
        }

        public IEnumerable<IOptionsEntry> CreateOptions()
        {
            yield break;
        }

        public void OnOptionsChanged()
        {
            System.Type afmCrossingCmp = _instance?.AFMCrossingCmp;
            _instance = this;
            _instance.AFMCrossingCmp = afmCrossingCmp;
        }
    }
}
