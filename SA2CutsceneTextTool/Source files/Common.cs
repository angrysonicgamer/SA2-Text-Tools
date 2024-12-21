using System.Text.Json.Serialization;
using static System.Formats.Asn1.AsnWriter;

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

    public enum ExportType
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
    public enum CenteringMethod : short
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
    
    public class EventInfo
    {
        public int EventID { get; set; }
        public uint MessagePointer { get; set; }
        public int TotalMessages { get; set; }
        public static uint Size => 12;

        public EventInfo() { }

        public EventInfo(int id, uint offset, int total)
        {
            EventID = id;
            MessagePointer = offset;
            TotalMessages = total;
        }


        public void Read(BinaryReader reader, Endianness endianness)
        {
            EventID = reader.ReadInt32(endianness);
            if (EventID == -1) return;

            MessagePointer = reader.ReadUInt32(endianness);
            TotalMessages = reader.ReadInt32(endianness);
        }

        public void Write(ref List<byte> writeTo, Endianness endianness)
        {
            byte[] eventID = BitConverter.GetBytes(EventID);
            byte[] messagePtr = BitConverter.GetBytes(MessagePointer);
            byte[] totalLines = BitConverter.GetBytes(TotalMessages);

            if (endianness == Endianness.BigEndian)
            {
                eventID = eventID.Reverse().ToArray();
                messagePtr = messagePtr.Reverse().ToArray();
                totalLines = totalLines.Reverse().ToArray();
            }

            writeTo.AddRange(eventID);
            writeTo.AddRange(messagePtr);
            writeTo.AddRange(totalLines);
        }

        public bool IsValid()
        {
            return EventID >= 0;
        }
    }    

    public class Message
    {
        public int Character { get; set; }
        public CenteringMethod? Centered { get; set; }
        public string Text { get; set; }        

        [JsonConstructor]
        public Message() { }

        public Message(int character, CenteringMethod? centered, string text)
        {
            Character = character;
            Centered = centered;
            Text = text;
        }

        public void Read(BinaryReader reader, AppConfig config)
        {
            Character = reader.ReadInt32(config.Endianness);
            uint textOffset = reader.ReadUInt32(config.Endianness) - Pointer.BaseAddress;
            string text = reader.ReadAt(textOffset, x => x.ReadCString(config.Encoding));

            if (config.ModifiedCodepage == true)
                text = text.ConvertToModifiedCodepage();
                        
            Centered = GetCenteringMethod(text);
            Text = Centered.HasValue ? text.Substring(1) : text;
        }


        private static CenteringMethod? GetCenteringMethod(string text)
        {
            if (text.StartsWith('\a'))
                return CenteringMethod.Block;

            if (text.StartsWith('\t'))
                return CenteringMethod.EachLine;

            return null;
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
