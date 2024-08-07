using System.Text.Json.Serialization;

namespace SA2MsgFileTextTool
{
    public class Message
    {
        public int? Voice { get; set; }
        public int? Duration { get; set; }

        [JsonPropertyName("2P Piece")]
        public bool? Is2PPiece { get; set; }
        public bool? Centered { get; set; }
        public string Text { get; set; }

        [JsonConstructor]
        public Message() { }

        public Message(int? voice, int? duration, bool? is2p, bool? centered, string text)
        {
            Voice = voice;
            Duration = duration;
            Is2PPiece = is2p;
            Centered = centered;
            Text = text;
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
