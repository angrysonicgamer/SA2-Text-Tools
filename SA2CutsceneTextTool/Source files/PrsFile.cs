using csharp_prs;
using System.Text;

namespace SA2CutsceneTextTool
{
    public class PrsMessage(int character, uint textPtr, string text)
    {
        public int Character => character;
        public uint TextPointer => textPtr;
        public string RawText => text;
        public static uint Size => 8;

        public void WriteMessageData(ref List<byte> writeTo, Endianness endianness)
        {
            byte[] character = BitConverter.GetBytes(Character);
            byte[] textPtr = BitConverter.GetBytes(TextPointer);

            if (endianness == Endianness.BigEndian)
            {
                character = character.Reverse().ToArray();
                textPtr = textPtr.Reverse().ToArray();
            }

            writeTo.AddRange(character);
            writeTo.AddRange(textPtr);
        }

        public void WriteText(ref List<byte> writeTo, Encoding encoding)
        {
            writeTo.AddRange(encoding.GetBytes(RawText));
            writeTo.Add(0);
        }
    }


    public static class PrsFile
    {
        public static EventFile Read(string prsFile, AppConfig config)
        {
            DisplayMessage.ReadingFile(prsFile);
            var decompressedFile = Prs.Decompress(File.ReadAllBytes(prsFile));
            string fileName = Path.GetFileNameWithoutExtension(prsFile);
            var events = ReadEventData(decompressedFile, config);
            return new EventFile(fileName, events);
        }

        public static void Write(EventFile data, AppConfig config)
        {
            string fileName = data.Name;
            var header = GenerateEventInfo(data.Events);
            var messageData = GenerateMessageData(data.Events, config);
            var binary = WriteDecompressedBinary(header, messageData, config);

            string destinationFolder = "New files";
            string prsFile = $"{destinationFolder}\\{fileName}.prs";
            Directory.CreateDirectory(destinationFolder);
            File.WriteAllBytes(prsFile, Prs.Compress(binary, 0x1FFF));
            DisplayMessage.Config(config);
            DisplayMessage.FileSaved($"{fileName}.prs");
        }


        // PRS file reading

        private static List<EventInfo> ReadEventInfoList(BinaryReader reader, Endianness endianness)
        {
            var eventInfoList = new List<EventInfo>();

            while (true)
            {
                var eventInfo = new EventInfo();
                eventInfo.Read(reader, endianness);

                if (eventInfo.IsValid())
                {
                    eventInfoList.Add(eventInfo);
                }                    
                else break;
            }

            return eventInfoList;
        }

        private static List<Scene> ReadEventData(byte[] decompressedFile, AppConfig config)
        {
            var reader = new BinaryReader(new MemoryStream(decompressedFile));
            var sceneInfoList = ReadEventInfoList(reader, config.Endianness);
            var eventData = new List<Scene>();

            foreach (var scene in sceneInfoList)
            {
                var messages = new List<Message>();
                reader.SetPosition(scene.MessagePointer - Pointer.BaseAddress);

                if (scene.TotalMessages == 0)
                {
                    int character = reader.ReadInt32(config.Endianness);
                    messages.Add(new Message(character, null, ""));
                }

                for (int i = 0; i < scene.TotalMessages; i++)
                {                    
                    var message = new Message();
                    message.Read(reader, config);
                    messages.Add(message);
                }

                eventData.Add(new Scene(scene.EventID, messages));
            }

            reader.Dispose();
            return eventData;
        }

        

        // Writing PRS file
                
        private static List<EventInfo> GenerateEventInfo(List<Scene> eventData)
        {
            uint separatorLength = 12;
            uint messagePointer = EventInfo.Size * (uint)eventData.Count + separatorLength + Pointer.BaseAddress;
            var eventInfoList = new List<EventInfo>();

            foreach (var scene in eventData)
            {
                int messagesCount = scene.Messages.Count;

                if (messagesCount == 1 && scene.Messages[0].Text == "")
                    messagesCount = 0;

                eventInfoList.Add(new EventInfo(scene.EventID, messagePointer, messagesCount));
                messagePointer += (uint)scene.Messages.Count * PrsMessage.Size;
            }

            return eventInfoList;
        }

        private static string GetRawString(Message message, AppConfig config)
        {
            string rawText = config.ModifiedCodepage == true ? message.Text.ConvertToModifiedCodepage(TextConversionMode.Reversed) : message.Text;
            rawText = message.Centered.HasValue ? $"{(char)message.Centered}{rawText}" : rawText;

            return rawText;
        }

        private static List<PrsMessage> GenerateMessageData(List<Scene> eventData, AppConfig config)
        {
            int totalMessagesCount = 0;

            foreach (var scene in eventData)
            {
                totalMessagesCount += scene.Messages.Count;
            }

            uint separatorLength = 12;
            uint textPointer = EventInfo.Size * (uint)eventData.Count + separatorLength + PrsMessage.Size * (uint)totalMessagesCount + Pointer.BaseAddress;
            var messageData = new List<PrsMessage>();

            foreach (var scene in eventData)
            {
                foreach (var message in scene.Messages)
                {
                    string rawText = GetRawString(message, config);
                    messageData.Add(new PrsMessage(message.Character, textPointer, rawText));
                    textPointer += (uint)config.Encoding.GetByteCount(rawText) + 1;
                }
            }

            return messageData;
        }

        private static byte[] WriteDecompressedBinary(List<EventInfo> eventInfoList, List<PrsMessage> messageData, AppConfig config)
        {
            var binary = new List<byte>();

            foreach (var scene in eventInfoList)
            {
                scene.Write(ref binary, config.Endianness);
            }

            binary.AddRange([0xFF, 0xFF, 0xFF, 0xFF]);
            binary.AddRange(new byte[8]);

            foreach (var message in messageData)
            {
                message.WriteMessageData(ref binary, config.Endianness);
            }

            foreach (var message in messageData)
            {
                message.WriteText(ref binary, config.Encoding);
            }

            return binary.ToArray();
        }
    }
}
