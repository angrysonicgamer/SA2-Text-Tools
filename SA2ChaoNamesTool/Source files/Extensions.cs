using System.Text;

namespace SA2ChaoNamesTool
{
    public static class Extensions
    {
        public static int ReadInt32BigEndian(this BinaryReader reader)
        {
            byte[] data = reader.ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToInt32(data, 0);
        }

        public static byte[] ReadBytesUntilNullTerminator(this BinaryReader reader)
        {
            var bytes = new List<byte>();

            do
            {
                bytes.Add(reader.ReadByte());
            }
            while (bytes.Last() != 0);

            return bytes.ToArray();
        }

        public static string ReadChaoName(this BinaryReader reader)
        {
            var bytes = reader.ReadBytesUntilNullTerminator();
            return TextConversion.ToString(bytes);
        }
    }
}
