using System.Text.Json.Serialization;

namespace SA2CutsceneTextTool
{
    public enum Endianness
    {
        BigEndian,
        LittleEndian
    }

    public enum Encodings
    {
        Windows1251 = 1251,
        Windows1252 = 1252,
        ShiftJIS = 932
    }

    public enum Export
    {
        JSON,
        CSV
    }

    public enum JsonStyle
    {
        Indented,
        SingleLinePerEntry
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Centered : short
    {
        NotCentered,        // ignored as null
        Block = 7,          // \a
        EachLine = 9        // \t
    }

    public class Pointer
    {
        public static uint BaseAddress { get; set; }

        public static void SetBaseAddress(AppConfig config)
        {
            BaseAddress = config.Endianness == Endianness.BigEndian ? 0x817AFE60 : 0xCBD0000;
        }
    }
    
    public class CutsceneHeader
    {
        public int EventID { get; set; }
        public uint MessagePointer { get; set; }
        public int TotalLines { get; set; }
        public static uint Size => 12;

        public CutsceneHeader(int id, uint offset, int total)
        {
            EventID = id;
            MessagePointer = offset;
            TotalLines = total;
        }
    }    

    public class Message
    {
        public int Character { get; set; }
        public Centered? Centered { get; set; }
        public string Text { get; set; }        

        [JsonConstructor]
        public Message() { }
        public Message(int character, Centered? centered, string text)
        {
            Character = character;
            Centered = centered;
            Text = text;
        }
    }

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
