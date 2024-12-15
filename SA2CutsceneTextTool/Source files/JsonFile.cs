using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace SA2CutsceneTextTool
{
    public static class JsonFile
    {
        public static List<Scene> Read(string jsonFile)
        {
            string fileName = Path.GetFileNameWithoutExtension(jsonFile);
            var json = JsonNode.Parse(File.ReadAllText(jsonFile));

            return JsonSerializer.Deserialize<List<Scene>>(json[fileName]);
        }
        
        
        public static void Write(string outputFile, List<Scene> jsonData, AppConfig config)
        {
            string fileName = Path.GetFileNameWithoutExtension(outputFile);

            if (config.JsonStyle == JsonStyle.Indented)
            {
                var jsonContents = new Dictionary<string, List<Scene>>()
                {
                    { fileName, jsonData }
                };
                
                var options = new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                var json = JsonSerializer.Serialize(jsonContents, options);
                File.WriteAllText($"{fileName}.json", json);
            }
            else
            {
                var json = new List<string>() { $"{{\n\t\"{fileName}\": [" };

                foreach (var scene in jsonData.OrderBy(x => x.EventID).ToList())
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

                File.WriteAllLines(outputFile, json);
            }            
        }
    }
}
