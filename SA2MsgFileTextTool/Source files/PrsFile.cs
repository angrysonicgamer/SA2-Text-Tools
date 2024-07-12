using csharp_prs;
using System.Text;

namespace SA2MsgFileTextTool
{
    public static class PrsFile
    {
        public static List<List<CsvMessageData>> Read(string inputFile, Encoding encoding)
        {
            var decompressedFile = Prs.Decompress(File.ReadAllBytes(inputFile));

            var stream = new MemoryStream(decompressedFile);
            var reader = new BinaryReader(stream);

            var pointers = reader.ReadPointers();
            var contents = reader.ReadContents(pointers, encoding);

            reader.Close();
            return contents;
        }

        public static void Write(string outputFile, List<List<CsvMessageData>> groupedRecords, Encoding encoding)
        {
            var combinedStrings = GetCombinedStrings(groupedRecords);
            var cText = GetCStringsAndPointers(combinedStrings, encoding);
            var contents = GetFileContents(cText);
            File.WriteAllBytes(outputFile, Prs.Compress(contents, 0x1FFF));
        }


        // Reading PRS file

        private static List<int> ReadPointers(this BinaryReader reader)
        {
            var pointers = new List<int>();

            while (true)
            {
                int pointer = reader.ReadInt32BigEndian();
                if (pointer == -1) break;

                pointers.Add(pointer);
            }

            return pointers;
        }

        private static List<List<CsvMessageData>> ReadContents(this BinaryReader reader, List<int> pointers, Encoding encoding)
        {
            var allMessagesList = new List<List<CsvMessageData>>();
            int lineNumber = 0;

            foreach (var pointer in pointers)
            {
                reader.BaseStream.Position = pointer;
                string contentsAtAddress = reader.ReadCString(encoding).ReplaceKeyboardButtons();

                if (encoding == Encoding.GetEncoding(1251))
                    contentsAtAddress = contentsAtAddress.ConvertToModifiedCodepage();

                string[] blocks = contentsAtAddress.Split(new char[] { '\xC' }, StringSplitOptions.RemoveEmptyEntries);

                var messageList = new List<CsvMessageData>();
                foreach (var block in blocks)
                {
                    string controls = block.Substring(0, block.IndexOf(' '));
                    string id = controls.IndexOf('s') != -1 ? controls.Substring(controls.IndexOf('s') + 1, controls.IndexOf('w') - controls.IndexOf('s') - 1) : "";
                    string frameCount = controls.Substring(controls.IndexOf('w') + 1);
                    string text = block.Substring(block.IndexOf(' ') + 1);
                    string centered = text.StartsWith('\a') ? "+" : "";
                    text = text.StartsWith('\a')? text.Substring(1) : text;

                    messageList.Add(new CsvMessageData(lineNumber.ToString(), centered, text, frameCount, id));
                }

                allMessagesList.Add(messageList);
                lineNumber++;
            }

            return allMessagesList;
        }


        // Writing PRS
        
        private static List<string> GetCombinedStrings(List<List<CsvMessageData>> groupedRecords)
        {
            var combinedStrings = new List<string>();

            foreach (var group in groupedRecords)
            {
                var builder = new StringBuilder();

                foreach (var line in group)
                {
                    builder.Append('\x0C');

                    if (line.VoiceID != "")
                        builder.Append($"s{line.VoiceID}");

                    builder.Append($"w{line.FrameCount} ");

                    if (line.Centered == "+")
                        builder.Append('\a');

                    builder.Append(line.Text);
                }

                string text = builder.ToString().ReplaceKeyboardButtons(true); 
                combinedStrings.Add(text);
            }

            return combinedStrings;
        }        

        private static List<CStyleText> GetCStringsAndPointers(List<string> strings, Encoding encoding)
        {
            var cText = new List<CStyleText>();
            int offset = sizeof(int) * strings.Count + sizeof(int);

            foreach (var line in strings)
            {
                var cString = new List<byte>();
                string text = line;

                if (text == "{null}")
                    text = "\x0";

                if (encoding == Encoding.GetEncoding(1251))
                    text = text.ConvertToModifiedCodepage(true);

                cString.AddRange(encoding.GetBytes(text));
                cString.Add(0);
                cText.Add(new CStyleText(cString.ToArray(), offset));
                offset += cString.Count;
            }

            return cText;
        }

        private static byte[] GetFileContents(List<CStyleText> cText)
        {
            var contents = new List<byte>();

            foreach (var entry in cText)
            {
                byte[] offsetBytes = BitConverter.GetBytes(entry.Pointer).Reverse().ToArray();
                contents.AddRange(offsetBytes);
            }

            contents.AddRange(BitConverter.GetBytes(-1));

            foreach (var entry in cText)
            {
                contents.AddRange(entry.Text);
            }

            return contents.ToArray();
        }
    }
}
