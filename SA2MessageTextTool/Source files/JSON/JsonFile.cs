using SA2MessageTextTool.Common;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace SA2MessageTextTool.JSON
{
    public static class JsonFile
    {
        public static MessageFile Read(string jsonFile)
        {
            DisplayMessage.ReadingFile(jsonFile);
            var json = JsonNode.Parse(File.ReadAllText(jsonFile));
            return JsonSerializer.Deserialize<MessageFile>(json);
        } 

        public static void Write(MessageFile msgFile, AppConfig config)
        {
            string jsonFile = $"{msgFile.Name}.json";
            string json;

            if (config.JsonStyle == JsonStyle.Indented)
            {
                json = GetIndentedJson(msgFile);
            }
            else
            {
                json = GetCustomJson(msgFile);
            }

            File.WriteAllText(jsonFile, json);
            DisplayMessage.Config(config);
            DisplayMessage.JsonCreated(jsonFile);
        }


        private static string GetIndentedJson(MessageFile jsonContents)
        {
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            return JsonSerializer.Serialize(jsonContents, options);
        }

        private static string GetCustomJson(MessageFile jsonContents)
        {
            const string objStart = "{";
            const string objEnd = "}";
            const string arrayStart = "[";
            const string arrayEnd = "]";
            
            var json = new CustomJson();
            json.AddString(objStart);
            json.AddString($"\"Name\": \"{jsonContents.Name}\",", JsonIndentationLevel.One);
            json.AddString($"\"Messages\": {arrayStart}", JsonIndentationLevel.One);

            foreach (var linesList in jsonContents.Messages)
            {
                json.AddString(arrayStart, JsonIndentationLevel.Two);

                foreach (var line in linesList)
                {
                    json.AddString($"{line},", JsonIndentationLevel.Three);
                }

                json.RemoveTrailingComma();
                json.AddString($"{arrayEnd},", JsonIndentationLevel.Two);
            }

            json.RemoveTrailingComma();
            json.AddString(arrayEnd, JsonIndentationLevel.One);
            json.AddString(objEnd);
            return json.Serialize();
        }        
    }
}
