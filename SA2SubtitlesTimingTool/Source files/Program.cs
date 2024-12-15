using System.Text;

namespace SA2SubtitlesTimingTool
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
            string fileName = Path.GetFileNameWithoutExtension(sourceFile);
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
            Encoding encoding = SetEncoding(fileName);

            if (fileExtension == ".prs" || fileExtension == ".scr")
            {
                var eventData = new List<CsvSubtitleInfo>();

                try
                {
                    eventData = GameFile.Read(sourceFile, encoding, config);
                    DisplayMessage.AssumedEncoding(encoding);
                }
                catch (Exception ex)
                {
                    DisplayMessage.Exception(ex);
                    return;
                }

                string outputFile = $"{fileName}.csv";
                CsvFile.Write(outputFile, eventData, config);
            }
            else if (fileExtension == ".csv")
            {
                var fileContents = CsvFile.Read(sourceFile);
                string extension = Path.GetFileNameWithoutExtension(sourceFile).ToLower().StartsWith("e0") ? ".prs" : ".scr";
                string outputFile = $"{fileName}{extension}";
                GameFile.Write(outputFile, fileContents, config);                
            }
            else
            {
                DisplayMessage.WrongExtension();
            }
        }


        private static void SetAppTitle()
        {
            Console.Title = "SA2 Subtitles Timing Tool";
        }

        private static Encoding SetEncoding(string fileName)
        {
            char language = fileName.Last();
            int codepage;

            switch (language)
            {
                case 'j':
                    codepage = 932;     // Japanese - Shift-JIS
                    break;
                case '1':
                    codepage = 1251;    // English - CP1251, meant to use for Russian as well
                    break;
                default:
                    codepage = 1252;    // other European languages - CP1252
                    break;
            }

            return Encoding.GetEncoding(codepage);
        }
    }
}