﻿using System.Text;

namespace SA2SubtitlesTimingTool
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
        private static readonly Dictionary<char, char> lettersToConvert = new Dictionary<char, char>()
        {
            { '№', 'Ё' },
            { 'є', 'ё' },
        };


        // string

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


        // BinaryReader

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

        public static uint ReadUInt32(this BinaryReader reader, Endianness endianness)
        {
            if (endianness == Endianness.LittleEndian)
            {
                return reader.ReadUInt32();
            }
            else
            {
                byte[] data = reader.ReadBytes(4);
                Array.Reverse(data);
                return BitConverter.ToUInt32(data, 0);
            }
        }

        public static string ReadCString(this BinaryReader reader, Encoding encoding)
        {
            var bytes = new List<byte>();

            while (true)
            {
                byte b = reader.ReadByte();
                if (b == 0) break;

                bytes.Add(b);
            }

            return encoding.GetString(bytes.ToArray());
        }
    }
}
