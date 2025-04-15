using SA2CutsceneTextTool.Common;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace SA2CutsceneTextTool.JSON
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
            string json;

            if (config.JsonStyle == JsonStyle.Indented)
            {
                json = GetIndentedJson(data);
            }
            else
            {
                json = GetCustomJson(data);
            }

            File.WriteAllText(jsonFile, json);
            DisplayMessage.Config(config);
            DisplayMessage.TextExtracted(jsonFile);
        }


        private static string GetIndentedJson(EventFile data)
        {
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            return JsonSerializer.Serialize(data, options);
        }

        private static string GetCustomJson(EventFile data)
        {
            const string objStart = "{";
            const string objEnd = "}";
            const string arrayStart = "[";
            const string arrayEnd = "]";

            var json = new CustomJson();
            json.AddString(objStart);
            json.AddString($"\"Name\": \"{data.Name}\",", JsonIndentationLevel.One);
            json.AddString($"\"Events\": {arrayStart}", JsonIndentationLevel.One);

            foreach (var scene in data.Events)
            {
                json.AddString(objStart, JsonIndentationLevel.Two);
                json.AddString($"\"EventID\": {scene.EventID},", JsonIndentationLevel.Three);
                json.AddString($"\"Messages\": {arrayStart}", JsonIndentationLevel.Three);

                foreach (var message in scene.Messages)
                {
                    json.AddString($"{message},", JsonIndentationLevel.Four);
                }

                json.RemoveTrailingComma();
                json.AddString(arrayEnd, JsonIndentationLevel.Three);
                json.AddString($"{objEnd},", JsonIndentationLevel.Two);
            }

            json.RemoveTrailingComma();
            json.AddString(arrayEnd, JsonIndentationLevel.One);
            json.AddString(objEnd);
            return json.Serialize();
        }
    }
}
