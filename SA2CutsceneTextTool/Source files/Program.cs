using System.Text;

namespace SA2CutsceneTextTool
{
    public static class Program
    {
        private static readonly string title = "SA2 Cutscene Text Tool";
        private static readonly string usage = "Please provide a compressed PRS file to export the data into CSV table and edit it however you want.\n" +
            "By providing a CSV table exported via this tool you can make a new PRS file with new data.\n\n" +
            "Use it only for cutscene files (evmesXY.prs/csv), where X - H/D/L (hero/dark/last story), Y - language (digits 0-5).\n\n" +
            "The tool uses CP1251 encoding for English, CP1252 for other European languages and Shift-JIS for Japanese (checks the last digit in the file name).\n";
        private static readonly string emptyArgs = "You haven't provided a file to read.\n\n";
        private static readonly string tooManyArgs = "Too many arguments.\nPlease provide a file name as the only argument.\n\n";
        private static readonly string noFile = "File not found.\n";
        private static readonly string wrongName = "Wrong file name.\nSupported file names: evmesXY.prs/csv, where X - H/D/L (hero/dark/last story), Y - language (digits 0-5).\n";
        private static readonly string wrongExtension = "Wrong extension.\nPlease provide a compressed PRS file or a CSV table exported via this tool before.\n";


        static void Main(string[] args)
        {
            SetAppTitle();
            if (CheckCondition(args.Length == 0, emptyArgs + usage)) return;
            if (CheckCondition(args.Length > 1, tooManyArgs + usage)) return;
            if (CheckCondition(!File.Exists(args[0]), noFile)) return;
            if (CheckCondition(!Path.GetFileNameWithoutExtension(args[0]).ToLower().StartsWith("evmes"), wrongName)) return;
            
            string fileExtension = Path.GetExtension(args[0]);
            if (CheckCondition(fileExtension != ".prs" && fileExtension != ".csv", wrongExtension)) return;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding encoding = SetEncoding(args[0]);

            if (fileExtension == ".prs")
            {
                var eventData = PrsFile.Read(args[0], encoding);
                string outputFile = GetOutputFileName(args[0], ".csv");
                CsvFile.Write(outputFile, eventData);
                Console.WriteLine($"CSV file \"{Path.GetFileName(outputFile)}\" successfully created!");
            }
            else // if .csv
            {
                var fileContents = CsvFile.Read(args[0]);
                string outputFile = GetOutputFileName(args[0], ".prs");
                PrsFile.Write(outputFile, fileContents, encoding);
                Console.WriteLine($"PRS file \"{Path.GetFileName(outputFile)}\" successfully created! You can use it in your mod!");
            }            
        }


        private static void SetAppTitle()
        {
            Console.Title = title;
        }

        private static bool CheckCondition(bool condition, string message)
        {
            if (condition)
            {
                Console.WriteLine(message);
                return true;
            }

            return false;
        }

        private static string GetOutputFileName(string inputFile, string extension)
        {
            return inputFile.Substring(0, inputFile.Length - 4) + extension;
        }

        private static Encoding SetEncoding(string fileName)
        {
            char language = Path.GetFileNameWithoutExtension(fileName).Last();
            int codepage;

            switch (language)
            {
                case '0':
                    codepage = 932; // Japanese - Shift-JIS
                    break;
                case '1':
                    codepage = 1251; // English - CP1251, meant to use for Russian as well
                    break;
                default:
                    codepage = 1252; // other European languages - CP1252
                    break;
            }

            return Encoding.GetEncoding(codepage);
        }
    }
}