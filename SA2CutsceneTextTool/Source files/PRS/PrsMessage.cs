using System.Text;
using SA2CutsceneTextTool.Common;
using SA2CutsceneTextTool.Extensions;

namespace SA2CutsceneTextTool.PRS
{
    public class PrsMessage(int character, uint textPtr, string text)
    {
        public int Character => character;
        public uint TextPointer => textPtr;
        public string RawText => text;
        public static uint Size => 8;


        public void WriteData(BinaryWriter writer, AppConfig config)
        {
            writer.WriteUInt32((uint)Character, config.Endianness);
            writer.WriteUInt32(TextPointer, config.Endianness);            
        }

        public void WriteText(BinaryWriter writer, AppConfig config)
        {
            writer.WriteCString(RawText, config.Encoding);
        }
    }
}
