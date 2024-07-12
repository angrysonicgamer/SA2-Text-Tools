namespace SA2SimpleTextTool
{
    public struct CsvData
    {
        public string Text { get; set; }

        public CsvData(string text)
        {
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
