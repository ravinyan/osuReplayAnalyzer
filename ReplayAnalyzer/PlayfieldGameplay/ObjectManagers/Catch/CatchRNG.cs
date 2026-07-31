namespace ReplayAnalyzer.PlayfieldGameplay.ObjectManagers.Catch
{
    // entire class taken from osu lazer but with hard coded catch rng X value
    public class CatchRNG
    {
        private const double int_to_real = 1.0 / (int.MaxValue + 1.0);
        private const uint int_mask = 0x7FFFFFFF;
        private static uint X = 1337; // this is value of RNG_SEED const in lazer used for offsetting droplets and fruits with HR enabled
        private static uint Y = 842502087;
        private static uint Z = 3579807591;
        private static uint W = 273326509;
        private static uint bitBuffer;
        private static int bitIndex = 32;

        public static void ResetSeed()
        {
            X = 1337;
            Y = 842502087;
            Z = 3579807591;
            W = 273326509;
            bitIndex = 32;
            bitBuffer = 0;
        }

        public static uint NextUInt()
        {
            uint t = X ^ (X << 11);
            X = Y;
            Y = Z;
            Z = W;
            return W = W ^ W >> 19 ^ t ^ t >> 8;
        }

        public static int Next(int lowerBound, int upperBound) => (int)(lowerBound + NextDouble() * (upperBound - lowerBound));

        public static int Next(double lowerBound, double upperBound) => (int)(lowerBound + NextDouble() * (upperBound - lowerBound));

        public static int Next() => (int)(int_mask & NextUInt());

        public static double NextDouble() => int_to_real * Next();

        public static bool NextBool()
        {
            if (bitIndex == 32)
            {
                bitBuffer = NextUInt();
                bitIndex = 1;

                return (bitBuffer & 1) == 1;
            }

            bitIndex++;
            return ((bitBuffer >>= 1) & 1) == 1;
        }
    }
}
