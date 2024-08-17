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
        CSV,
        Both
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
