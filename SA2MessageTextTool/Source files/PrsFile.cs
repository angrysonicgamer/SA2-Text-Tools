using csharp_prs;
using System.Text;

namespace SA2MessageTextTool
{
    public static class PrsFile
    {
        public static MessageFile Read(string prsFile, AppConfig config)
        {
            DisplayMessage.ReadingFile(prsFile);
            var decompressedFile = Prs.Decompress(File.ReadAllBytes(prsFile));
            string fileName = Path.GetFileNameWithoutExtension(prsFile);
            var reader = new BinaryReader(new MemoryStream(decompressedFile));

            var offsets = ReadOffsets(reader, config);
            var messages = new List<List<Message>>();

            if (fileName.ToLower().StartsWith("eh"))
            {
                messages = ReadEmeraldHints(reader, offsets, config);
            }                
            else if (fileName.ToLower().StartsWith("mh"))
            {
                messages = ReadGameplayMessages(reader, offsets, config);
            }
            else if (fileName.ToLower().StartsWith("msgalkinderfoname"))
            {
                messages = ReadChaoNames(reader, offsets, config);
            }                
            else
            {
                messages = ReadSimpleText(reader, offsets, config);
            }

            reader.Dispose();
            return new MessageFile(fileName, messages);
        }

        public static void Write(MessageFile jsonContents, AppConfig config)
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

        private static List<int> ReadOffsets(BinaryReader reader, AppConfig config)
        {
            var offsets = new List<int>();

            while (true)
            {
                int offset = reader.ReadInt32(config.Endianness);
                if (offset == -1 || offset > reader.BaseStream.Length) break;

                offsets.Add(offset);
            }

            return offsets;
        }

        private static List<List<Message>> ReadGameplayMessages(BinaryReader reader, List<int> offsets, AppConfig config)
        {
            var gameplayMessages = new List<List<Message>>();

            foreach (var offset in offsets)
            {
                string rawText = reader.ReadAt(offset, x => x.ReadCString(config.Encoding));
                string[] lines = rawText.Split(new char[] { '\x0C' }, StringSplitOptions.RemoveEmptyEntries);
                var linesList = new List<Message>();

                foreach (var line in lines)
                {
                    var message = new Message();
                    message.Parse($"\x0C{line}", config);
                    linesList.Add(message);
                }

                gameplayMessages.Add(linesList);
            }

            return gameplayMessages;
        }

        private static List<List<Message>> ReadEmeraldHints(BinaryReader reader, List<int> offsets, AppConfig config)
        {
            var hintsPerPiece = new List<Message>();
            var messagesList = new List<List<Message>>();

            foreach (var offset in offsets)
            {
                var hint = new Message();
                string rawText = reader.ReadAt(offset, x => x.ReadCString(config.Encoding));
                hint.Parse(rawText, config);
                hintsPerPiece.Add(hint);

                if (hintsPerPiece.Count == 3)
                {
                    messagesList.Add(hintsPerPiece);
                    hintsPerPiece = new List<Message>();
                }               
            }

            return messagesList;
        }

        private static List<List<Message>> ReadSimpleText(BinaryReader reader, List<int> offsets, AppConfig config)
        {
            var messagesList = new List<List<Message>>();
            var stringsList = new List<Message>();

            foreach (var offset in offsets)
            {
                var hint = new Message();
                string rawText = reader.ReadAt(offset, x => x.ReadCString(config.Encoding));
                hint.Parse(rawText, config);
                stringsList.Add(hint);
            }

            messagesList.Add(stringsList);

            return messagesList;
        }

        private static List<List<Message>> ReadChaoNames(BinaryReader reader, List<int> offsets, AppConfig config)
        {
            var messagesList = new List<List<Message>>();
            var namesList = new List<Message>();

            ChaoTextConverter.SetCharacterTable(config);

            foreach (var offset in offsets)
            {
                var chaoName = new Message();
                reader.SetPosition(offset);
                chaoName.ReadChaoName(reader);
                namesList.Add(chaoName);
            }

            messagesList.Add(namesList);

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
