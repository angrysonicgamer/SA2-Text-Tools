using csharp_prs;
using SA2CutsceneTextTool.Common;
using SA2CutsceneTextTool.Extensions;

namespace SA2CutsceneTextTool.PRS
{
    public static partial class PrsFile
    {
        public static void Write(EventFile data, AppConfig config)
        {
            string fileName = data.Name;
            var header = GenerateEventInfo(data.Events);
            var messageData = GenerateMessageData(data.Events, config);
            var binary = WriteDecompressedBinary(header, messageData, config);

            string destinationFolder = "New files";
            string prsFile = $"{destinationFolder}\\{fileName}.prs";
            Directory.CreateDirectory(destinationFolder);
            File.WriteAllBytes(prsFile, Prs.Compress(binary, 0x1FFF));
            DisplayMessage.Config(config);
            DisplayMessage.FileSaved($"{fileName}.prs");
        }


        private static List<EventInfo> GenerateEventInfo(List<Scene> eventData)
        {
            uint separatorLength = 12;
            uint messagePointer = EventInfo.Size * (uint)eventData.Count + separatorLength + Pointer.BaseAddress;
            var eventInfoList = new List<EventInfo>();

            foreach (var scene in eventData)
            {
                int messagesCount = scene.Messages.Count;

                if (messagesCount == 1 && scene.Messages[0].Text == "")
                    messagesCount = 0;

                eventInfoList.Add(new EventInfo(scene.EventID, messagePointer, messagesCount));
                messagePointer += (uint)scene.Messages.Count * PrsMessage.Size;
            }

            return eventInfoList;
        }

        private static string GetRawString(Message message, AppConfig config)
        {
            string rawText = config.UseModifiedCyrillicCP ? message.Text.ModifyCyrillicCP(TextConversionMode.Reversed) : message.Text;
            rawText = message.Centered.HasValue ? $"{(char)message.Centered}{rawText}" : rawText;

            return rawText;
        }

        private static List<PrsMessage> GenerateMessageData(List<Scene> eventData, AppConfig config)
        {
            int totalMessagesCount = 0;

            foreach (var scene in eventData)
            {
                totalMessagesCount += scene.Messages.Count;
            }

            uint separatorLength = 12;
            uint textPointer = EventInfo.Size * (uint)eventData.Count + separatorLength + PrsMessage.Size * (uint)totalMessagesCount + Pointer.BaseAddress;
            var messageData = new List<PrsMessage>();

            foreach (var scene in eventData)
            {
                foreach (var message in scene.Messages)
                {
                    string rawText = GetRawString(message, config);
                    messageData.Add(new PrsMessage(message.Character, textPointer, rawText));
                    textPointer += (uint)config.Encoding.GetByteCount(rawText) + 1;
                }
            }

            return messageData;
        }

        private static byte[] WriteDecompressedBinary(List<EventInfo> eventInfoList, List<PrsMessage> messageData, AppConfig config)
        {
            var memory = new MemoryStream();
            var writer = new BinaryWriter(memory);

            foreach (var scene in eventInfoList)
            {
                scene.Write(writer, config.Endianness);
            }

            EventInfo.Null.Write(writer, config.Endianness);

            foreach (var message in messageData)
            {
                message.WriteData(writer, config);
            }

            foreach (var message in messageData)
            {
                message.WriteText(writer, config);
            }

            writer.Dispose();
            return memory.GetBuffer();
        }
    }
}
