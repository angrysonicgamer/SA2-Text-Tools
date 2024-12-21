using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace SA2CutsceneTextTool
{
    public static class JsonFile
    {
        public static EventFile Read(string jsonFile)
        {
            DisplayMessage.ReadingFile(jsonFile);
            var json = JsonNode.Parse(File.ReadAllText(jsonFile));
            return JsonSerializer.Deserialize<EventFile>(json);
        }
        
        
        public static void Write(EventFile data, AppConfig config)
        {
            string jsonFile = $"{data.Name}.json";
            
            if (config.JsonStyle == JsonStyle.Indented)
            {
                var options = new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                var json = JsonSerializer.Serialize(data, options);
                File.WriteAllText(jsonFile, json);
            }
            else // making custom json where each message is represented by a single line
            {
                var json = new List<string>() { $"{{\n\t\"Name\": \"{data.Name}\",\n\t\"Events\": [" };

                foreach (var scene in data.Events.OrderBy(x => x.EventID).ToList())
                {
                    json.Add($"\t\t{{\n\t\t\t\"EventID\": {scene.EventID},\n\t\t\t\"Messages\": [");

                    foreach (var message in scene.Messages)
                    {
                        string character = $"\"Character\": {message.Character}";
                        string centered = message.Centered.HasValue ? $", \"Centered\": \"{message.Centered}\"" : "";
                        string text = $", \"Text\": \"{message.Text.Replace("\\", "\\\\").Replace("\n", "\\n").Replace("\"", "\\\"")}\"";

                        json.Add($"\t\t\t\t{{ {character}{centered}{text} }},");
                    }

                    json[json.Count - 1] = json[json.Count - 1].TrimEnd(',');
                    json.Add("\t\t\t]\n\t\t},");
                }

                json[json.Count - 1] = json[json.Count - 1].TrimEnd(',');
                json.Add("\t]\n}");

                File.WriteAllLines(jsonFile, json);
            }

            DisplayMessage.Config(config);
            DisplayMessage.TextExtracted(jsonFile);
        }
    }
}
