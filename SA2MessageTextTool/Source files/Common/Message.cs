using SA2MessageTextTool.Extensions;
using System.Text.Json.Serialization;

namespace SA2MessageTextTool.Common
{
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

            if (config.UseModifiedCyrillicCP)
                rawText = rawText.ModifyCyrillicCP();

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
            var bytes = reader.ReadBytesUntilNullTerminator();
            Text = ChaoTextConverter.ToString(bytes);
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
