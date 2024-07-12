using csharp_prs;
using System.Text;

namespace SA2EmeraldHintsTool
{
    public static class PrsFile
    {
        public static List<CsvEmeraldHintData> Read(string inputFile, Encoding encoding)
        {
            var decompressedFile = Prs.Decompress(File.ReadAllBytes(inputFile));

            var stream = new MemoryStream(decompressedFile);
            var reader = new BinaryReader(stream);

            var pointers = reader.ReadPointers();
            var contents = reader.ReadContents(pointers, encoding);

            reader.Close();
            return contents;
        }

        public static void Write(string outputFile, List<CsvEmeraldHintData> hints, Encoding encoding)
        {
            var cText = GetCStringsAndPointers(hints, encoding);
            var contents = GetFileContents(cText);
            File.WriteAllBytes(outputFile, Prs.Compress(contents, 0x1FFF));
        }


        // PRS file reading

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

        private static List<CsvEmeraldHintData> ReadContents(this BinaryReader reader, List<int> pointers, Encoding encoding)
        {
            var emeraldHintsList = new List<CsvEmeraldHintData>();

            foreach (var pointer in pointers)
            {
                reader.BaseStream.Position = pointer;
                string contentsAtAddress = reader.ReadCString(encoding).ReplaceKeyboardButtons();

                if (encoding == Encoding.GetEncoding(1251))
                    contentsAtAddress = contentsAtAddress.ConvertToModifiedCodepage();

                string flag2p = contentsAtAddress[1] == 'D' ? "+" : "";
                string text = contentsAtAddress.Substring(contentsAtAddress.IndexOf(' ') + 1);
                string centered = text.StartsWith('\a') ? "+" : "";
                text = text.StartsWith('\a') ? text.Substring(1) : text;

                emeraldHintsList.Add(new CsvEmeraldHintData(flag2p, centered, text));
            }

            return emeraldHintsList;
        }


        // Writing PRS file

        private static List<CStyleText> GetCStringsAndPointers(List<CsvEmeraldHintData> hints, Encoding encoding)
        {
            var cText = new List<CStyleText>();
            int offset = sizeof(int) * hints.Count + sizeof(int);

            foreach (var hint in hints)
            {
                var cString = new List<byte>();
                byte[] controls = hint.Flag2P == "+" ? new byte[] { 0xC, 0x44, 0x20 } : new byte[] { 0xC, 0x20 };
                string text = hint.Text;

                if (text == "{null}")
                    text = "\x0";

                if (encoding == Encoding.GetEncoding(1251))
                    text = text.ConvertToModifiedCodepage(true);

                cString.AddRange(controls);

                if (hint.Centered == "+")
                    cString.Add(0x7);

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
