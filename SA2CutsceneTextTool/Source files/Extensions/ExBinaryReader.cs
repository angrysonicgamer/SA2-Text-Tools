using System.Text;
using SA2CutsceneTextTool.Common;

namespace SA2CutsceneTextTool.Extensions
{
    public static class ExBinaryReader
    {
        public static void SetPosition(this BinaryReader reader, long position)
        {
            reader.BaseStream.Position = position;
        }

        public static T ReadAt<T>(this BinaryReader reader, long position, Func<BinaryReader, T> func)
        {
            var origPosition = reader.BaseStream.Position;

            if (origPosition != position)
            {
                reader.SetPosition(position);
            }

            T value;

            try
            {
                value = func(reader);
            }
            finally
            {
                reader.SetPosition(origPosition);
            }

            return value;
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
