namespace SA2EmeraldHintsTool
{
    public struct CsvEmeraldHintData
    {
        public string Flag2P { get; set; }
        public string Centered { get; set; }
        public string Text { get; set; }

        public CsvEmeraldHintData(string flag2p, string centered, string text)
        {
            Flag2P = flag2p;
            Centered = centered;
            Text = text;
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
