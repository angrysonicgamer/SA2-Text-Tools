using System.Text;
using SA2CutsceneTextTool.Common;

namespace SA2CutsceneTextTool.PRS
{
    public class PrsMessage(int character, uint textPtr, string text)
    {
        public int Character => character;
        public uint TextPointer => textPtr;
        public string RawText => text;
        public static uint Size => 8;

        public void WriteMessageData(ref List<byte> writeTo, Endianness endianness)
        {
            byte[] character = BitConverter.GetBytes(Character);
            byte[] textPtr = BitConverter.GetBytes(TextPointer);

            if (endianness == Endianness.BigEndian)
            {
                character = character.Reverse().ToArray();
                textPtr = textPtr.Reverse().ToArray();
            }

            writeTo.AddRange(character);
            writeTo.AddRange(textPtr);
        }

        public void WriteText(ref List<byte> writeTo, Encoding encoding)
        {
            writeTo.AddRange(encoding.GetBytes(RawText));
            writeTo.Add(0);
        }
    }
}
