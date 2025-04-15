using SA2CutsceneTextTool.Extensions;

namespace SA2CutsceneTextTool.Common
{
    public class EventInfo
    {
        public int EventID { get; set; }
        public uint MessagePointer { get; set; }
        public int TotalMessages { get; set; }
        public static uint Size => 12;
        public static EventInfo Null => new EventInfo(-1, 0, 0);

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

        public void Write(BinaryWriter writer, Endianness endianness)
        {
            writer.WriteUInt32((uint)EventID, endianness);
            writer.WriteUInt32(MessagePointer, endianness);
            writer.WriteUInt32((uint)TotalMessages, endianness);
        }

        public bool IsValid()
        {
            return EventID >= 0;
        }
    }
}
