namespace SA2CutsceneTextTool
{
    public struct Pointer
    {
        public static uint BaseAddress = 0x817AFE60;
    }
    
    public struct EventHeader
    {
        public int EventID { get; set; }
        public uint MessagePointer { get; set; }
        public int TotalLines { get; set; }
        public static uint Size => 12;

        public EventHeader(int id, uint offset, int total)
        {
            EventID = id;
            MessagePointer = offset;
            TotalLines = total;
        }
    }

    public struct Message
    {
        public int Character { get; set; }
        public string Text { get; set; }
        public static uint Size => 8;

        public Message(int character, string text)
        {
            Character = character;
            Text = text;
        }
    }

    public struct Message_WPtr
    {
        public int Character { get; set; }
        public uint TextPointer { get; set; }

        public Message_WPtr(int character, uint pointer)
        {
            Character = character;
            TextPointer = pointer; ;
        }
    }

    public struct CsvEventData
    {
        public string EventID { get; set; }
        public string Character { get; set; }
        public string Centered { get; set; }
        public string Text { get; set; }

        public CsvEventData(string id, string character, string centered, string text)
        {
            EventID = id;
            Character = character;
            Centered = centered;
            Text = text;
        }
    }
}
