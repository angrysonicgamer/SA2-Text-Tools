using CsvHelper;
using System.Globalization;
using SA2CutsceneTextTool.Common;

namespace SA2CutsceneTextTool.CSV
{
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

            foreach (var scene in eventData)
            {
                var messageData = new List<CsvMessage>();

                foreach (var message in scene.Messages)
                {
                    var csvMessage = new CsvMessage();
                    csvMessage.GetCsvMessage(message, scene);
                    messageData.Add(csvMessage);
                }

                csvData.Add(messageData);
            }

            return csvData;
        }
    }
}
