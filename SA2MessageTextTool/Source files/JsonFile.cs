using System.Text.Json;
using System.Text.Json.Nodes;

namespace SA2MsgFileTextTool
{
    public static class JsonFile
    {
        public static List<List<Message>> Read(string inputFile)
        {
            string fileName = Path.GetFileNameWithoutExtension(inputFile);
            var json = JsonNode.Parse(File.ReadAllText(inputFile));
            var jsonContents = JsonSerializer.Deserialize<List<List<Message>>>(json[fileName]);

            return jsonContents;
        }

        public static void Write(string outputFile, List<List<Message>> fileContents)
        {
            string fileName = Path.GetFileNameWithoutExtension(outputFile);
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
