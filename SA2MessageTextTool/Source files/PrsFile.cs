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

        public static void Write(MessageFile data, AppConfig config)
        {
            string fileName = data.Name;
            List<string> strings;

            if (fileName.ToLower().StartsWith("eh"))
            {
                strings = GetEmeraldHintStrings(data.Messages, config);
            }                
            else if (fileName.ToLower().StartsWith("mh"))
            {
                strings = GetCombinedMessageStrings(data.Messages, config);
            }                
            else
            {
                strings = GetSimpleStrings(data.Messages, config);
            }                

            bool isChaoNames = fileName.ToLower().StartsWith("msgalkinderfoname");
            var binary = WriteDecompressedBinary(strings, config, isChaoNames);

            string destinationFolder = "New files";
            string prsFile = $"{destinationFolder}\\{fileName}.prs";
            Directory.CreateDirectory(destinationFolder);
            File.WriteAllBytes(prsFile, Prs.Compress(binary, 0x1FFF));
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
        
        private static string GetRawString(Message message, AppConfig config)
        {
            var builder = new StringBuilder();
            bool hasControls = message.Voice.HasValue || message.Duration.HasValue || message.Is2PPiece.HasValue;

            if (hasControls)
            {
                builder.Append('\x0C');

                if (message.Voice.HasValue)
                {
                    builder.Append($"s{message.Voice}");
                }                    

                if (message.Duration.HasValue)
                {
                    builder.Append($"w{message.Duration}");
                }                    

                if (message.Is2PPiece.HasValue)
                {
                    builder.Append('D');
                }                   

                builder.Append(' ');
            }            

            if (message.Centered.HasValue)
            {
                builder.Append((char)message.Centered);
            }                

            builder.Append(message.Text);
            string text = builder.ToString();

            if (config.ModifiedCodepage == true)
                text = text.ConvertToModifiedCodepage(TextConversionMode.Reversed);
            
            text = text.ReplaceKeyboardButtons(TextConversionMode.Reversed);
            return text;
        } 

        private static List<string> GetCombinedMessageStrings(List<List<Message>> messages, AppConfig config)
        {
            var combinedStrings = new List<string>();

            foreach (var group in messages)
            {
                var builder = new StringBuilder();

                foreach (var line in group)
                {
                    builder.Append(GetRawString(line, config));
                }

                string text = builder.ToString();
                combinedStrings.Add(text);
            }

            return combinedStrings;
        }

        private static List<string> GetEmeraldHintStrings(List<List<Message>> messages, AppConfig config)
        {
            var emeraldHints = new List<string>();

            foreach (var hintsPerPiece in messages)
            {
                foreach (var hint in hintsPerPiece)
                {
                    emeraldHints.Add(GetRawString(hint, config));
                }
            }

            return emeraldHints;
        }

        private static List<string> GetSimpleStrings(List<List<Message>> messages, AppConfig config)
        {
            var strings = new List<string>();

            foreach (var group in messages)
            {
                foreach (var line in group)
                {
                    strings.Add(GetRawString(line, config));
                }
            }

            return strings;
        }

        private static void WriteOffsets(ref List<byte> writeTo, List<string> messages, AppConfig config)
        {
            int separatorLength = 4;
            int offset = messages.Count * sizeof(int) + separatorLength;

            foreach (var message in messages)
            {
                byte[] offsetBytes = config.Endianness == Endianness.BigEndian ? BitConverter.GetBytes(offset).Reverse().ToArray() : BitConverter.GetBytes(offset);
                writeTo.AddRange(offsetBytes);
                offset += config.Encoding.GetByteCount(message) + 1;
            }
        }

        private static void WriteStrings(ref List<byte> writeTo, List<string> messages, AppConfig config, bool isChaoNames)
        {
            foreach (var message in messages)
            {
                byte[] textBytes;

                if (isChaoNames)
                {
                    ChaoTextConverter.SetCharacterTable(config);
                    textBytes = ChaoTextConverter.ToBytes(message);
                }
                else
                {
                    textBytes = config.Encoding.GetBytes(message);
                }

                writeTo.AddRange(textBytes);
                writeTo.Add(0);
            }
        }

        private static byte[] WriteDecompressedBinary(List<string> messages, AppConfig config, bool isChaoNames)
        {
            var binary = new List<byte>();

            WriteOffsets(ref binary, messages, config);
            binary.AddRange(BitConverter.GetBytes(-1));
            WriteStrings(ref binary, messages, config, isChaoNames);

            return binary.ToArray();
        }
    }
}
