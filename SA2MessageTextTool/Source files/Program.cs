using System.Text;

namespace SA2MsgFileTextTool
{
    public static class Program
    {
        private static readonly string title = "SA2 Message File Text Tool";
        private static readonly string usage = "Please provide a compressed PRS file to export the data into JSON file and edit it however you want.\n" +
            "By providing a JSON file exported via this tool you can make a new PRS file with new data.\n\n" +
            "You can use for any SA2 message file, except for the event ones.\n\n" +
            "The tool uses CP1251 encoding for English, CP1252 for other European languages and Shift-JIS for Japanese (checks the last letter in the file name).\n";
        private static readonly string emptyArgs = "You haven't provided a file to read.\n\n";            
        private static readonly string tooManyArgs = "Too many arguments.\nPlease provide a file name as the only argument.\n\n";
        private static readonly string noFile = "File not found.\n";
        private static readonly string wrongExtension = "Wrong extension.\nPlease provide a compressed PRS file or a JSON file via this tool before.\n";
        

        public static void Main(string[] args)
        {
            SetAppTitle();
            if (CheckCondition(args.Length == 0, emptyArgs + usage)) return;
            if (CheckCondition(args.Length > 1, tooManyArgs + usage)) return;
            if (CheckCondition(!File.Exists(args[0]), noFile)) return;

            string fileExtension = Path.GetExtension(args[0]).ToLower();
            if (CheckCondition(fileExtension != ".prs" && fileExtension != ".json", wrongExtension)) return;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding encoding = SetEncoding(args[0]);

            if (fileExtension == ".prs")
            {
                var fileContents = PrsFile.Read(args[0], encoding);
                string outputFile = GetOutputFileName(args[0], ".json");
                JsonFile.Write(outputFile, fileContents);
                Console.WriteLine($"JSON file \"{Path.GetFileName(outputFile)}\" successfully created!");
            }
            else // if .json
            {
                var fileContents = JsonFile.Read(args[0]);
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
            return Path.GetFileNameWithoutExtension(inputFile) + extension;
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
                case 'e':
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