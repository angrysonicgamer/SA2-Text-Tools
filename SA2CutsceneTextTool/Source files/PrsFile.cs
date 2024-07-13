using csharp_prs;
using System.Text;

namespace SA2CutsceneTextTool
{
    public static class PrsFile
    {
        private static readonly Encoding cyrillic = Encoding.GetEncoding(1251);
        
        public static List<List<CsvEventData>> Read(string inputFile, Encoding encoding)
        {
            var decompressedFile = Prs.Decompress(File.ReadAllBytes(inputFile));

            var stream = new MemoryStream(decompressedFile);
            var reader = new BinaryReader(stream);
            
            var eventData = reader.ReadEventData(encoding);
            var csvData = GenerateCsvData(eventData);

            return csvData;
        }

        public static void Write(string outputFile, Dictionary<int, List<Message>> eventData, Encoding encoding)
        {
            var strings = GetCStrings(eventData, encoding);
            var header = CalculateHeader(eventData);
            var messageData = CalculateMessageData(eventData);
            var contents = MergeContents(header, messageData, strings);
            File.WriteAllBytes(outputFile, Prs.Compress(contents, 0x1FFF));
        }


        // PRS file reading

        private static List<Cutscene> ReadHeader(this BinaryReader reader)
        {
            var header = new List<Cutscene>();

            while (true)
            {
                int eventID = reader.ReadInt32(Endianness.BigEndian);
                if (eventID == -1) break;

                uint messagePointer = reader.ReadUInt32(Endianness.BigEndian);
                int totalLines = reader.ReadInt32(Endianness.BigEndian);                

                header.Add(new Cutscene(eventID, messagePointer, totalLines));
            }

            return header;
        }

        private static Dictionary<int, List<Message>> ReadEventData(this BinaryReader reader, Encoding encoding)
        {
            var header = reader.ReadHeader();
            var eventData = new Dictionary<int, List<Message>>();

            foreach (var scene in header)
            {
                var messages = new List<Message>();
                reader.BaseStream.Position = scene.MessagePointer - Pointer.BaseAddress;

                if (scene.TotalLines == 0)
                {
                    int character = reader.ReadInt32(Endianness.BigEndian);
                    messages.Add(new Message(character, ""));
                }

                for (int i = 0; i < scene.TotalLines; i++)
                {
                    int character = reader.ReadInt32(Endianness.BigEndian);
                    uint textOffset = reader.ReadUInt32(Endianness.BigEndian) - Pointer.BaseAddress;

                    long currentPosition = reader.BaseStream.Position;
                    reader.BaseStream.Position = textOffset;
                    string text = reader.ReadCString(encoding);

                    if (encoding == cyrillic)
                        text = text.ConvertToModifiedCodepage();

                    reader.BaseStream.Position = currentPosition;
                    messages.Add(new Message(character, text));
                }

                eventData.Add(scene.EventID, messages);
            }

            return eventData;
        }

        private static List<List<CsvEventData>> GenerateCsvData(Dictionary<int, List<Message>> eventData)
        {
            var csvData = new List<List<CsvEventData>>();

            foreach (var scene in eventData.OrderBy(x => x.Key))
            {
                var messageData = new List<CsvEventData>();

                foreach (var message in scene.Value)
                {
                    string centered = message.Text.StartsWith('\a') ? "+" : "";
                    string text = centered == "+" ? message.Text.Substring(1) : message.Text;
                    messageData.Add(new CsvEventData(scene.Key.ToString(), message.Character.ToString(), centered, text));
                }

                csvData.Add(messageData);
            }

            return csvData;
        }


        // Writing PRS file

        private static List<byte[]> GetCStrings(Dictionary<int, List<Message>> eventData, Encoding encoding)
        {
            var cStrings = new List<byte[]>();

            foreach (var entry in eventData)
            {
                foreach (var message in entry.Value)
                {
                    string text = encoding == cyrillic ? message.Text.ConvertToModifiedCodepage(TextConversionMode.Reversed) : message.Text;
                    var textBytes = new List<byte>();
                    textBytes.AddRange(encoding.GetBytes(text));
                    textBytes.Add(0);
                    cStrings.Add(textBytes.ToArray());
                }
            }

            return cStrings;
        }

        private static List<Cutscene> CalculateHeader(Dictionary<int, List<Message>> eventData)
        {
            uint separatorLength = 12;
            uint pointer = Cutscene.Size * (uint)eventData.Count + separatorLength + Pointer.BaseAddress;
            var header = new List<Cutscene>();

            foreach (var (eventID, messageList) in eventData)
            {
                int messagesCount = messageList.Count;

                if (messagesCount == 0)
                    messagesCount = 1;

                header.Add(new Cutscene(eventID, pointer, messagesCount));
                pointer += (uint)messagesCount * Message.Size;
            }

            return header;
        }

        private static List<(Message, uint)> CalculateMessageData(Dictionary<int, List<Message>> eventData)
        {
            int totalMessagesCount = 0;

            foreach (var (eventID, messageList) in eventData)
            {
                totalMessagesCount += messageList.Count;
            }

            uint separatorLength = 12;
            uint textPointer = Cutscene.Size * (uint)eventData.Count + separatorLength + Message.Size * (uint)totalMessagesCount + Pointer.BaseAddress;
            var messageData = new List<(Message, uint)>();
            
            foreach (var (eventID, messageList) in eventData)
            {
                foreach (var message in messageList)
                {
                    messageData.Add((message, textPointer));
                    textPointer += (uint)message.Text.Length + 1;
                }
            }

            return messageData;
        }

        private static byte[] MergeContents(List<Cutscene> header, List<(Message, uint)> messageData, List<byte[]> strings)
        {
            var contents = new List<byte>();

            foreach (var scene in header)
            {
                contents.AddRange(BitConverter.GetBytes(scene.EventID).Reverse());
                contents.AddRange(BitConverter.GetBytes(scene.MessagePointer).Reverse());
                contents.AddRange(BitConverter.GetBytes(scene.TotalLines).Reverse());
            }

            contents.AddRange(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
            contents.AddRange(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 });

            foreach (var (message, textPointer) in messageData)
            {
                contents.AddRange(BitConverter.GetBytes(message.Character).Reverse());
                contents.AddRange(BitConverter.GetBytes(textPointer).Reverse());
            }

            foreach (var line in strings)
            {
                contents.AddRange(line);
            }

            return contents.ToArray();
        }
    }
}
