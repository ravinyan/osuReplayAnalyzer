using SevenZip;
using SevenZip.Compression.LZMA;

namespace what.Decoders
{
    // literally god
    // https://gist.github.com/ststeiger/cb9750664952f775a341
    public class LZMADecoder
    {
        public static byte[] Compress(string fileName)
        {
            CoderPropID[] propIDs =
            {
                CoderPropID.DictionarySize,
                CoderPropID.PosStateBits,
                CoderPropID.LitContextBits,
                CoderPropID.LitPosBits,
            };

            object[] properties =
            {
                1 << 11,
                2,
                3,
                0,
            };

            byte[] compressedVal = null!;
            Encoder encoder = new Encoder();
            encoder.SetCoderProperties(propIDs, properties);

            using (Stream inStream = new FileStream(fileName, FileMode.Open))
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    encoder.WriteCoderProperties(outStream);
                    long fileSize = inStream.Length;

                    for (int i = 0; i < 8; i++)
                    {
                        outStream.WriteByte((byte)(fileSize >> 8 * i));
                    }

                    encoder.Code(inStream, outStream, -1, -1, null!);
                    compressedVal = outStream.ToArray();
                }
            }

            return compressedVal;
        }

        public static byte[] Decompress(byte[] inputBytes)
        {
            byte[] decompressedVal = null!;
            Decoder decoder = new Decoder();

            using (MemoryStream inStream = new MemoryStream(inputBytes))
            {
                inStream.Seek(0, 0);

                using (MemoryStream outStream = new MemoryStream())
                {
                    byte[] properties2 = new byte[5];

                    if (inStream.Read(properties2, 0, 5) != 5)
                    {
                        throw new Exception("input .lzma is too short");
                    }

                    long outSize = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        int val = inStream.ReadByte();

                        if (val < 0)
                        {
                            throw new Exception("Cant read 1");
                        }

                        outSize |= (long)(byte)val << 8 * i;
                    }

                    decoder.SetDecoderProperties(properties2);

                    long compressedSize = inStream.Length - inStream.Position;
                    decoder.Code(inStream, outStream, compressedSize, outSize, null!);

                    decompressedVal = outStream.ToArray();
                }
            }

            return decompressedVal;
        }
    }
}
