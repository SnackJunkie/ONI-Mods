using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace PipeFlowOverlay
{
    [JsonObject(MemberSerialization.OptIn)]
    internal class PipeFlowOverlaySettings
    {
        [JsonProperty]
        internal bool ShowOverlay { get; set; }

        internal System.Type AFMCrossingCmp { get; set; }

        internal static PipeFlowOverlaySettings Instance { get; }
        internal static event System.Action ShowOverlayChanged;

        static PipeFlowOverlaySettings()
        {
            Instance = POptions.ReadSettings<PipeFlowOverlaySettings>() ?? new PipeFlowOverlaySettings();
        }

        internal PipeFlowOverlaySettings()
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
    }
}
