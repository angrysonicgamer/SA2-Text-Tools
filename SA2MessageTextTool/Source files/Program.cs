using System.Text;

namespace SA2MessageTextTool
{
    public static class Program
    {
        private static readonly string title = "SA2 Message File Text Tool";
        private static readonly string usage = "Provide a compressed PRS file to export the data into JSON file and edit it however you want.\n" +
            "Provide a JSON file exported via this tool to create a new PRS file with new data.\n\n" +
            "You can use for any SA2 message file, except for the event ones.\n\n" +
            "The tool supports Windows-1251, Windows-1252 and Shift-JIS encodings and both little endian and big endian byte orders.\n" +
            "Edit AppConfig.json to set the settings you want.\n";
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
            AppConfig config = new();
            config.Read();

            if (fileExtension == ".prs")
            {
                var fileContents = PrsFile.Read(args[0], config);
                string outputFile = GetOutputFileName(args[0], ".json");
                JsonFile.Write(outputFile, fileContents);
                Console.WriteLine($"JSON file \"{Path.GetFileName(outputFile)}\" successfully created!");
            }
            else // if .json
            {
                var fileContents = JsonFile.Read(args[0]);
                string outputFile = GetOutputFileName(args[0], ".prs");
                PrsFile.Write(outputFile, fileContents, config);
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
    }
}