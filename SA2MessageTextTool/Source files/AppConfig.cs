using System.Text;
using System.Text.Json.Nodes;

namespace SA2MessageTextTool
{
    public class AppConfig
    {
        public Endianness Endianness { get; set; }
        public Encoding Encoding { get; set; }
        public bool? ModifiedCodepage { get; set; }
        public JsonStyle JsonStyle { get; set; }


        public void Read()
        {
            var json = JsonNode.Parse(File.ReadAllText("AppConfig.json"));
            var config = json["Config"];

            Endianness = ParseEnum<Endianness>(config["Endianness"]);
            Encodings encoding = ParseEnum<Encodings>(config["Encoding"]);
            Encoding = Encoding.GetEncoding((int)encoding);
            ModifiedCodepage = config["ModifiedCodepage"] != null ? config["ModifiedCodepage"].GetValue<bool?>() : null;
            JsonStyle = ParseEnum<JsonStyle>(config["JsonStyle"]);
        }


        private static TEnum ParseEnum<TEnum>(JsonNode node) where TEnum : struct
        {
            return Enum.Parse<TEnum>(node.GetValue<string>());
        }
    }
}
