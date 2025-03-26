using csharp_prs;
using SA2MessageTextTool.Common;
using SA2MessageTextTool.Extensions;

namespace SA2MessageTextTool.PRS
{
    public static partial class PrsFile
    {
        public static MessageFile Read(string prsFile, AppConfig config)
        {
            DisplayMessage.ReadingFile(prsFile);
            var decompressedFile = Prs.Decompress(File.ReadAllBytes(prsFile));
            string fileName = Path.GetFileNameWithoutExtension(prsFile);
            var reader = new BinaryReader(new MemoryStream(decompressedFile));

            var offsets = ReadOffsets(reader, config);
            var messages = new List<List<Message>>();

            if (fileName.StartsWith("eh", StringComparison.OrdinalIgnoreCase))
            {
                messages = ReadEmeraldHints(reader, offsets, config);
            }                
            else if (fileName.StartsWith("mh", StringComparison.OrdinalIgnoreCase))
            {
                messages = ReadGameplayMessages(reader, offsets, config);
            }
            else if (fileName.StartsWith("msgalkinderfoname", StringComparison.OrdinalIgnoreCase))
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
    }
}
