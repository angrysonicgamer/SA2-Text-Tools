using CsvHelper;
using System.Globalization;

namespace SA2MsgFileTextTool
{
    public static class CsvFile
    {
        public static List<List<CsvMessageData>> Read(string inputFile)
        {
            var reader = new StreamReader(inputFile);
            var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<CsvMessageData>().ToList();
            int linesCount = int.Parse(records.Last().Num) + 1;
            var groupedRecords = new List<List<CsvMessageData>>();

            for (int i = 0; i < linesCount; i++)
            {
                groupedRecords.Add(new List<CsvMessageData>());
            }

            foreach (var record in records)
            {
                if (int.TryParse(record.Num, out int num))
                {
                    groupedRecords[num].Add(record);
                }
            }

            return groupedRecords;
        }

        public static void Write(string outputFile, List<List<CsvMessageData>> fileContents)
        {
            var writer = new StreamWriter(outputFile);
            var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteHeader<CsvMessageData>();
            csv.NextRecord();
            csv.NextRecord();
            foreach (var line in fileContents)
            {
                csv.WriteRecords(line);
                csv.NextRecord();
            }

            writer.Flush();            
        }
    }
}
