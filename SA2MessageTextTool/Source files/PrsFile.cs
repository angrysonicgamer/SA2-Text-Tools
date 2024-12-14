using csharp_prs;
using System.Text;

namespace SA2MessageTextTool
{
    public static class PrsFile
    {
        public static JsonContents Read(string prsFile, AppConfig config)
        {
            var decompressedFile = Prs.Decompress(File.ReadAllBytes(prsFile));
            string fileName = Path.GetFileNameWithoutExtension(prsFile);

            var pointers = ReadOffsets(decompressedFile, config);
            var messages = new List<List<Message>>();

            if (fileName.ToLower().StartsWith("eh"))
                messages = ReadEmeraldHints(decompressedFile, pointers, config);
            else if (fileName.ToLower().StartsWith("mh"))
                messages = ReadMessages(decompressedFile, pointers, config);
            else if (fileName.ToLower().StartsWith("msgalkinderfoname"))
                messages = ReadChaoNames(decompressedFile, pointers, config);
            else
                messages = ReadSimpleText(decompressedFile, pointers, config);

            return new JsonContents(fileName, messages);
        }

        public static void Write(JsonContents jsonContents, AppConfig config)
        {
            string fileName = jsonContents.Name;
            var strings = new List<string>();

            if (fileName.ToLower().StartsWith("eh"))
                strings = GetEmeraldHintStrings(jsonContents.Messages);
            else if (fileName.ToLower().StartsWith("mh"))
                strings = GetCombinedMessageStrings(jsonContents.Messages);
            else
                strings = GetSimpleStrings(jsonContents.Messages);

            bool isChaoNames = fileName.StartsWith("MsgAlKinderFoName");

            var cText = GetCStringsAndPointers(strings, config, isChaoNames);
            var contents = GetFileContents(cText, config);

            string destinationFolder = "New files";
            string prsFile = $"{destinationFolder}\\{fileName}.prs";
            Directory.CreateDirectory(destinationFolder);
            File.WriteAllBytes(prsFile, Prs.Compress(contents, 0x1FFF));
            DisplayMessage.Config(config);
            DisplayMessage.FileSaved($"{fileName}.prs");
        }


        // Reading PRS file

        private static List<int> ReadOffsets(byte[] decompressedFile, AppConfig config)
        {
            var reader = new BinaryReader(new MemoryStream(decompressedFile));
            var offsets = new List<int>();

            while (true)
            {
                int offset = reader.ReadInt32(config.Endianness);
                if (offset == -1 || offset > reader.BaseStream.Length) break;

                offsets.Add(offset);
            }

            reader.Dispose();
            return offsets;
        }

        private static List<List<Message>> ReadMessages(byte[] decompressedFile, List<int> offsets, AppConfig config)
        {
            var reader = new BinaryReader(new MemoryStream(decompressedFile));
            var messagesList = new List<List<Message>>();

            foreach (var offset in offsets)
            {
                reader.BaseStream.Position = offset;
                string contents = reader.ReadCString(config.Encoding).ReplaceKeyboardButtons();

                if (config.ModifiedCodepage == true)
                    contents = contents.ConvertToModifiedCodepage();

                string[] lines = contents.Split(new char[] { '\x0C' }, StringSplitOptions.RemoveEmptyEntries);
                var linesList = new List<Message>();

                foreach (var line in lines)
                {
                    string controls = line.Substring(0, line.IndexOf(' '));

                    string? voice = controls.IndexOf('s') != -1 ? controls.Substring(controls.IndexOf('s') + 1, controls.IndexOf('w') - controls.IndexOf('s') - 1) : null;
                    int? voiceID = voice != null ? int.Parse(voice) : null;

                    string? frameCount = controls.IndexOf('w') != -1 ? controls.Substring(controls.IndexOf('w') + 1) : null;
                    int? duration = frameCount != null ? int.Parse(frameCount) : null;

                    string text = line.Substring(line.IndexOf(' ') + 1);
                    Centered? centered = null;

                    if (text.StartsWith('\a'))
                        centered = Centered.Block;
                    else if (text.StartsWith('\t'))
                        centered = Centered.EachLine;

                    text = centered.HasValue ? text.Substring(1) : text;
                    linesList.Add(new Message(voiceID, duration, null, centered, text));
                }

                messagesList.Add(linesList);
            }

            return messagesList;
        }

        private static List<List<Message>> ReadEmeraldHints(byte[] decompressedFile, List<int> pointers, AppConfig config)
        {
            var reader = new BinaryReader(new MemoryStream(decompressedFile));
            var hintsPerPiece = new List<Message>();
            var messagesList = new List<List<Message>>();

            foreach (var pointer in pointers)
            {
                reader.BaseStream.Position = pointer;
                string contents = reader.ReadCString(config.Encoding).ReplaceKeyboardButtons();

                if (config.ModifiedCodepage == true)
                    contents = contents.ConvertToModifiedCodepage();

                string controls = contents.Substring(0, contents.IndexOf(' '));

                bool? is2p = controls.IndexOf('D') != -1 ? true : null;
                string text = contents.Substring(contents.IndexOf(' ') + 1);

                Centered? centered = null;

                if (text.StartsWith('\a'))
                    centered = Centered.Block;
                else if (text.StartsWith('\t'))
                    centered = Centered.EachLine;

                text = centered.HasValue ? text.Substring(1) : text;

                hintsPerPiece.Add(new Message(null, null, is2p, centered, text));

                if (hintsPerPiece.Count == 3)
                {
                    messagesList.Add(hintsPerPiece);
                    hintsPerPiece = new List<Message>();
                }               
            }

            return messagesList;
        }

        private static List<List<Message>> ReadSimpleText(byte[] decompressedFile, List<int> pointers, AppConfig config)
        {
            var reader = new BinaryReader(new MemoryStream(decompressedFile));
            var messagesList = new List<List<Message>>();
            var linesList = new List<Message>();

            foreach (var pointer in pointers)
            {
                reader.BaseStream.Position = pointer;
                string text = reader.ReadCString(config.Encoding).ReplaceKeyboardButtons();

                if (config.ModifiedCodepage == true)
                    text = text.ConvertToModifiedCodepage();

                Centered? centered = null;

                if (text.StartsWith('\a'))
                    centered = Centered.Block;
                else if (text.StartsWith('\t'))
                    centered = Centered.EachLine;

                text = centered.HasValue ? text.Substring(1) : text;

                linesList.Add(new Message(null, null, null, centered, text));
            }

            messagesList.Add(linesList);

            return messagesList;
        }

        private static List<List<Message>> ReadChaoNames(byte[] decompressedFile, List<int> pointers, AppConfig config)
        {
            var reader = new BinaryReader(new MemoryStream(decompressedFile));
            var messagesList = new List<List<Message>>();
            var linesList = new List<Message>();

            ChaoTextConverter.SetCharacterTable(config);

            foreach (var pointer in pointers)
            {
                reader.BaseStream.Position = pointer;
                string text = reader.ReadChaoName();

                Centered? centered = null;

                if (text.StartsWith('\a'))
                    centered = Centered.Block;
                else if (text.StartsWith('\t'))
                    centered = Centered.EachLine;

                text = centered.HasValue ? text.Substring(1) : text;
                linesList.Add(new Message(null, null, null, centered, text));
            }

            messagesList.Add(linesList);

            return messagesList;
        }


        // Writing PRS
        
        private static List<string> GetCombinedMessageStrings(List<List<Message>> jsonContents)
        {
            var combinedStrings = new List<string>();

            foreach (var group in jsonContents)
            {
                var builder = new StringBuilder();

                foreach (var line in group)
                {
                    builder.Append('\x0C');

                    if (line.Voice.HasValue)
                        builder.Append($"s{line.Voice}");

                    if (line.Duration.HasValue)
                        builder.Append($"w{line.Duration}");

                    builder.Append(' ');

                    if (line.Centered.HasValue)
                        builder.Append((char)line.Centered);

                    builder.Append(line.Text);
                }

                string text = builder.ToString();
                combinedStrings.Add(text);
            }

            return combinedStrings;
        }

        private static List<string> GetEmeraldHintStrings(List<List<Message>> jsonContents)
        {
            var emeraldHints = new List<string>();

            foreach (var hintsPerPiece in jsonContents)
            {
                foreach (var hint in hintsPerPiece)
                {
                    var builder = new StringBuilder();

                    builder.Append('\x0C');

                    if (hint.Is2PPiece.HasValue)
                        builder.Append('D');

                    builder.Append(' ');

                    if (hint.Centered.HasValue)
                        builder.Append((char)hint.Centered);

                    builder.Append(hint.Text);

                    string text = builder.ToString();
                    emeraldHints.Add(text);
                }
            }

            return emeraldHints;
        }

        private static List<string> GetSimpleStrings(List<List<Message>> jsonContents)
        {
            var strings = new List<string>();

            foreach (var group in jsonContents)
            {
                foreach (var line in group)
                {
                    var builder = new StringBuilder();

                    if (line.Centered.HasValue)
                        builder.Append((char)line.Centered);

                    builder.Append(line.Text);

                    string text = builder.ToString();
                    strings.Add(text);
                }
            }

            return strings;
        }


        private static List<CStyleText> GetCStringsAndPointers(List<string> strings, AppConfig config, bool isChaoNames = false)
        {
            var cText = new List<CStyleText>();
            int separatorLength = 4;
            int offset = sizeof(int) * strings.Count + separatorLength;

            foreach (var line in strings)
            {
                var cString = new List<byte>();
                string text = line;

                if (config.ModifiedCodepage == true)
                    text = text.ConvertToModifiedCodepage(TextConversionMode.Reversed);

                text = text.ReplaceKeyboardButtons(TextConversionMode.Reversed);

                byte[] textBytes;
                
                if (isChaoNames)
                {
                    ChaoTextConverter.SetCharacterTable(config);
                    textBytes = ChaoTextConverter.ToBytes(text);
                }
                else
                {
                    textBytes = config.Encoding.GetBytes(text);
                }

                cString.AddRange(textBytes);
                cString.Add(0);
                cText.Add(new CStyleText(cString.ToArray(), offset));
                offset += cString.Count;
            }

            return cText;
        }

        private static byte[] GetFileContents(List<CStyleText> cText, AppConfig config)
        {
            var contents = new List<byte>();

            foreach (var entry in cText)
            {
                byte[] offsetBytes = config.Endianness == Endianness.BigEndian ? BitConverter.GetBytes(entry.Offset).Reverse().ToArray() : BitConverter.GetBytes(entry.Offset);
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
