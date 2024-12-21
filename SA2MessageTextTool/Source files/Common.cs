using System.Text.Json.Serialization;

namespace SA2MessageTextTool
{
    public enum Endianness
    {
        BigEndian,
        LittleEndian
    }

    public enum Encodings
    {
        Windows1251 = 1251,
        Windows1252 = 1252,
        ShiftJIS = 932
    }

    public enum JsonStyle
    {
        Indented,
        SingleLinePerEntry
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CenteringMethod : short
    {
        NotCentered,        // ignored as null
        Block = 7,          // \a
        EachLine = 9        // \t
    }

    public class Message
    {
        public int? Voice { get; set; }
        public int? Duration { get; set; }

        [JsonPropertyName("2P Piece")]
        public bool? Is2PPiece { get; set; }
        public CenteringMethod? Centered { get; set; }
        public string Text { get; set; }

        [JsonConstructor]
        public Message() { }


        public void Parse(string rawText, AppConfig config)
        {
            rawText = rawText.ReplaceKeyboardButtons();
            
            if (config.ModifiedCodepage == true)
                rawText = rawText.ConvertToModifiedCodepage();

            if (rawText.StartsWith('\x0C'))
            {
                string controls = rawText.Substring(0, rawText.IndexOf(' '));
                Is2PPiece = controls.IndexOf('D') != -1 ? true : null;
                string? voice = controls.IndexOf('s') != -1 ? controls.Substring(controls.IndexOf('s') + 1, controls.IndexOf('w') - controls.IndexOf('s') - 1) : null;
                Voice = voice != null ? int.Parse(voice) : null;
                string? frameCount = controls.IndexOf('w') != -1 ? controls.Substring(controls.IndexOf('w') + 1) : null;
                Duration = frameCount != null ? int.Parse(frameCount) : null;
                rawText = rawText.Substring(rawText.IndexOf(' ') + 1);
            }            
            
            Centered = GetCenteringMethod(rawText);
            Text = Centered.HasValue ? rawText.Substring(1) : rawText;
        }

        public void ReadChaoName(BinaryReader reader)
        {
            Text = reader.ReadChaoName();
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

    public class MessageFile
    {
        public string Name { get; set; }
        public List<List<Message>> Messages { get; set; }

        [JsonConstructor]
        public MessageFile() { }

        public MessageFile(string name, List<List<Message>> messages)
        {
            Name = name;
            Messages = messages;
        }
    }

    public class CStyleText
    {
        public byte[] Text { get; set; }
        public int Offset { get; set; }

        public CStyleText(byte[] text, int offset)
        {
            Text = text;
            Offset = offset;
        }
    }
}
