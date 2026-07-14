namespace ReplayAnalyzer.PlayfieldGameplay.ObjectManagers
{
    internal class CatchRNG
    {
        private const double int_to_real = 1.0 / (int.MaxValue + 1.0);
        private const uint int_mask = 0x7FFFFFFF;
        private static uint X = 1337; // this is value of RNG_SEED const in lazer
        private static uint Y = 842502087;
        private static uint Z = 3579807591;
        private static uint W = 273326509;

        public static uint NextUInt()
        {
            uint t = X ^ (X << 11);
            X = Y;
            Y = Z;
            Z = W;
            return W = W ^ (W >> 19) ^ t ^ (t >> 8);
        }

        public static int Next(int lowerBound, int upperBound) => (int)(lowerBound + NextDouble() * (upperBound - lowerBound));

        public static int Next() => (int)(int_mask & NextUInt());

        public static double NextDouble() => int_to_real * Next();
    }
}
