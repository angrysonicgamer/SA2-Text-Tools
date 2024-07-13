using csharp_prs;
using System.Text;

namespace SA2SimpleTextTool
{
    public static class PrsFile
    {
        private static readonly Encoding cyrillic = Encoding.GetEncoding(1251);

        public static List<CsvData> Read(string inputFile, Encoding encoding)
        {
            var decompressedFile = Prs.Decompress(File.ReadAllBytes(inputFile));

            var stream = new MemoryStream(decompressedFile);
            var reader = new BinaryReader(stream);
            
            var pointers = reader.ReadPointers();
            var contents = reader.ReadContents(pointers, encoding);

            reader.Close();
            return contents;
        }

        public static void Write(string outputFile, List<string> strings, Encoding encoding)
        {
            var cText = GetCStringsAndPointers(strings, encoding);
            var contents = GetFileContents(cText);
            File.WriteAllBytes(outputFile, Prs.Compress(contents, 0x1FFF));
        }


        // Reading PRS file

        private static List<int> ReadPointers(this BinaryReader reader)
        {
            var pointers = new List<int>();

            while (true)
            {
                int pointer = reader.ReadInt32(Endianness.BigEndian);
                if (pointer == -1 || pointer > reader.BaseStream.Length) break;

                pointers.Add(pointer);
            }

            return pointers;
        }

        private static List<CsvData> ReadContents(this BinaryReader reader, List<int> pointers, Encoding encoding)
        {
            var stringsList = new List<CsvData>();

            foreach (var pointer in pointers)
            {
                reader.BaseStream.Position = pointer;
                string text = reader.ReadCString(encoding);

                string centered = text.StartsWith('\a') ? "+" : "";
                text = text.StartsWith('\a') ? text.Substring(1) : text;

                if (encoding == cyrillic)
                    text = text.ConvertToModifiedCodepage();

                if (text.StartsWith('\x0'))
                    text = "{null}";

                stringsList.Add(new CsvData(centered, text));
            }

            return stringsList;
        }


        // Writing PRS file

        private static List<CStyleText> GetCStringsAndPointers(List<string> strings, Encoding encoding)
        {
            var cText = new List<CStyleText>();
            int separatorLength = 4;
            int offset = sizeof(int) * strings.Count + separatorLength;

            foreach (var line in strings)
            {
                var cString = new List<byte>();
                string text = line;

                if (text == "{null}")
                    text = "\x0";

                if (encoding == cyrillic)
                    text = text.ConvertToModifiedCodepage(TextConversionMode.Reversed);

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
