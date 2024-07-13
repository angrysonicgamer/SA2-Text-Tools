using System.Text;
using csharp_prs;

namespace SA2ChaoNamesTool
{
    public static class PrsFile
    {
        public static List<string> Read(string inputFile)
        {
            var decompressedFile = Prs.Decompress(File.ReadAllBytes(inputFile));

            var stream = new MemoryStream(decompressedFile);
            var reader = new BinaryReader(stream);

            var pointers = reader.ReadPointers();
            var contents = reader.ReadContents(pointers);

            reader.Close();
            return contents;
        }

        public static void Write(string outputFile, string[] strings)
        {
            var cText = GetCStringsAndPointers(strings);
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

        private static List<string> ReadContents(this BinaryReader reader, List<int> pointers)
        {
            var stringsList = new List<string>();

            foreach (var pointer in pointers)
            {
                reader.BaseStream.Position = pointer;
                string stringAtAddress = reader.ReadChaoName();
                stringsList.Add(stringAtAddress);
            }

            return stringsList;
        }


        // Writing PRS file

        private static List<CStyleText> GetCStringsAndPointers(string[] strings)
        {
            var cText = new List<CStyleText>();
            int separatorLength = 4;
            int offset = sizeof(int) * strings.Length + separatorLength;

            foreach (var line in strings)
            {
                var cString = new List<byte>();
                cString.AddRange(TextConversion.ToBytes(line));
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
