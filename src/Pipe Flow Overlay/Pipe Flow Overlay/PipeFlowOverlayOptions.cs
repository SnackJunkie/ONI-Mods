using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace PipeFlowOverlay
{
    [JsonObject(MemberSerialization.OptIn)]
    [ConfigFile(SharedConfigLocation: true)]
    public class PipeFlowOverlayOptions
    {
        private static PipeFlowOverlayOptions _options;

        [Option("STRINGS.UI.FRONTEND.PIPEFLOWOVERLAY.USEAFMARROWS", "STRINGS.UI.TOOLTIPS.PIPEFLOWOVERLAY.USEAFMARROWS")]
        [JsonProperty]
        public bool UseAFMArrows { get; set; }

        internal static PipeFlowOverlayOptions Get()
        {
            return _options ?? (_options = POptions.ReadSettings<PipeFlowOverlayOptions>() ?? new PipeFlowOverlayOptions());
        }
    }
}
