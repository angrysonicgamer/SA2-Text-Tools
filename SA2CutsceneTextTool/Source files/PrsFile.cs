using csharp_prs;
using System.Text;

namespace SA2CutsceneTextTool
{
    public static class PrsFile
    {
        public static List<Scene> Read(string inputFile, AppConfig config)
        {
            var decompressedFile = Prs.Decompress(File.ReadAllBytes(inputFile));
            return ReadEventData(decompressedFile, config);
        }

        public static void Write(string outputFile, List<Scene> eventData, AppConfig config)
        {
            var strings = GetCStrings(eventData, config);
            var header = CalculateHeader(eventData);
            var messageData = CalculateMessageData(eventData);
            var contents = MergeContents(header, messageData, strings);
            File.WriteAllBytes(outputFile, Prs.Compress(contents, 0x1FFF));
        }


        // PRS file reading

        private static List<CutsceneHeader> ReadHeader(byte[] decompressedFile, Endianness endianness)
        {
            var reader = new BinaryReader(new MemoryStream(decompressedFile));
            var header = new List<CutsceneHeader>();

            while (true)
            {
                int eventID = reader.ReadInt32(endianness);
                if (eventID == -1) break;

                uint messagePointer = reader.ReadUInt32(endianness);
                int totalLines = reader.ReadInt32(endianness);                

                header.Add(new CutsceneHeader(eventID, messagePointer, totalLines));
            }

            reader.Dispose();
            return header;
        }

        private static List<Scene> ReadEventData(byte[] decompressedFile, AppConfig config)
        {
            var reader = new BinaryReader(new MemoryStream(decompressedFile));
            var header = ReadHeader(decompressedFile, config.Endianness);
            var eventData = new List<Scene>();

            foreach (var scene in header)
            {
                var messages = new List<Message>();
                reader.BaseStream.Position = scene.MessagePointer - Pointer.BaseAddress;

                if (scene.TotalLines == 0)
                {
                    int character = reader.ReadInt32(config.Endianness);
                    messages.Add(new Message(character, false, ""));
                }

                for (int i = 0; i < scene.TotalLines; i++)
                {
                    int character = reader.ReadInt32(config.Endianness);
                    uint textOffset = reader.ReadUInt32(config.Endianness) - Pointer.BaseAddress;

                    long currentPosition = reader.BaseStream.Position;
                    reader.BaseStream.Position = textOffset;
                    string text = reader.ReadCString(config.Encoding);

                    if (config.ModifiedCodepage == true)
                        text = text.ConvertToModifiedCodepage();

                    bool centered = text.StartsWith('\a');
                    text = centered ? text.Substring(1) : text;

                    reader.BaseStream.Position = currentPosition;
                    messages.Add(new Message(character, centered, text));
                }

                eventData.Add(new Scene(scene.EventID, messages));
            }

            reader.Dispose();
            return eventData;
        }

        

        // Writing PRS file

        private static List<byte[]> GetCStrings(List<Scene> eventData, AppConfig config)
        {
            var cStrings = new List<byte[]>();

            foreach (var scene in eventData)
            {
                foreach (var message in scene.Messages)
                {
                    string text = config.ModifiedCodepage == true ? message.Text.ConvertToModifiedCodepage(TextConversionMode.Reversed) : message.Text;
                    text = message.Centered ? $"\a{text}" : text;
                    var textBytes = new List<byte>();
                    textBytes.AddRange(config.Encoding.GetBytes(text));
                    textBytes.Add(0);
                    cStrings.Add(textBytes.ToArray());
                }
            }

            return cStrings;
        }

        private static List<CutsceneHeader> CalculateHeader(List<Scene> eventData)
        {
            uint separatorLength = 12;
            uint pointer = CutsceneHeader.Size * (uint)eventData.Count + separatorLength + Pointer.BaseAddress;
            var header = new List<CutsceneHeader>();

            foreach (var scene in eventData)
            {
                int messagesCount = scene.Messages.Count;

                if (messagesCount == 0)
                    messagesCount = 1;

                header.Add(new CutsceneHeader(scene.EventID, pointer, messagesCount));
                pointer += (uint)messagesCount * MessagePrs.Size;
            }

            return header;
        }

        private static List<MessagePrs> CalculateMessageData(List<Scene> eventData)
        {
            int totalMessagesCount = 0;

            foreach (var scene in eventData)
            {
                totalMessagesCount += scene.Messages.Count;
            }

            uint separatorLength = 12;
            uint textPointer = CutsceneHeader.Size * (uint)eventData.Count + separatorLength + MessagePrs.Size * (uint)totalMessagesCount + Pointer.BaseAddress;
            var messageData = new List<MessagePrs>();

            foreach (var scene in eventData)
            {
                foreach (var message in scene.Messages)
                {
                    messageData.Add(new MessagePrs(message.Character, textPointer));
                    textPointer += (uint)message.Text.Length + 1;
                    if (message.Centered)
                        textPointer++;
                }
            }

            return messageData;
        }

        private static byte[] MergeContents(List<CutsceneHeader> header, List<MessagePrs> messageData, List<byte[]> strings)
        {
            var contents = new List<byte>();

            foreach (var scene in header)
            {
                contents.AddRange(BitConverter.GetBytes(scene.EventID).Reverse());
                contents.AddRange(BitConverter.GetBytes(scene.MessagePointer).Reverse());
                contents.AddRange(BitConverter.GetBytes(scene.TotalLines).Reverse());
            }

            contents.AddRange([0xFF, 0xFF, 0xFF, 0xFF]);
            contents.AddRange([0, 0, 0, 0, 0, 0, 0, 0]);

            foreach (var message in messageData)
            {
                contents.AddRange(BitConverter.GetBytes(message.Character).Reverse());
                contents.AddRange(BitConverter.GetBytes(message.TextPointer).Reverse());
            }

            foreach (var line in strings)
            {
                contents.AddRange(line);
            }

            return contents.ToArray();
        }
    }
}
