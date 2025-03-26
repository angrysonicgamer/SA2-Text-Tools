using System.Text;
using SA2MessageTextTool.Common;

namespace SA2MessageTextTool.Extensions
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

        public static string ReadCString(this BinaryReader reader, Encoding encoding)
        {
            return encoding.GetString(reader.ReadBytesUntilNullTerminator());
        }

        public static byte[] ReadBytesUntilNullTerminator(this BinaryReader reader)
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
