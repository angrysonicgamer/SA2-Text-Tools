namespace SA2SubtitlesTimingTool
{
    public struct Pointer
    {
        public static uint BaseAddress = 0x817AFE60;
    }

    public struct Cutscene
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

    public struct CsvTimingData
    {
        public string Text { get; set; }
        public int FrameStart { get; set; }
        public uint Duration { get; set; } // if FrameStart = -1, this acts as an X coordinate of the timestamp

        public CsvTimingData(string text, int frameStart, uint duration)
        {
            Text = text;
            FrameStart = frameStart;
            Duration = duration;
        }
    }
}
