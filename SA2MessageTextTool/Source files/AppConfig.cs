using System.Text;
using System.Text.Json.Nodes;

namespace SA2MessageTextTool
{
    public class AppConfig
    {
        public Endianness Endianness { get; set; }
        public Encoding Encoding { get; set; }
        public bool? ModifiedCodepage { get; set; }


        public void Read()
        {
            var json = JsonNode.Parse(File.ReadAllText("AppConfig.json"));
            var jsonConfig = json["Config"];

            Endianness = Enum.Parse<Endianness>(jsonConfig["Endianness"].GetValue<string>());
            Encodings encoding = Enum.Parse<Encodings>(jsonConfig["Encoding"].GetValue<string>());
            Encoding = Encoding.GetEncoding((int)encoding);
            ModifiedCodepage = json["Config"]["ModifiedCodepage"] == null ? null : json["Config"]["ModifiedCodepage"].GetValue<bool?>();
        }
    }
}
