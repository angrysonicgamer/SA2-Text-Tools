﻿using csharp_prs;
using System.Text;

namespace SA2MsgFileTextTool
{
    public static class PrsFile
    {
        private static readonly Encoding cyrillic = Encoding.GetEncoding(1251);

        public static List<List<Message>> Read(string inputFile, Encoding encoding)
        {
            var decompressedFile = Prs.Decompress(File.ReadAllBytes(inputFile));
            string fileName = Path.GetFileNameWithoutExtension(inputFile);

            var pointers = ReadOffsets(decompressedFile);
            var messages = new List<List<Message>>();

            if (fileName.StartsWith("eh"))
                messages = ReadEmeraldHints(decompressedFile, pointers, encoding);
            else if (fileName.StartsWith("mh"))
                messages = ReadMessages(decompressedFile, pointers, encoding);
            else if (fileName.StartsWith("MsgAlKinderFoName"))
                messages = ReadChaoNames(decompressedFile, pointers);
            else
                messages = ReadSimpleText(decompressedFile, pointers, encoding);

            return messages;
        } 

        public static void Write(string outputFile, List<List<Message>> jsonContents, Encoding encoding)
        {
            string fileName = Path.GetFileNameWithoutExtension(outputFile);
            var strings = new List<string>();

            if (fileName.StartsWith("eh"))
                strings = GetEmeraldHintStrings(jsonContents);
            else if (fileName.StartsWith("mh"))
                strings = GetCombinedMessageStrings(jsonContents);            
            else
                strings = GetSimpleStrings(jsonContents);

            bool isChaoNames = fileName.StartsWith("MsgAlKinderFoName");

            var cText = GetCStringsAndPointers(strings, encoding, isChaoNames);
            var contents = GetFileContents(cText);
            File.WriteAllBytes(outputFile, Prs.Compress(contents, 0x1FFF));
        }


        // Reading PRS file

        private static List<int> ReadOffsets(byte[] decompressedFile)
        {
            var reader = new BinaryReader(new MemoryStream(decompressedFile));
            var offsets = new List<int>();

            while (true)
            {
                int offset = reader.ReadInt32(Endianness.BigEndian);
                if (offset == -1 || offset > reader.BaseStream.Length) break;

                offsets.Add(offset);
            }

            reader.Dispose();
            return offsets;
        }

        private static List<List<Message>> ReadMessages(byte[] decompressedFile, List<int> pointers, Encoding encoding)
        {
            var reader = new BinaryReader(new MemoryStream(decompressedFile));
            var messagesList = new List<List<Message>>();

            foreach (var pointer in pointers)
            {
                reader.BaseStream.Position = pointer;
                string contents = reader.ReadCString(encoding).ReplaceKeyboardButtons();

                if (encoding == cyrillic)
                    contents = contents.ConvertToModifiedCodepage();

                string[] lines = contents.Split(new char[] { '\x0C' }, StringSplitOptions.RemoveEmptyEntries);
                var linesList = new List<Message>();

                foreach (var line in lines)
                {
                    string controls = line.Substring(0, line.IndexOf(' '));

                    string? voice = controls.IndexOf('s') != -1 ? controls.Substring(controls.IndexOf('s') + 1, controls.IndexOf('w') - controls.IndexOf('s') - 1) : null;
                    int? voiceID = voice == null ? null : int.Parse(voice);

                    string? framecount = controls.IndexOf('w') != -1 ? controls.Substring(controls.IndexOf('w') + 1) : null;
                    int? duration = framecount == null ? null : int.Parse(framecount);

                    string text = line.Substring(line.IndexOf(' ') + 1);

                    bool centered = text.StartsWith('\a');
                    text = centered ? text.Substring(1) : text;

                    linesList.Add(new Message(voiceID, duration, null, centered, text));
                }

                messagesList.Add(linesList);
            }

            return messagesList;
        }

        private static List<List<Message>> ReadEmeraldHints(byte[] decompressedFile, List<int> pointers, Encoding encoding)
        {
            var reader = new BinaryReader(new MemoryStream(decompressedFile));
            var hintsPerPiece = new List<Message>();
            var messagesList = new List<List<Message>>();

            foreach (var pointer in pointers)
            {
                reader.BaseStream.Position = pointer;
                string contents = reader.ReadCString(encoding).ReplaceKeyboardButtons();

                if (encoding == cyrillic)
                    contents = contents.ConvertToModifiedCodepage();

                string controls = contents.Substring(0, contents.IndexOf(' '));

                bool? is2p = controls.IndexOf('D') != -1 ? true : null;
                string text = contents.Substring(contents.IndexOf(' ') + 1);                

                bool centered = text.StartsWith('\a');
                text = centered ? text.Substring(1) : text;

                hintsPerPiece.Add(new Message(null, null, is2p, centered, text));

                if (hintsPerPiece.Count == 3)
                {
                    messagesList.Add(hintsPerPiece);
                    hintsPerPiece = new List<Message>();
                }               
            }

            return messagesList;
        }

        private static List<List<Message>> ReadSimpleText(byte[] decompressedFile, List<int> pointers, Encoding encoding)
        {
            var reader = new BinaryReader(new MemoryStream(decompressedFile));
            var messagesList = new List<List<Message>>();
            var linesList = new List<Message>();

            foreach (var pointer in pointers)
            {
                reader.BaseStream.Position = pointer;
                string text = reader.ReadCString(encoding).ReplaceKeyboardButtons();

                if (encoding == cyrillic)
                    text = text.ConvertToModifiedCodepage();

                bool? centered = text.StartsWith('\a') == true ? true : null;
                text = centered != null ? text.Substring(1) : text;

                linesList.Add(new Message(null, null, null, centered, text));
            }

            messagesList.Add(linesList);

            return messagesList;
        }

        private static List<List<Message>> ReadChaoNames(byte[] decompressedFile, List<int> pointers)
        {
            var reader = new BinaryReader(new MemoryStream(decompressedFile));
            var messagesList = new List<List<Message>>();
            var linesList = new List<Message>();

            foreach (var pointer in pointers)
            {
                reader.BaseStream.Position = pointer;
                string text = reader.ReadChaoName();

                bool? centered = text.StartsWith('\a') == true ? true : null;
                text = centered == true ? text.Substring(1) : text;

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
                        builder.Append('\a');

                    builder.Append(line.Text);
                }

                string text = builder.ToString().ReplaceKeyboardButtons(TextConversionMode.Reversed);
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
                        builder.Append('\a');

                    builder.Append(hint.Text);

                    string text = builder.ToString().ReplaceKeyboardButtons(TextConversionMode.Reversed);
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
                        builder.Append('\a');

                    builder.Append(line.Text);

                    string text = builder.ToString().ReplaceKeyboardButtons(TextConversionMode.Reversed);
                    strings.Add(text);
                }
            }

            return strings;
        }


        private static List<CStyleText> GetCStringsAndPointers(List<string> strings, Encoding encoding, bool isChaoNames = false)
        {
            var cText = new List<CStyleText>();
            int separatorLength = 4;
            int offset = sizeof(int) * strings.Count + separatorLength;

            foreach (var line in strings)
            {
                var cString = new List<byte>();
                string text = line;

                if (encoding == cyrillic)
                    text = text.ConvertToModifiedCodepage(TextConversionMode.Reversed);

                byte[] textBytes = isChaoNames ? TextConversion.ToBytes(text) : encoding.GetBytes(text);

                cString.AddRange(textBytes);
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
                byte[] offsetBytes = BitConverter.GetBytes(entry.Offset).Reverse().ToArray();
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