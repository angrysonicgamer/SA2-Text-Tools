namespace SA2SubtitlesTimingTool
{
    public class Pointer
    {
        public static uint BaseAddress { get; set; }

        public static void SetBaseAddress(AppConfig config)
        {
            BaseAddress = config.Endianness == Endianness.BigEndian ? 0x817AFE60 : 0xCBD0000;
        }
    }

    public class Cutscene
    {
        public int EventID { get; set; }
        public uint MessagePointer { get; set; }
        public int TotalLines { get; set; }
        public static uint Size => 12;

        public Cutscene(int id, uint offset, int total)
        {
            EventID = id;
            MessagePointer = offset;
            TotalLines = total;
        }
    }

    public class Message
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

    public class CsvSubtitleInfo
    {
        public string Text { get; set; }
        public int FrameStart { get; set; }
        public uint Duration { get; set; } // if FrameStart = -1, this acts as an X coordinate of the timestamp

        public CsvSubtitleInfo() { }

        public CsvSubtitleInfo(string text, int frameStart, uint duration)
        {
            Text = text;
            FrameStart = frameStart;
            Duration = duration;
        }
    }
}
