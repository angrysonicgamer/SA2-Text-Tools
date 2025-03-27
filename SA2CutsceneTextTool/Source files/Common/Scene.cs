using System.Text.Json.Serialization;

namespace SA2CutsceneTextTool.Common
{
    public class Scene
    {
        public int EventID { get; set; }
        public List<Message> Messages { get; set; }

        [JsonConstructor]
        public Scene() { }

        public Scene(int eventID, List<Message> messages)
        {
            EventID = eventID;
            Messages = messages;
        }
    }
}
