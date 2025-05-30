﻿using System.Text.Json.Serialization;
using SA2CutsceneTextTool.Extensions;

namespace SA2CutsceneTextTool.Common
{
    public class Message
    {
        public int Character { get; set; }
        public CenteringMethod? Centered { get; set; }
        public string Text { get; set; }

        [JsonConstructor]
        public Message() { }

        public Message(int character, CenteringMethod? centered, string text)
        {
            Character = character;
            Centered = centered;
            Text = text;
        }

        public void Read(BinaryReader reader, AppConfig config)
        {
            Character = reader.ReadInt32(config.Endianness);
            uint textOffset = reader.ReadUInt32(config.Endianness) - Pointer.BaseAddress;
            string text = reader.ReadAt(textOffset, x => x.ReadCString(config.Encoding));
            Centered = GetCenteringMethod(text);
            Text = Centered.HasValue ? text.Substring(1) : text;
        }

        public override string ToString()
        {
            string character = $"\"Character\": {Character}";
            string centered = Centered.HasValue ? $", \"Centered\": \"{Centered}\"" : "";
            string text = $", \"Text\": \"{Text.Replace("\\", "\\\\").Replace("\n", "\\n").Replace("\"", "\\\"")}\"";

            return $"{{ {character}{centered}{text} }}";
        }


        private static CenteringMethod? GetCenteringMethod(string text)
        {
            if (text.StartsWith('\a'))
                return CenteringMethod.Block;

            if (text.StartsWith('\t'))
                return CenteringMethod.EachLine;

            return null;
        }
    }
}
