using System.Text;

namespace SA2SubtitlesTimingTool
{
    public static class Program
    {
        private static readonly string title = "SA2 Subtitles Timing Tool";
        private static readonly string usage = "Please provide a compressed PRS file to export the data into CSV table and edit it however you want.\n" +
            "By providing a CSV table exported via this tool you can make a new PRS file with new data.\n\n" +
            "Use it only for subtitle timing files (e0XXX_Y.prs/csv),\n" +
            "where XXX - event ID, Y - language (letter 'j' for Japanese, digits 1-5 for other languages).\n" +
            "Note that you also need event text files (evmesXY.prs) in the same folder.\n\n" +
            "The tool uses CP1251 encoding for English, CP1252 for other European languages and Shift-JIS for Japanese (checks the last char in the file name).\n";
        private static readonly string emptyArgs = "You haven't provided a file to read.\n\n";
        private static readonly string tooManyArgs = "Too many arguments.\nPlease provide a file name as the only argument.\n\n";
        private static readonly string noFile = "File not found.\n";
        private static readonly string wrongName = "Wrong file name.\nSupported file names: e0XXX_Y.prs/csv, where XXX - event ID, Y - language (letter 'j' for Japanese, digits 1-5 for other languages).\n";
        private static readonly string wrongExtension = "Wrong extension.\nPlease provide a compressed PRS file or a CSV table exported via this tool before.\n";


        public static void Main(string[] args)
        {
            SetAppTitle();
            if (CheckCondition(args.Length == 0, emptyArgs + usage)) return;
            if (CheckCondition(args.Length > 1, tooManyArgs + usage)) return;
            if (CheckCondition(!File.Exists(args[0]), noFile)) return;

            string fileName = Path.GetFileNameWithoutExtension(args[0]).ToLower();
            if (CheckCondition(!(fileName.StartsWith("e0") || fileName.StartsWith("me0")), wrongName)) return;

            string fileExtension = Path.GetExtension(args[0]).ToLower();
            if (CheckCondition(fileExtension != ".prs" && fileExtension != ".scr" && fileExtension != ".csv", wrongExtension)) return;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding encoding = SetEncoding(args[0]);

            if (fileExtension == ".prs" || fileExtension == ".scr")
            {
                int eventID = int.Parse(fileName.Substring(fileName.IndexOf('0'), 4));
                string eventTextFile = GetEventTextFileName(args[0], eventID);
                var eventData = GameFile.Read(args[0], eventTextFile, eventID, encoding);
                string outputFile = GetOutputFileName(args[0], ".csv");
                CsvFile.Write(outputFile, eventData);
                Console.WriteLine($"CSV file \"{Path.GetFileName(outputFile)}\" successfully created!");
            }
            else // if .csv
            {
                var fileContents = CsvFile.Read(args[0]);
                string extension = Path.GetFileNameWithoutExtension(args[0]).ToLower().StartsWith("e0") ? ".prs" : ".scr";
                string outputFile = GetOutputFileName(args[0], extension);
                GameFile.Write(outputFile, fileContents);
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
                case 'j':
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

        private static string GetEventTextFileName(string timingFile, int eventID)
        {
            char language = Path.GetFileNameWithoutExtension(timingFile).Last();
            if (language == 'j')
                language = '0';
                        
            string eventTextFile = "";

            if (eventID < 100)
            {
                eventTextFile = $"evmesH{language}.prs";
            }
            else if (eventID >= 100 && eventID < 200)
            {
                eventTextFile = $"evmesD{language}.prs";
            }
            else if (eventID >= 200)
            {
                eventTextFile = $"evmesL{language}.prs";
            }

            return eventTextFile;
        }
    }
}