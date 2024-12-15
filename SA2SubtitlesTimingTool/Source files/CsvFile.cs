using CsvHelper;
using System.Globalization;

namespace SA2SubtitlesTimingTool
{
    public static class CsvFile
    {
        public static List<CsvSubtitleInfo> Read(string csvFile)
        {
            var reader = new StreamReader(csvFile);
            var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            return csv.GetRecords<CsvSubtitleInfo>().ToList();
        }

        public static void Write(string csvFile, List<CsvSubtitleInfo> csvData, AppConfig config)
        {
            var writer = new StreamWriter(csvFile);
            var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteRecords(csvData);
            writer.Flush();

            DisplayMessage.Config(config);
            DisplayMessage.CsvCreated(csvFile);
        }
    }
}
