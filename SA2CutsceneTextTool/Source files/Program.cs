using System.Text;

namespace SA2CutsceneTextTool
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            SetAppTitle();

            if (args.Length == 0)
            {
                DisplayMessage.AboutTool();
                return;
            }

            if (args.Length > 1)
            {
                DisplayMessage.TooManyArguments();
                DisplayMessage.AboutTool();
                return;
            }

            string sourceFile = args[0];
            string fileExtension = Path.GetExtension(sourceFile).ToLower();

            if (!File.Exists(sourceFile))
            {
                DisplayMessage.FileNotFound(sourceFile);
                return;
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            AppConfig config = new();
            config.Read();
            Pointer.SetBaseAddress(config);

            if (fileExtension == ".prs")
            {
                var eventData = PrsFile.Read(sourceFile, config);
                
                string outputJson = GetOutputFileName(sourceFile, ".json");
                string outputCsv = GetOutputFileName(sourceFile, ".csv");

                if (config.Export == Export.JSON)
                {
                    JsonFile.Write(outputJson, eventData, config);
                }
                else // if CSV
                {
                    var csvEventData = CsvFile.GetCsvData(eventData);
                    CsvFile.Write(outputCsv, csvEventData, config);
                }
            }
            else if (fileExtension == ".json" || fileExtension == ".csv")
            {
                var fileContents = new List<Scene>();

                if (fileExtension == ".csv")
                {
                    fileContents = CsvFile.Read(args[0]);                    
                }
                else // if .json
                {
                    fileContents = JsonFile.Read(args[0]);
                }

                string outputFile = GetOutputFileName(args[0], ".prs");
                PrsFile.Write(outputFile, fileContents, config);
            }
            else
            {
                DisplayMessage.WrongExtension();
            }
        }


        private static void SetAppTitle()
        {
            Console.Title = "SA2 Cutscene Text Tool";
        }

        private static string GetOutputFileName(string inputFile, string extension)
        {
            return Path.GetFileNameWithoutExtension(inputFile) + extension;
        }
    }
}