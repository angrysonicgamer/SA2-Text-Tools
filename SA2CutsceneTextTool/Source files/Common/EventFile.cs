using System.Text.Json.Serialization;

namespace SA2CutsceneTextTool.Common
{
    public class EventFile
    {
        public string Name { get; set; }
        public List<Scene> Events { get; set; }

        [JsonConstructor]
        public EventFile() { }

        public EventFile(string name, List<Scene> events)
        {
            Name = name;
            Events = events;
        }
    }
}
