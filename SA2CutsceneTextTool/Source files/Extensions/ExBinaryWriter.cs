using System.Text;
using SA2CutsceneTextTool.Common;

namespace SA2CutsceneTextTool.Extensions
{
    public static class ExBinaryWriter
    {
        public static void WriteUInt32(this BinaryWriter writer, uint value, Endianness endianness)
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
