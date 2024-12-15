using System.Text.Json.Nodes;

namespace SA2SubtitlesTimingTool
{
    public class AppConfig
    {
        public Endianness Endianness { get; set; }

        public void Read()
        {
            var json = JsonNode.Parse(File.ReadAllText("AppConfig.json"));
            Endianness = ParseEnum<Endianness>(json["Config"]["Endianness"]);
        }


        private static TEnum ParseEnum<TEnum>(JsonNode node) where TEnum : struct
        {
            return Enum.Parse<TEnum>(node.GetValue<string>());
        }
    }
}
