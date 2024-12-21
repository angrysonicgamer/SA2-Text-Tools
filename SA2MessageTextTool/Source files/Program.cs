﻿using System.Text;

namespace SA2MessageTextTool
{
    public static class Program
    {
        private static void SetAppTitle()
        {
            Console.Title = "SA2 Message Text Tool";
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
            AppConfig config = new();
            config.Read();

            // Main actions

            if (fileExtension == ".prs")
            {
                var extractedData = PrsFile.Read(sourceFile, config);
                JsonFile.Write(extractedData, config);
            }
            else if (fileExtension == ".json")
            {
                var extractedData = JsonFile.Read(sourceFile);
                PrsFile.Write(extractedData, config);
            }
            else
            {
                DisplayMessage.WrongExtension();
            }
        }
    }
}