namespace ReworkedIncubator
{
    public static class ReworkedIncubatorStrings
    {
        public static class CREATURES
        {
            public static class MODIFIERS
            {
                public static class INCUBATOR_SONG
                {
                    public static LocString NAME = "Powered Incubator";
                    public static LocString TOOLTIP = "This egg is currently in a powered Incubator\n\nIncreased " + STRINGS.UI.PRE_KEYWORD + "Incubation Rate" + STRINGS.UI.PST_KEYWORD;
                }
            }
        }

        public static class UI
        {
            public static class FRONTEND
            {
                public static class REWORKEDINCUBATOR
                {
                    public static LocString POWER = "Power";
                    public static LocString SELFHEAT = "Self Heat";
                    public static LocString EXHAUSTHEAT = "Exhaust Heat";
                    public static LocString INCUBATIONRATE = "Incubation Rate";
                    public static LocString FETCHAUTOMATION = "Enable Automation";
                }
            }

            public static class TOOLTIPS
            {
                public static class REWORKEDINCUBATOR
                {
                    public static LocString POWER = "How much power, in watts, the incubator uses.";
                    public static LocString SELFHEAT = "How much heat, in DTU, the incubator generates internally when running.";
                    public static LocString EXHAUSTHEAT = "How much heat, in DTU, the incubator emits when running.";
                    public static LocString INCUBATIONRATE = "% increase in incubation rate from powered incubator.";
                    public static LocString FETCHAUTOMATION = "If enabled, errands to place an egg in the incubator can be disabled via automation.";
                    public static LocString AUTOMATIONPRECONDITION = "Disabled by automation";
                }
            }
        }
    }
}
