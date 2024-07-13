namespace SA2SimpleTextTool
{
    public struct CsvData
    {
        public string Centered { get; set; }
        public string Text { get; set; }

        public CsvData(string centered, string text)
        {
            Centered = centered;
            Text = text;
        }
    }

    public struct CStyleText
    {
        public byte[] Text {  get; set; }
        public int Pointer { get; set; }

        public CStyleText(byte[] text, int pointer)
        {
            Text = text;
            Pointer = pointer;
        }
    }
}
