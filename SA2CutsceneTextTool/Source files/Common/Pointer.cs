namespace SA2CutsceneTextTool.Common
{
    public class Pointer
    {
        public static uint BaseAddress { get; set; }

        public static void SetBaseAddress(AppConfig config)
        {
            BaseAddress = config.Endianness == Endianness.BigEndian ? 0x817AFE60 : 0xCBD0000;
        }
    }
}
