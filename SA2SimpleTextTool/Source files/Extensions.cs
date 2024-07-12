using System.Text;

namespace SA2SimpleTextTool
{
    public static class Extensions
    {
        private static readonly Dictionary<char, char> lettersToConvert = new Dictionary<char, char>()
        {
            { '№', 'Ё' },
            { 'є', 'ё' },
        };

        public static string ConvertToModifiedCodepage(this string text, bool reverse = false)
        {
            foreach (var pair in lettersToConvert)
            {
                if (!reverse)
                    text = text.Replace(pair.Key, pair.Value);
                else
                    text = text.Replace(pair.Value, pair.Key);
            }

            return text;
        }

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

        public static string ReadCString(this BinaryReader reader, Encoding encoding)
        {
            return encoding.GetString(reader.ReadBytesUntilNullTerminator());
        }
    }
}
