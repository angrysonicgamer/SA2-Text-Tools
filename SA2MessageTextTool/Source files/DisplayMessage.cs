namespace SA2MessageTextTool
{
    public static class DisplayMessage
    {
        public static void AboutTool()
        {
            string about = "This is a tool to work with Sonic Adventure 2 message text files (Omochao hints, emerald hints and others)\n\n" +
                "- Usage -\n" +
                "Drag and drop a compressed PRS file to export the data into JSON file.\n" +
                "Drag and drop a JSON file exported via this tool to create a new PRS file with new data.\n\n" +
                "- CMD usage -\n" +
                "SA2MessageTextTool filename\n\n" +
                "- Message file types -\n" +
                "Determined by what the file name starts with (case insensitive).\n" +
                "\"eh\" - emerald hints for treasure hunting stages, grouped by 3 messages representing 3 hints per piece.\n" +
                "\"mh\" - gameplay text (e.g. Omochao hints), each group of messages represents a whole line (each message - text block).\n" +
                "\"MsgAlKinderFoName\" - Chao names, represented by array of strings.\n" +
                "Anything else - treated as simple text files, represented by array of strings (may have \"Centered\" parameter).\n\n" +
                "- Editing extracted data -\n" +
                "You can add or remove messages (text blocks) for gameplay text. You can also add or remove Chao names.\n" +
                "\"Text\" parameter is the main data you might want to edit.\n" +
                "\"Centered\" parameter may be null (ignored) or have one of the following values: Block, EachLine.\n" +
                "\"Voice\" parameter usually has value (voice ID) in the first message in a group, don't edit if not sure.\n" +
                "\"Duration\" parameter is the number of frames the message will be displayed (60 frames = 1 second).\n" +
                "\"2P Piece\" parameter is only for emerald hints, necessary for 2P mode RNG, leave it alone.\n\n" +                
                "You can use this tool for any SA2 message file, except for the event ones.\n" +
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

        public static void JsonCreated(string file)
        {
            Console.WriteLine($"Text has been extracted to {file}!\n");
            Wait();
        }

        public static void FileSaved(string file)
        {
            Console.WriteLine($"File {file} has been successfully saved in the \"New files\" directory!\nYou can use it in your mod!\n");
            Wait();
        }

        public static void ReadingFile(string file)
        {
            Console.WriteLine($"Reading file: {Path.GetFileName(file)}...\n");
        }

        public static void Config(AppConfig config)
        {
            string modifiedCodepage = config.ModifiedCodepage == true ? "(modified)" : "";
            
            Console.WriteLine($"Config settings:\n" +
                $"Endianness - {config.Endianness}\n" +
                $"Encoding - {config.Encoding.EncodingName} {modifiedCodepage}\n" +
                $"JSON Style - {config.JsonStyle}\n");
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
