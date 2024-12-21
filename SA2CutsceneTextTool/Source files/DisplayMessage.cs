namespace SA2CutsceneTextTool
{
    public static class DisplayMessage
    {
        public static void AboutTool()
        {
            string about = "This is a tool to work with Sonic Adventure 2 cutscene text files\n\n" +
                "- Usage -\n" +
                "Drag and drop a compressed PRS file to export the data into JSON or CSV file (depending on the config).\n" +
                "Drag and drop an edited JSON or CSV file exported via this tool to create a new PRS file with new data.\n" +
                "It will be saved in the \"New files\" subfolder.\n\n" +
                "- CMD usage -\n" +
                "SA2CutsceneTextTool filename\n\n" +
                "- Editing extracted data -\n" +
                "Extracted data is ordered by event ID. Each event is represented by a group of messages.\n" +
                "\"Text\" parameter is the main data you might want to edit.\n" +
                "\"Centered\" parameter may be null (ignored) or have one of the following values: Block, EachLine.\n" +
                "\"Character\" parameter doesn't really matter, just leave it alone.\n" +
                "You can add or remove messages but in this case you would need to edit subtitle timings as well.\n\n" +
                "Use this tool only for cutscene files (evmesXY), where X - H/D/L (hero/dark/last story), Y - language (digits 0-5).\n" +
                "The tool supports Windows-1251, Windows-1252 and Shift-JIS encodings and both little endian and big endian byte orders.\n" +
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
            Console.WriteLine($"File {file} has been successfully saved in the \"New files\" subfolder!\nYou can use it in your mod!\n");
            Wait();
        }
        public static void ReadingFile(string file)
        {
            Console.WriteLine($"Reading file: {Path.GetFileName(file)}...\n");
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
