using CsvHelper;
using System.Globalization;

namespace SA2SimpleTextTool
{
    public static class CsvFile
    {
        public static List<string> Read(string inputFile)
        {
            var reader = new StreamReader(inputFile);
            var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<CsvData>().ToList();

            var strings = new List<string>();

            foreach (var record in records)
            {
                string text = record.Centered == "+" ? $"\a{record.Text}" : record.Text;
                strings.Add(text);
            }

            return strings;
        }

        public static void Write(string outputFile, List<CsvData> fileContents)
        {
            var writer = new StreamWriter(outputFile);
            var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            
            csv.WriteRecords(fileContents);
            writer.Flush();
        }
    }    
}
