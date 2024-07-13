using System.Text;

namespace SA2ChaoNamesTool
{
    public enum Endianness
    {
        BigEndian,
        LittleEndian,
    }

    public enum TextConversionMode
    {
        Default,
        Reversed,
    }

    public static class Extensions
    {
        public static int ReadInt32(this BinaryReader reader, Endianness endianness)
        {
            if (endianness == Endianness.LittleEndian)
            {
                return reader.ReadInt32();
            }
            else
            {
                byte[] data = reader.ReadBytes(4);
                Array.Reverse(data);
                return BitConverter.ToInt32(data, 0);
            }
        }

        public static string ReadChaoName(this BinaryReader reader)
        {
            var bytes = reader.ReadBytesUntilNullTerminator();
            return TextConversion.ToString(bytes);
        }


        private static byte[] ReadBytesUntilNullTerminator(this BinaryReader reader)
        {
            var bytes = new List<byte>();

            do
            {
                bytes.Add(reader.ReadByte());
            }
            while (bytes.Last() != 0);

            return bytes.ToArray();
        }
    }
}
