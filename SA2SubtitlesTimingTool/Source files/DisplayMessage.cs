using System.Text;

namespace SA2SubtitlesTimingTool
{
    public static class DisplayMessage
    {
        public static void AboutTool()
        {
            string about = "This is a tool to work with Sonic Adventure 2 event subtitle timings.\n" +
                "An alternative to Split Tools but visualizes which line has which timing in a CSV file.\n" +
                "It doesn't modify other data in those files, only subtitle timings.\n\n" +
                "- Usage -\nDrag and drop a timing file to export the data into CSV file and edit it however you want.\n" +
                "Drag and drop a CSV file exported via this tool to create a new timing file with new data.\n\n" +
                "- CMD usage -\nSA2SubtitlesTimingTool filename\n\n" +
                "Timing file formats: main events - e0XXX_Y.prs, mini events: me0XXX_Y.scr\n" +
                "where XXX - event ID, Y - language (letter 'j' for Japanese, digits 1-5 for other languages).\n\n" +
                "- Important note -\nThe tool would read a corresponding event text file (evmesXY: X - story, Y - language).\n" +
                "It must be in the same folder and must have the same endianness\n\n" +
                "The tool assumes Windows-1251 encoding for English (may be used for Russian),\n" +
                "Windows-1252 for other European languages and Shift-JIS for Japanese (checks the last char in the file name).\n" +
                "It also supports both little endian and big endian byte orders.\n" +
                "Edit AppConfig.json to set the endianness.\n";
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

        public static void CsvCreated(string file)
        {
            Console.WriteLine($"Subtitle timing data has been extracted to file {file}!\n");
            Wait();
        }

        public static void FileOverwritten(string file)
        {
            Console.WriteLine($"File {file} has been successfully overwritten with new data!\nYou can use it in your mod!\n");
            Wait();
        }

        public static void Config(AppConfig config)
        {
            Console.WriteLine($"Config settings:\nEndianness - {config.Endianness}\n");
        }

        public static void AssumedEncoding(Encoding encoding)
        {
            Console.WriteLine($"Assumed encoding according to file name - {encoding.EncodingName}\n");
        }

        public static void Exception(Exception ex)
        {
            Console.WriteLine($"Caught exception: {ex.Message}\n");
            Wait();
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
