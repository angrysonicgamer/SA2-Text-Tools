using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace SA2MessageTextTool
{
    public static class JsonFile
    {
        public static List<List<Message>> Read(string jsonFile)
        {
            string fileName = Path.GetFileNameWithoutExtension(jsonFile);
            var json = JsonNode.Parse(File.ReadAllText(jsonFile));
            return JsonSerializer.Deserialize<List<List<Message>>>(json[fileName]);
        }

        public static void Write(string outputFile, List<List<Message>> fileContents, AppConfig config)
        {
            string fileName = Path.GetFileNameWithoutExtension(outputFile);
            var jsonContents = new Dictionary<string, List<List<Message>>>()
            {
                { fileName, fileContents },
            };

            if (config.JsonStyle == JsonStyle.Indented)
            {
                var options = new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                var json = JsonSerializer.Serialize(jsonContents, options);
                File.WriteAllText(outputFile, json);
            }
            else
            {
                var json = new List<string>() { $"{{\n\t\"{fileName}\": [" };

                foreach (var linesList in fileContents)
                {
                    json.Add("\t\t[");
                    foreach (var line in linesList)
                    {
                        string text = $"\"Text\": \"{line.Text.Replace("\n", "\\n").Replace("\"", "\\\"")}\"";
                        string voice = line.Voice != null ? $", \"Voice\": {line.Voice}" : "";
                        string duration = line.Duration != null ? $", \"Duration\": {line.Duration}" : "";
                        string centered = line.Centered != null ? $", \"Centered\": {line.Centered.ToString().ToLower()}" : "";
                        string is2p = line.Is2PPiece != null ? $", \"2P Piece\": {line.Is2PPiece.ToString().ToLower()}" : "";

                        string msg = $"\t\t\t{{ {text}{voice}{duration}{centered}{is2p} }},";
                        json.Add(msg);
                    }
                    json[json.Count - 1] = json[json.Count - 1].TrimEnd(',');
                    json.Add("\t\t],");
                }

                json[json.Count - 1] = json[json.Count - 1].TrimEnd(',');
                json.Add("\t]\n}");

                File.WriteAllLines(outputFile, json);
            }            
        }
    }
}
