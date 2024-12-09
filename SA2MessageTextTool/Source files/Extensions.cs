using System.Text;

namespace SA2MessageTextTool
{
    public enum TextConversionMode
    {
        Default,
        Reversed,
    }

    public static class Extensions
    {
        private static readonly Dictionary<string, string> buttonsMap = new Dictionary<string, string>()
        {
            { "±", "{A}" },
            { "¶", "{B}" },
            { "Ё", "{Y}" },
        };

        private static readonly Dictionary<char, char> lettersToConvert = new Dictionary<char, char>()
        {
            { '№', 'Ё' },
            { 'є', 'ё' },
        };


        public static string ReplaceKeyboardButtons(this string text, TextConversionMode mode = TextConversionMode.Default)
        {
            foreach (var pair in buttonsMap)
            {
                if (mode == TextConversionMode.Default)
                    text = text.Replace(pair.Key, pair.Value);
                else
                    text = text.Replace(pair.Value, pair.Key);
            }

            return text;
        }

        public static string ConvertToModifiedCodepage(this string text, TextConversionMode mode = TextConversionMode.Default)
        {
            foreach (var pair in lettersToConvert)
            {
                if (mode == TextConversionMode.Default)
                    text = text.Replace(pair.Key, pair.Value);
                else
                    text = text.Replace(pair.Value, pair.Key);
            }

            return text;
        }

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

        public static string ReadCString(this BinaryReader reader, Encoding encoding)
        {
            return encoding.GetString(reader.ReadBytesUntilNullTerminator());
        }

        public static string ReadChaoName(this BinaryReader reader)
        {
            var bytes = reader.ReadBytesUntilNullTerminator();
            return ChaoTextConverter.ToString(bytes);
        }


        private static byte[] ReadBytesUntilNullTerminator(this BinaryReader reader)
        {
            var bytes = new List<byte>();

            while (true)
            {
                byte b = reader.ReadByte();
                if (b == 0) break;

                bytes.Add(b);
            }

            return bytes.ToArray();
        }
    }
}
