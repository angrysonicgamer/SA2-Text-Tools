using csharp_prs;
using SA2CutsceneTextTool.Common;
using SA2CutsceneTextTool.Extensions;

namespace SA2CutsceneTextTool.PRS
{
    public static partial class PrsFile
    {
        public static EventFile Read(string prsFile, AppConfig config)
        {
            DisplayMessage.ReadingFile(prsFile);
            var decompressedFile = Prs.Decompress(File.ReadAllBytes(prsFile));
            string fileName = Path.GetFileNameWithoutExtension(prsFile);
            var events = ReadEventData(decompressedFile, config);

            if (config.OrderByID)
            {
                events = events.OrderBy(x => x.EventID).ToList();
            }                

            return new EventFile(fileName, events);
        }


        private static List<EventInfo> ReadEventInfoList(BinaryReader reader, Endianness endianness)
        {
            var eventInfoList = new List<EventInfo>();

            while (true)
            {
                var eventInfo = new EventInfo();
                eventInfo.Read(reader, endianness);

                if (eventInfo.IsValid())
                {
                    eventInfoList.Add(eventInfo);
                }                    
                else break;
            }

            return eventInfoList;
        }

        private static List<Scene> ReadEventData(byte[] decompressedFile, AppConfig config)
        {
            var reader = new BinaryReader(new MemoryStream(decompressedFile));
            var sceneInfoList = ReadEventInfoList(reader, config.Endianness);
            var eventData = new List<Scene>();

            foreach (var scene in sceneInfoList)
            {
                var messages = new List<Message>();
                reader.SetPosition(scene.MessagePointer - Pointer.BaseAddress);

                if (scene.TotalMessages == 0)
                {
                    int character = reader.ReadInt32(config.Endianness);
                    messages.Add(new Message(character, null, ""));
                }

                for (int i = 0; i < scene.TotalMessages; i++)
                {                    
                    var message = new Message();
                    message.Read(reader, config);
                    messages.Add(message);
                }

                eventData.Add(new Scene(scene.EventID, messages));
            }

            reader.Dispose();
            return eventData;
        }
    }
}
