namespace ReplayAnalyzer.GameplayMods.Mods
{
    public class StrictTrackingMod
    {
        public static bool IsStrictTrackingEnabled { get; private set; } = false;

        public static void ApplyValues(bool isLazer)
        {
            if (isLazer == true)
            {
                ApplyLazer();
            }
        }

        private static void ApplyLazer()
        {
            IsStrictTrackingEnabled = true;
        }
    }
}
