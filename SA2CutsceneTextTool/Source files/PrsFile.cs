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

        public static void Write(string outputFile, Dictionary<int, List<Message>> cutsceneTextData, Encoding encoding)
        {
            var strings = GetCStrings(cutsceneTextData, encoding);
            var textPointers = CalculateTextPointers(strings, cutsceneTextData);
            var dataPointers = CalculateDataPointers(cutsceneTextData);
            var header = CalculateHeader(cutsceneTextData, dataPointers);
            var textData = CalculateTextData(cutsceneTextData, textPointers);
            var contents = MergeContents(header, textData, strings);
            File.WriteAllBytes(outputFile, Prs.Compress(contents, 0x1FFF));
        }


        // PRS file reading

        private static List<EventHeader> ReadHeader(this BinaryReader reader)
        {
            var header = new List<EventHeader>();

            while (true)
            {
                int eventID = reader.ReadInt32(Endianness.BigEndian);
                if (eventID == -1) break;

                uint messagePointer = reader.ReadUInt32(Endianness.BigEndian);
                int totalLines = reader.ReadInt32(Endianness.BigEndian);                

                header.Add(new EventHeader(eventID, messagePointer, totalLines));
            }

            return header;
        }

        private static Dictionary<int, List<Message>> ReadEventData(this BinaryReader reader, Encoding encoding)
        {
            var header = reader.ReadHeader();
            var eventData = new Dictionary<int, List<Message>>();

            foreach (var entry in header)
            {
                var messages = new List<Message>();
                reader.BaseStream.Position = entry.MessagePointer - Pointer.BaseAddress;

                if (entry.TotalLines == 0)
                {
                    int character = reader.ReadInt32(Endianness.BigEndian);
                    messages.Add(new Message(character, ""));
                }

                for (int i = 0; i < entry.TotalLines; i++)
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

                eventData.Add(entry.EventID, messages);
            }

            return eventData;
        }

        private static List<List<CsvEventData>> GenerateCsvData(Dictionary<int, List<Message>> cutsceneTextData)
        {
            var csvData = new List<List<CsvEventData>>();

            foreach (var entry in cutsceneTextData.OrderBy(x => x.Key))
            {
                var entryData = new List<CsvEventData>();

                foreach (var line in entry.Value)
                {
                    string centered = line.Text.StartsWith('\a') ? "+" : "";
                    string text = centered == "+" ? line.Text.Substring(1) : line.Text;
                    entryData.Add(new CsvEventData(entry.Key.ToString(), line.Character.ToString(), centered, text));
                }

                csvData.Add(entryData);
            }

            return csvData;
        }


        // Writing PRS file

        private static List<byte[]> GetCStrings(Dictionary<int, List<Message>> eventData, Encoding encoding)
        {
            var cStrings = new List<byte[]>();

            foreach (var entry in eventData)
            {
                foreach (var line in entry.Value)
                {
                    string text = encoding == cyrillic ? line.Text.ConvertToModifiedCodepage(TextConversionMode.Reversed) : line.Text;
                    var textBytes = new List<byte>();
                    textBytes.AddRange(encoding.GetBytes(text));
                    textBytes.Add(0);
                    cStrings.Add(textBytes.ToArray());
                }
            }

            return cStrings;
        }

        private static List<uint> CalculateTextPointers(List<byte[]> cStrings, Dictionary<int, List<Message>> eventData)
        {
            uint separatorLength = 12;
            uint offset = EventHeader.Size * (uint)eventData.Count + separatorLength + Message.Size * (uint)cStrings.Count + Pointer.BaseAddress;
            var pointers = new List<uint>();

            foreach (var line in cStrings)
            {
                pointers.Add(offset);
                offset += (uint)line.Length;
            }

            return pointers;
        }

        private static List<uint> CalculateDataPointers(Dictionary<int, List<Message>> cutsceneTextData)
        {
            uint offset = 12 * (uint)cutsceneTextData.Count + 12 + Pointer.BaseAddress;
            var pointers = new List<uint>();

            foreach (var entry in cutsceneTextData)
            {
                pointers.Add(offset);
                int count = entry.Value.Count;

                if (count == 0)
                    count = 1;

                offset += (uint)count * 8;
            }

            return pointers;
        }

        private static List<EventHeader> CalculateHeader(Dictionary<int, List<Message>> cutsceneTextData, List<uint> dataPointers)
        {
            var header = new List<EventHeader>();
            int index = 0;

            foreach (var entry in cutsceneTextData)
            {
                header.Add(new EventHeader(entry.Key, dataPointers[index], entry.Value.Count));
                index++;
            }

            return header;
        }

        private static List<Message_WPtr> CalculateTextData(Dictionary<int, List<Message>> cutsceneTextData, List<uint> textPointers)
        {
            var textData = new List<Message_WPtr>();
            int index = 0;

            foreach (var entry in cutsceneTextData)
            {
                foreach (var data in entry.Value)
                {
                    textData.Add(new Message_WPtr(data.Character, textPointers[index]));
                    index++;
                }
            }

            return textData;
        }

        private static byte[] MergeContents(List<EventHeader> header, List<Message_WPtr> textData, List<byte[]> strings)
        {
            var contents = new List<byte>();

            foreach (var entry in header)
            {
                contents.AddRange(BitConverter.GetBytes(entry.EventID).Reverse());
                contents.AddRange(BitConverter.GetBytes(entry.MessagePointer).Reverse());
                contents.AddRange(BitConverter.GetBytes(entry.TotalLines).Reverse());
            }

            contents.AddRange(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
            contents.AddRange(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 });

            foreach (var entry in textData)
            {
                contents.AddRange(BitConverter.GetBytes(entry.Character).Reverse());
                contents.AddRange(BitConverter.GetBytes(entry.TextPointer).Reverse());
            }

            foreach (var line in strings)
            {
                contents.AddRange(line);
            }

            return contents.ToArray();
        }
    }
}
