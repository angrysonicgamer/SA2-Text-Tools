using System.Text;

namespace SA2CutsceneTextTool
{
    public static class Program
    {
        private static void SetAppTitle()
        {
            Console.Title = "SA2 Cutscene Text Tool";
        }


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
            var config = new AppConfig();
            config.Read();
            Pointer.SetBaseAddress(config);

            if (fileExtension == ".prs")
            {
                var extractedData = PrsFile.Read(sourceFile, config);

                if (config.Export == ExportType.JSON)
                {
                    JsonFile.Write(extractedData, config);
                }
                else // if CSV
                {
                    CsvFile.Write(extractedData, config);
                }
            }
            else if (fileExtension == ".json" || fileExtension == ".csv")
            {
                EventFile extractedData;

                if (fileExtension == ".csv")
                {
                    extractedData = CsvFile.Read(sourceFile);
                }
                else // if .json
                {
                    extractedData = JsonFile.Read(sourceFile);
                }

                PrsFile.Write(extractedData, config);
            }
            else
            {
                DisplayMessage.WrongExtension();
            }
        }
    }
}