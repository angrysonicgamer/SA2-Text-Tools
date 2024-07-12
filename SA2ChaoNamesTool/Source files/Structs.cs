namespace SA2ChaoNamesTool
{
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
