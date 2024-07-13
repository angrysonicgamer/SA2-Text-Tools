using CsvHelper;
using System.Globalization;

namespace SA2CutsceneTextTool
{
    public static class CsvFile
    {
        public static Dictionary<int, List<Message>> Read(string inputFile)
        {
            var reader = new StreamReader(inputFile);
            var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<CsvEventData>().ToList();
            var eventData = new Dictionary<int, List<Message>>();

            foreach (var record in records)
            {
                if (int.TryParse(record.EventID, out int eventID))
                {
                    string text = record.Centered == "+" ? $"\a{record.Text}" : record.Text;

                    if (!eventData.ContainsKey(eventID))
                        eventData.Add(eventID, new List<Message>());

                    eventData[eventID].Add(new Message(int.Parse(record.Character), text));
                }
            }

            return eventData;
        }

        public static void Write(string outputFile, List<List<CsvEventData>> csvData)
        {
            var writer = new StreamWriter(outputFile);
            var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteHeader<CsvEventData>();
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
