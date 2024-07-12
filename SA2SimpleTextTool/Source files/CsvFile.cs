using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace SA2SimpleTextTool
{
    public static class CsvFile
    {
        public static List<string> Read(string inputFile)
        {
            var reader = new StreamReader(inputFile);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false
            };
            var csv = new CsvReader(reader, config);
            var records = csv.GetRecords<CsvData>().ToList();

            var strings = new List<string>();

            foreach (var record in records)
            {
                strings.Add(record.Text);
            }

            return strings;
        }

        public static void Write(string outputFile, List<string> fileContents)
        {
            var writer = new StreamWriter(outputFile);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false
            };
            var csv = new CsvWriter(writer, config);
            var csvData = new List<CsvData>();

            foreach (var line in fileContents)
                csvData.Add(new CsvData(line));

            csv.WriteRecords(csvData);
            writer.Flush();
        }
    }    
}
