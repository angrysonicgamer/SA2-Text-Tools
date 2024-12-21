using CsvHelper;
using System.Globalization;

namespace SA2CutsceneTextTool
{
    public class CsvMessage
    {
        public string EventID { get; set; }
        public string Character { get; set; }
        public string Centered { get; set; }
        public string Text { get; set; }

        public CsvMessage() { }

        public CsvMessage(string id, string character, string centered, string text)
        {
            EventID = id;
            Character = character;
            Centered = centered;
            Text = text;
        }
    }


    public static class CsvFile
    {
        public static EventFile Read(string csvFile)
        {
            DisplayMessage.ReadingFile(csvFile);
            string fileName = Path.GetFileNameWithoutExtension(csvFile);
            var reader = new StreamReader(csvFile);
            var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<CsvMessage>().ToList();
            var dict = new Dictionary<int, List<Message>>();

            foreach (var record in records)
            {
                if (int.TryParse(record.EventID, out int eventID))
                {
                    CenteringMethod? centered = record.Centered != "" ? Enum.Parse<CenteringMethod>(record.Centered) : null;

                    if (!dict.ContainsKey(eventID))
                        dict.Add(eventID, new List<Message>());

                    dict[eventID].Add(new Message(int.Parse(record.Character), centered, record.Text));
                }
            }

            var eventData = new List<Scene>();

            foreach (var entry in dict)
            {
                eventData.Add(new Scene(entry.Key, entry.Value));
            }

            return new EventFile(fileName, eventData);
        }

        public static void Write(EventFile data, AppConfig config)
        {
            string csvFile = $"{data.Name}.csv";
            var csvData = GetCsvData(data.Events);
            
            var writer = new StreamWriter(csvFile);
            var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteHeader<CsvMessage>();
            csv.NextRecord();
            csv.NextRecord();

            foreach (var line in csvData)
            {
                csv.WriteRecords(line);
                csv.NextRecord();
            }

            writer.Flush();
            writer.Dispose();
            DisplayMessage.Config(config);
            DisplayMessage.TextExtracted(csvFile);
        }


        private static List<List<CsvMessage>> GetCsvData(List<Scene> eventData)
        {
            var csvData = new List<List<CsvMessage>>();

            foreach (var scene in eventData.OrderBy(x => x.EventID))
            {
                var messageData = new List<CsvMessage>();

                foreach (var message in scene.Messages)
                {
                    string centered = message.Centered.HasValue ? message.Centered.ToString() : "";
                    messageData.Add(new CsvMessage(scene.EventID.ToString(), message.Character.ToString(), centered, message.Text));
                }

                csvData.Add(messageData);
            }

            return csvData;
        }
    }
}
