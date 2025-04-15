using SA2MessageTextTool.Common;
using System.Text;

namespace SA2MessageTextTool.Extensions
{
    public static class ExBinaryWriter
    {
        public static void WriteInt32(this BinaryWriter writer, int value, Endianness endianness)
        {
            if (endianness == Endianness.LittleEndian)
            {
                writer.Write(value);
            }
            else
            {
                writer.Write(BitConverter.GetBytes(value).Reverse().ToArray());
            }
        }

        public static void WriteCString(this BinaryWriter writer, string str, Encoding encoding)
        {
            writer.Write(encoding.GetBytes(str));
            writer.Write((byte)0);
        }
    }
}
