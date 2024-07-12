namespace SA2MsgFileTextTool
{
    public struct CsvMessageData
    {
        public string Num { get; set; }
        public string Centered { get; set; }
        public string Text { get; set; }        
        public string FrameCount { get; set; }
        public string VoiceID { get; set; }

        public CsvMessageData(string num, string centered, string text, string frameCount, string id)
        {
            Num = num;
            Centered = centered;
            Text = text;            
            FrameCount = frameCount;
            VoiceID = id;
        }
    }

    public struct CStyleText
    {
        public byte[] Text { get; set; }
        public int Pointer { get; set; }

        public CStyleText(byte[] text, int pointer)
        {
            Text = text;
            Pointer = pointer;
        }
    }
}
