using Newtonsoft.Json;

namespace PipeFlowOverlay
{
    [JsonObject(MemberSerialization.OptIn)]
    internal class PipeFlowOverlaySettings
    {
        [JsonProperty]
        public bool ShowOverlay { get; set; }

        internal PipeFlowOverlaySettings()
        {
            ShowOverlay = true;
        }
    }
}
