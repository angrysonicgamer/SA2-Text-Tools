using csharp_prs;
using System.Text;

namespace SA2SubtitlesTimingTool
{
    public static class PrsFile
    {
        private static readonly Encoding cyrillic = Encoding.GetEncoding(1251);

        public static List<CsvTimingData> Read(string timingFile, string eventTextFile, int eventID, Encoding encoding)
        {
            var timingFileDecompressed = Prs.Decompress(File.ReadAllBytes(timingFile));
            var textFileDecompressed = Prs.Decompress(File.ReadAllBytes(eventTextFile));

            var timingStream = new MemoryStream(timingFileDecompressed);
            var timingReader = new BinaryReader(timingStream);
            var textStream = new MemoryStream(textFileDecompressed);
            var textReader = new BinaryReader(textStream);

            var csvData = new List<CsvTimingData>();
            var messageList = textReader.ReadMessageListOfEventID(eventID, encoding);

            for (int i = 0; i < messageList.Count; i++)
            {
                int frameStart = timingReader.ReadInt32(Endianness.BigEndian);
                uint duration = timingReader.ReadUInt32(Endianness.BigEndian);

                csvData.Add(new CsvTimingData(messageList[i].Text, frameStart, duration));
            }

            return csvData;
        }

        public static void Write(string outputFile, List<CsvTimingData> csvData)
        {
            var outputFileDecompressed = Prs.Decompress(File.ReadAllBytes(outputFile));
            var contents = MergeContents(csvData);

            for (int i = 0; i < contents.Count; i++)
            {
                outputFileDecompressed[i] = contents[i];
            }

            File.WriteAllBytes(outputFile, Prs.Compress(outputFileDecompressed, 0x1FFF));
        }


        // PRS file reading

        private static List<Message> ReadMessageListOfEventID(this BinaryReader reader, int id, Encoding encoding)
        {
            var messages = new List<Message>();
            Cutscene cutscene;

            while (true)
            {
                int eventID = reader.ReadInt32(Endianness.BigEndian);
                uint ptr = reader.ReadUInt32(Endianness.BigEndian) - Pointer.BaseAddress;
                int totalLines = reader.ReadInt32(Endianness.BigEndian);

                if (eventID == id)
                {
                    cutscene = new Cutscene(eventID, ptr, totalLines);
                    break;
                }
            }

            reader.BaseStream.Position = cutscene.MessagePointer;

            for (int i = 0; i < cutscene.TotalLines; i++)
            {
                int character = reader.ReadInt32(Endianness.BigEndian);
                uint textPtr = reader.ReadUInt32(Endianness.BigEndian) - Pointer.BaseAddress;

                long currentPosition = reader.BaseStream.Position;
                reader.BaseStream.Position = textPtr;
                string text = reader.ReadCString(encoding);

                if (encoding == cyrillic)
                    text = text.ConvertToModifiedCodepage();

                messages.Add(new Message(character, text));
                reader.BaseStream.Position = currentPosition;
            }

            return messages;
        }


        // Writing PRS file

        private static List<byte> MergeContents(List<CsvTimingData> csvData)
        {
            var contents = new List<byte>();

            foreach (CsvTimingData data in csvData)
            {
                contents.AddRange(BitConverter.GetBytes(data.FrameStart).Reverse());
                contents.AddRange(BitConverter.GetBytes(data.Duration).Reverse());
            }

            return contents;
        }
    }
}
