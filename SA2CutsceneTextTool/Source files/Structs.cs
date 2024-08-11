using System.Text.Json.Serialization;

namespace SA2CutsceneTextTool
{
    public struct Pointer
    {
        public static uint BaseAddress = 0x817AFE60;
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

    public class Message
    {
        public int Character { get; set; }
        public bool Centered { get; set; }
        public string Text { get; set; }        

        [JsonConstructor]
        public Message() { }
        public Message(int character, bool centered, string text)
        {
            Character = character;
            Centered = centered;
            Text = text;
        }
    }

    public class MessagePrs
    {
        public int Character { get; set; }
        public uint TextPointer { get; set; }
        public static uint Size => 8;

        public MessagePrs(int character, uint textPtr)
        {
            Character = character;
            TextPointer = textPtr;
        }
    }

    public struct CsvMessage
    {
        public string EventID { get; set; }
        public string Character { get; set; }
        public string Centered { get; set; }
        public string Text { get; set; }

        public CsvMessage(string id, string character, string centered, string text)
        {
            EventID = id;
            Character = character;
            Centered = centered;
            Text = text;
        }
    }
}
