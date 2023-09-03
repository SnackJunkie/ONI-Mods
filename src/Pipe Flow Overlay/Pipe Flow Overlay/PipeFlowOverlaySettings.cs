using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace PipeFlowOverlay
{
    [JsonObject(MemberSerialization.OptIn)]
    internal class PipeFlowOverlaySettings
    {
        [JsonProperty]
        public bool ShowOverlay { get; set; }

        public static PipeFlowOverlaySettings Instance { get; }
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
