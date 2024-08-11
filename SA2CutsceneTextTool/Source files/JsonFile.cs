using System.Text.Json;
using System.Text.Json.Nodes;

namespace SA2CutsceneTextTool
{
    public static class JsonFile
    {
        public static List<Scene> Read(string inputFile)
        {
            string fileName = Path.GetFileNameWithoutExtension(inputFile);
            var json = JsonNode.Parse(File.ReadAllText(inputFile));

            return JsonSerializer.Deserialize<List<Scene>>(json[fileName]);
        }
        
        
        public static void Write(string outputFile, List<Scene> jsonData)
        {
            string fileName = Path.GetFileNameWithoutExtension(outputFile);
            var json = new List<string>() { $"{{\n\t\"{fileName}\": [" };

            foreach (var scene in jsonData.OrderBy(x => x.EventID).ToList())
            {
                json.Add($"\t\t{{\n\t\t\t\"EventID\": {scene.EventID},\n\t\t\t\"Messages\": [");

                foreach (var message in scene.Messages)
                {
                    string character = $"\"Character\": {message.Character}";                    
                    string centered = $", \"Centered\": {message.Centered.ToString().ToLower()}";
                    string text = $", \"Text\": \"{message.Text.Replace("\n", "\\n").Replace("\"", "\\\"")}\"";

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
