using CsvHelper;
using System.Globalization;

namespace SA2CutsceneTextTool
{
    public static class CsvFile
    {
        public static List<Scene> Read(string inputFile)
        {
            var reader = new StreamReader(inputFile);
            var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<CsvMessage>().ToList();
            var dict = new Dictionary<int, List<Message>>();

            foreach (var record in records)
            {
                if (int.TryParse(record.EventID, out int eventID))
                {
                    Centered? centered = record.Centered != "" ? Enum.Parse<Centered>(record.Centered) : null;

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

            return eventData;
        }


        public static List<List<CsvMessage>> GetCsvData(List<Scene> eventData)
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

        public static void Write(string outputFile, List<List<CsvMessage>> csvData)
        {
            var writer = new StreamWriter(outputFile);
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
        }
    }
}
