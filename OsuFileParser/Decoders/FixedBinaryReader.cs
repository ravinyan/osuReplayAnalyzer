using System.Text;

namespace ReplayParsers.Decoders
{
    public class FixedBinaryReader : BinaryReader
    {
        public FixedBinaryReader(Stream stream) : base(stream, Encoding.UTF8) { }
    
        // you stupid not working correctly as i want function who gave me big headache i personally want to kick you in the face if you had one
        public override string ReadString()
        {
            if (ReadByte() == 0)
            {
                return null!;
            }
    
            return base.ReadString();
        }
    
    }
}
