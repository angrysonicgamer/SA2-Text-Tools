using SA2CutsceneTextTool.Extensions;

namespace SA2CutsceneTextTool.Common
{
    public class EventInfo
    {
        public int EventID { get; set; }
        public uint MessagePointer { get; set; }
        public int TotalMessages { get; set; }
        public static uint Size => 12;

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

        public void Write(ref List<byte> writeTo, Endianness endianness)
        {
            byte[] eventID = BitConverter.GetBytes(EventID);
            byte[] messagePtr = BitConverter.GetBytes(MessagePointer);
            byte[] totalLines = BitConverter.GetBytes(TotalMessages);

            if (endianness == Endianness.BigEndian)
            {
                eventID = eventID.Reverse().ToArray();
                messagePtr = messagePtr.Reverse().ToArray();
                totalLines = totalLines.Reverse().ToArray();
            }

            writeTo.AddRange(eventID);
            writeTo.AddRange(messagePtr);
            writeTo.AddRange(totalLines);
        }

        public bool IsValid()
        {
            return EventID >= 0;
        }
    }
}
