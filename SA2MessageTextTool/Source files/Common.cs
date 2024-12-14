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
    public enum Centered : short
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
        public Centered? Centered { get; set; }
        public string Text { get; set; }

        [JsonConstructor]
        public Message() { }

        public Message(int? voice, int? duration, bool? is2p, Centered? centered, string text)
        {
            Voice = voice;
            Duration = duration;
            Is2PPiece = is2p;
            Centered = centered;
            Text = text;
        }
    }

    public class JsonContents
    {
        public string Name { get; set; }
        public List<List<Message>> Messages { get; set; }

        [JsonConstructor]
        public JsonContents() { }

        public JsonContents(string name, List<List<Message>> messages)
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
