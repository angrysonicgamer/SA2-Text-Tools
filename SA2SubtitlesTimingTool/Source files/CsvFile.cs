using CsvHelper;
using System.Globalization;

namespace SA2SubtitlesTimingTool
{
    public static class CsvFile
    {
        public static List<CsvSubtitleInfo> Read(string inputFile)
        {
            var reader = new StreamReader(inputFile);
            var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            return csv.GetRecords<CsvSubtitleInfo>().ToList();
        }

        public static void Write(string outputFile, List<CsvSubtitleInfo> csvData)
        {
            var writer = new StreamWriter(outputFile);
            var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteRecords(csvData);
            writer.Flush();
        }
    }
}
