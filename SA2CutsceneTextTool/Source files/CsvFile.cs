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
            var cutsceneTextData = new Dictionary<int, List<Message>>();

            foreach (var record in records)
            {
                if (int.TryParse(record.EventID, out int eventID))
                {
                    string text = record.Centered == "+" ? $"\a{record.Text}" : record.Text;

                    if (!cutsceneTextData.ContainsKey(eventID))
                        cutsceneTextData.Add(eventID, new List<Message>());

                    cutsceneTextData[eventID].Add(new Message(int.Parse(record.Character), text));
                }
            }

            return cutsceneTextData;
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
