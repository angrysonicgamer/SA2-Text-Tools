using csharp_prs;

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
            var messageData = CalculateMessageData(eventData, config);
            var contents = MergeContents(header, messageData, strings, config);

            string destinationFolder = "New files";
            Directory.CreateDirectory(destinationFolder);
            File.WriteAllBytes($"{destinationFolder}\\{outputFile}", Prs.Compress(contents, 0x1FFF));
            DisplayMessage.Config(config);
            DisplayMessage.FileSaved(outputFile);
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
                    messages.Add(new Message(character, null, ""));
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

                    Centered? centered = null;

                    if (text.StartsWith('\a'))
                        centered = Centered.Block;
                    else if (text.StartsWith('\t'))
                        centered = Centered.EachLine;

                    text = centered.HasValue ? text.Substring(1) : text;

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
                    text = message.Centered.HasValue ? $"{(char)message.Centered}{text}" : text;
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

                if (messagesCount == 1 && scene.Messages[0].Text == "")
                    messagesCount = 0;

                header.Add(new CutsceneHeader(scene.EventID, pointer, messagesCount));
                pointer += (uint)scene.Messages.Count * MessagePrs.Size;
            }

            return header;
        }

        private static List<MessagePrs> CalculateMessageData(List<Scene> eventData, AppConfig config)
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
                    textPointer += (uint)config.Encoding.GetByteCount(message.Text) + 1;
                    if (message.Centered.HasValue)
                        textPointer++;
                }
            }

            return messageData;
        }

        private static byte[] MergeContents(List<CutsceneHeader> header, List<MessagePrs> messageData, List<byte[]> strings, AppConfig config)
        {
            var contents = new List<byte>();

            foreach (var scene in header)
            {
                byte[] eventID = BitConverter.GetBytes(scene.EventID);
                byte[] messagePtr = BitConverter.GetBytes(scene.MessagePointer);
                byte[] totalLines = BitConverter.GetBytes(scene.TotalLines);

                if (config.Endianness == Endianness.BigEndian)
                {
                    eventID = eventID.Reverse().ToArray();
                    messagePtr = messagePtr.Reverse().ToArray();
                    totalLines = totalLines.Reverse().ToArray();
                }

                contents.AddRange(eventID);
                contents.AddRange(messagePtr);
                contents.AddRange(totalLines);
            }

            contents.AddRange([0xFF, 0xFF, 0xFF, 0xFF]);
            contents.AddRange([0, 0, 0, 0, 0, 0, 0, 0]);

            foreach (var message in messageData)
            {
                byte[] character = BitConverter.GetBytes(message.Character);
                byte[] textPtr = BitConverter.GetBytes(message.TextPointer);

                if (config.Endianness == Endianness.BigEndian)
                {
                    character = character.Reverse().ToArray();
                    textPtr = textPtr.Reverse().ToArray();
                }

                contents.AddRange(character);
                contents.AddRange(textPtr);
            }

            foreach (var line in strings)
            {
                contents.AddRange(line);
            }

            return contents.ToArray();
        }
    }
}
