using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace SA2MessageTextTool
{
    public static class JsonFile
    {
        public static JsonContents Read(string jsonFile)
        {
            var json = JsonNode.Parse(File.ReadAllText(jsonFile));
            return JsonSerializer.Deserialize<JsonContents>(json);
        } 

        public static void Write(JsonContents jsonContents, AppConfig config)
        {
            string jsonFile = $"{jsonContents.Name}.json";

            if (config.JsonStyle == JsonStyle.Indented)
            {
                var options = new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                var json = JsonSerializer.Serialize(jsonContents, options);
                File.WriteAllText(jsonFile, json);
            }
            else
            {
                var json = new List<string>() { $"{{\n\t\"Name\": \"{jsonContents.Name}\",\n\t\"Messages\": [" };

                foreach (var linesList in jsonContents.Messages)
                {
                    json.Add("\t\t[");
                    foreach (var line in linesList)
                    {
                        string text = $"\"Text\": \"{line.Text.Replace("\\", "\\\\").Replace("\n", "\\n").Replace("\"", "\\\"")}\"";
                        string voice = line.Voice.HasValue ? $", \"Voice\": {line.Voice}" : "";
                        string duration = line.Duration.HasValue ? $", \"Duration\": {line.Duration}" : "";
                        string centered = line.Centered.HasValue ? $", \"Centered\": \"{line.Centered}\"" : "";
                        string is2p = line.Is2PPiece.HasValue ? $", \"2P Piece\": {line.Is2PPiece.ToString().ToLower()}" : "";

                        string msg = $"\t\t\t{{ {text}{voice}{duration}{centered}{is2p} }},";
                        json.Add(msg);
                    }
                    json[json.Count - 1] = json[json.Count - 1].TrimEnd(',');
                    json.Add("\t\t],");
                }

                json[json.Count - 1] = json[json.Count - 1].TrimEnd(',');
                json.Add("\t]\n}");

                File.WriteAllLines(jsonFile, json);                
            }

            DisplayMessage.Config(config);
            DisplayMessage.JsonCreated(jsonFile);
        }
    }
}
