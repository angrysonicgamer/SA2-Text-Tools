﻿namespace SA2CutsceneTextTool
{
    public static class DisplayMessage
    {
        public static void AboutTool()
        {
            string about = "This is a tool to work with Sonic Adventure 2 cutscene text files\n\n" +
                "- Usage -\nDrag and drop a compressed PRS file to export the data into JSON file and edit it however you want.\n" +
                "Drag and drop a JSON file exported via this tool to create a new PRS file with new data.\n\n" +
                "- CMD usage -\nSA2CutsceneTextTool filename\n\n" +
                "Use it only for cutscene files (evmesXY), where X - H/D/L (hero/dark/last story), Y - language (digits 0-5).\n\n" +
                "It supports Windows-1251, Windows-1252 and Shift-JIS encodings and both little endian and big endian byte orders.\n" +
                "Edit AppConfig.json to set the settings you want.\n";
            Console.WriteLine(about);
            Wait();
        }
        public static void TooManyArguments()
        {
            Console.WriteLine("Too many arguments.\n");
        }
        public static void FileNotFound(string file)
        {
            Console.WriteLine($"File {file} not found.\n");
            Wait();
        }
        public static void WrongExtension()
        {
            Console.WriteLine("The file extension is not supported.\n");
            Wait();
        }
        public static void TextExtracted(string file)
        {
            Console.WriteLine($"Text has been extracted to {file}!\n");
            Wait();
        }
        public static void FileSaved(string file)
        {
            Console.WriteLine($"File {file} has been successfully saved in the \"New files\" directory!\nYou can use it in your mod!\n");
            Wait();
        }
        public static void Config(AppConfig config)
        {
            string modifiedCodepage = config.ModifiedCodepage == true ? "(modified)" : "";
            string jsonStyle = config.JsonStyle.HasValue ? $"JSON Style - {config.JsonStyle}\n" : "";

            Console.WriteLine($"Config settings:\n" +
                $"Endianness - {config.Endianness}\n" +
                $"Encoding - {config.Encoding.EncodingName} {modifiedCodepage}\n" +
                $"Export - {config.Export}\n" +
                jsonStyle);
        }

        private static void Wait()
        {
            Console.WriteLine("Press Enter to exit");
            while (true)
            {
                var keyPressed = Console.ReadKey(true).Key;
                if (keyPressed == ConsoleKey.Enter) break;
            }
        }
    }
}