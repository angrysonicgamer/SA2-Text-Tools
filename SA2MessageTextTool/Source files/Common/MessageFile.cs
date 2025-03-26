using System.Text.Json.Serialization;

namespace SA2MessageTextTool.Common
{
    public class MessageFile
    {
        public string Name { get; set; }
        public List<List<Message>> Messages { get; set; }

        [JsonConstructor]
        public MessageFile() { }

        public MessageFile(string name, List<List<Message>> messages)
        {
            Name = name;
            Messages = messages;
        }
    }
}
