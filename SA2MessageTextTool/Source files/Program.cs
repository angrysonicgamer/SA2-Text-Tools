using System.Text;

namespace SA2MessageTextTool
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

            // Main actions

            if (fileExtension == ".prs")
            {
                var fileContents = PrsFile.Read(sourceFile, config);
                string outputFile = GetOutputFileName(sourceFile, ".json");
                JsonFile.Write(outputFile, fileContents, config);
                DisplayMessage.Config(config);
                DisplayMessage.JsonCreated(outputFile);
            }
            else if (fileExtension == ".json")
            {
                var fileContents = JsonFile.Read(args[0]);
                string outputFile = GetOutputFileName(args[0], ".prs");
                PrsFile.Write(outputFile, fileContents, config);
                DisplayMessage.Config(config);
                DisplayMessage.FileSaved(outputFile);
            }
            else
            {
                DisplayMessage.WrongExtension();
            }
        }

        private static void SetAppTitle()
        {
            Console.Title = "SA2 Message File Text Tool";
        }

        private static string GetOutputFileName(string inputFile, string extension)
        {
            return $"{Path.GetFileNameWithoutExtension(inputFile)}{extension}";
        }
    }
}