using SA2MessageTextTool.Common;

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
    }
}
