using CsvHelper;
using System.Globalization;

namespace SA2EmeraldHintsTool
{
    public static class CsvFile
    {
        public static List<CsvEmeraldHintData> Read(string inputFile)
        {
            var reader = new StreamReader(inputFile);
            var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<CsvEmeraldHintData>().ToList();
            return records;
        }

        public static void Write(string outputFile, List<CsvEmeraldHintData> fileContents)
        {
            var writer = new StreamWriter(outputFile);
            var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteRecords(fileContents);
            writer.Flush();
        }
    }
}
