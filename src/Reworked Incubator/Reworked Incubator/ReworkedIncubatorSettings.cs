using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace ReworkedIncubator
{
    [JsonObject(MemberSerialization.OptIn)]
    [RestartRequired]
    [ConfigFile(SharedConfigLocation: true)]
    public class ReworkedIncubatorSettings
    {
        [Option("STRINGS.UI.FRONTEND.REWORKEDINCUBATOR.POWER", "STRINGS.UI.TOOLTIPS.REWORKEDINCUBATOR.POWER")]
        [Limit(0, 50000)]
        [JsonProperty]
        public float Power { get; set; }

        [Option("STRINGS.UI.FRONTEND.REWORKEDINCUBATOR.SELFHEAT", "STRINGS.UI.TOOLTIPS.REWORKEDINCUBATOR.SELFHEAT")]
        [Limit(0, 50000)]
        [JsonProperty]
        public float SelfHeat { get; set; }

        [Option("STRINGS.UI.FRONTEND.REWORKEDINCUBATOR.EXHAUSTHEAT", "STRINGS.UI.TOOLTIPS.REWORKEDINCUBATOR.EXHAUSTHEAT")]
        [Limit(0, 50000)]
        [JsonProperty]
        public float ExhaustHeat { get; set; }

        [Option("STRINGS.UI.FRONTEND.REWORKEDINCUBATOR.INCUBATIONRATE", "STRINGS.UI.TOOLTIPS.REWORKEDINCUBATOR.INCUBATIONRATE")]
        [Limit(0, 50000)]
        [JsonProperty]
        public float IncubationRate { get; set; }

        [Option("STRINGS.UI.FRONTEND.REWORKEDINCUBATOR.XPMULTIPIER", "STRINGS.UI.TOOLTIPS.REWORKEDINCUBATOR.XPMULTIPIER")]
        [Limit(0, 100)]
        [JsonProperty]
        public float XPMultiplier { get; set; }

        [Option("STRINGS.UI.FRONTEND.REWORKEDINCUBATOR.FETCHAUTOMATION", "STRINGS.UI.TOOLTIPS.REWORKEDINCUBATOR.FETCHAUTOMATION")]
        [JsonProperty]
        public bool FetchAutomation { get; set; }

        public ReworkedIncubatorSettings()
        {
            Power = 120;
            SelfHeat = 2000;
            ExhaustHeat = 250;
            IncubationRate = 400;
            FetchAutomation = true;
            XPMultiplier = 1;
        }
    }
}
