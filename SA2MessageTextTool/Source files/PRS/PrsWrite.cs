using csharp_prs;
using System.Text;
using SA2MessageTextTool.Common;
using SA2MessageTextTool.Extensions;

namespace SA2MessageTextTool.PRS
{
    public static partial class PrsFile
    {
        public static void Write(MessageFile data, AppConfig config)
        {
            string fileName = data.Name;
            List<string> strings;

            if (fileName.StartsWith("eh", StringComparison.OrdinalIgnoreCase))
            {
                strings = GetEmeraldHintStrings(data.Messages, config);
            }
            else if (fileName.StartsWith("mh", StringComparison.OrdinalIgnoreCase))
            {
                strings = GetCombinedMessageStrings(data.Messages, config);
            }
            else
            {
                strings = GetSimpleStrings(data.Messages, config);
            }

            bool isChaoNamesFile = fileName.StartsWith("msgalkinderfoname", StringComparison.OrdinalIgnoreCase);
            var binary = WriteDecompressedBinary(strings, config, isChaoNamesFile);

            string destinationFolder = "New files";
            string prsFile = $"{destinationFolder}\\{fileName}.prs";
            Directory.CreateDirectory(destinationFolder);
            File.WriteAllBytes(prsFile, Prs.Compress(binary, 0x1FFF));
            DisplayMessage.Config(config);
            DisplayMessage.FileSaved($"{fileName}.prs");
        }


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

            if (config.UseModifiedCyrillicCP)
                text = text.ModifyCyrillicCP(TextConversionMode.Reversed);

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

        private static byte[] WriteDecompressedBinary(List<string> messages, AppConfig config, bool isChaoNamesFile)
        {
            var binary = new List<byte>();

            WriteOffsets(ref binary, messages, config);
            binary.AddRange(BitConverter.GetBytes(-1));
            WriteStrings(ref binary, messages, config, isChaoNamesFile);

            return binary.ToArray();
        }
    }
}
