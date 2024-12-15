using csharp_prs;
using System.Text;

namespace SA2SubtitlesTimingTool
{
    public static class GameFile
    {
        private static readonly Encoding cyrillic = Encoding.GetEncoding(1251);


        public static List<CsvSubtitleInfo> Read(string timingFile, Encoding encoding, AppConfig config)
        {
            string fileName = Path.GetFileNameWithoutExtension(timingFile);
            string extension = Path.GetExtension(timingFile).ToLower();
            var timingFileDecompressed = extension == ".prs" ? Prs.Decompress(File.ReadAllBytes(timingFile)) : File.ReadAllBytes(timingFile);
            var reader = new BinaryReader(new MemoryStream(timingFileDecompressed));

            int eventID = GetEventID(fileName);
            string eventTextFile = GetEventTextFileName(fileName, eventID);            

            var csvData = new List<CsvSubtitleInfo>();
            var messageList = ReadMessageList(eventTextFile, eventID, encoding, config);

            for (int i = 0; i < messageList.Count; i++)
            {
                int frameStart = reader.ReadInt32(config.Endianness);
                uint duration = reader.ReadUInt32(config.Endianness);

                csvData.Add(new CsvSubtitleInfo(messageList[i].Text, frameStart, duration));
            }

            reader.Dispose();
            return csvData;
        }

        public static void Write(string timingFile, List<CsvSubtitleInfo> csvData, AppConfig config)
        {
            string extension = Path.GetExtension(timingFile).ToLower();
            var outputFileDecompressed = extension == ".prs" ? Prs.Decompress(File.ReadAllBytes(timingFile)) : File.ReadAllBytes(timingFile);
            var contents = MergeContents(csvData, config);

            for (int i = 0; i < contents.Count; i++)
            {
                outputFileDecompressed[i] = contents[i];
            }

            if (extension == ".prs")
            {
                File.WriteAllBytes(timingFile, Prs.Compress(outputFileDecompressed, 0x1FFF));
            }
            else // if .scr
            {
                File.WriteAllBytes(timingFile, outputFileDecompressed);
            }

            DisplayMessage.Config(config);
            DisplayMessage.FileOverwritten(timingFile);
        }


        // PRS file reading

        private static int GetEventID(string fileName)
        {
            return int.Parse(fileName.Substring(fileName.IndexOf('0'), 4));
        }

        private static string GetEventTextFileName(string fileName, int eventID)
        {
            char language = fileName.ToLower().Last();

            if (language == 'j')
                language = '0';

            string story = "";

            if (eventID < 100)                              // hero story
                story = "H";
            else if (eventID >= 100 && eventID < 200)       // dark story
                story = "D";
            else if (eventID >= 200)                        // last story
                story = "L";

            return $"evmes{story}{language}.prs";
        }

        private static List<Message> ReadMessageList(string eventTextFile, int id, Encoding encoding, AppConfig config)
        {
            if (!File.Exists(eventTextFile))
            {
                throw new Exception($"Corresponding event text file {eventTextFile} not found.\n");
            }
            
            var textFileDecompressed = Prs.Decompress(File.ReadAllBytes(eventTextFile));
            var reader = new BinaryReader(new MemoryStream(textFileDecompressed));

            var messages = new List<Message>();
            Cutscene cutscene;

            while (true)
            {
                int eventID = reader.ReadInt32(config.Endianness);
                uint ptr = reader.ReadUInt32(config.Endianness) - Pointer.BaseAddress;
                int totalLines = reader.ReadInt32(config.Endianness);

                if (eventID == id)
                {
                    cutscene = new Cutscene(eventID, ptr, totalLines);
                    break;
                }
            }

            reader.BaseStream.Position = cutscene.MessagePointer;

            for (int i = 0; i < cutscene.TotalLines; i++)
            {
                int character = reader.ReadInt32(config.Endianness);
                uint textPtr = reader.ReadUInt32(config.Endianness) - Pointer.BaseAddress;

                long currentPosition = reader.BaseStream.Position;
                reader.BaseStream.Position = textPtr;
                string text = reader.ReadCString(encoding);

                if (encoding == cyrillic)
                    text = text.ConvertToModifiedCodepage();

                messages.Add(new Message(character, text));
                reader.BaseStream.Position = currentPosition;
            }

            reader.Dispose();

            return messages;
        }


        // Writing PRS file

        private static List<byte> MergeContents(List<CsvSubtitleInfo> csvData, AppConfig config)
        {
            var contents = new List<byte>();

            foreach (CsvSubtitleInfo data in csvData)
            {
                if (config.Endianness == Endianness.BigEndian)
                {
                    contents.AddRange(BitConverter.GetBytes(data.FrameStart).Reverse());
                    contents.AddRange(BitConverter.GetBytes(data.Duration).Reverse());
                }
                else
                {
                    contents.AddRange(BitConverter.GetBytes(data.FrameStart));
                    contents.AddRange(BitConverter.GetBytes(data.Duration));
                }
            }

            return contents;
        }
    }
}
