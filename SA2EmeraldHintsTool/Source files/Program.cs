using System.Text;

namespace SA2EmeraldHintsTool
{
    public static class Program
    {
        private static readonly string title = "SA2 Emerald Hints Thing";
        private static readonly string usage =
            "---ABOUT---\n" +
            "This little tool is meant to work with Sonic Adventure 2 emerald hints.\n" +
            "You can easily extract them into a csv file, edit and convert them back.\n" +
            "Might be useful for (re)translation mods. 2P treasure hunting will also work as well.\n" +
            "Don't use the tool with other .prs files like Omochao hints (mh00**e.prs), etc.\n" +
            "---USAGE---\n" +
            "Drag and drop an ORIGINAL PC version eh00**e.prs file on the exe file (or type sa2emeraldhints eh00**e.prs), a csv file of the same name will be created.\n" +
            "Drag and drop the csv file on the exe file (or type sa2emeraldhints eh00**e.csv), a new file named eh00**e_new.prs will be created.\n" +
            "That file is ready to use in your mod, don't forget to remove the \"_new\" part.\n\n" +
            "This little thing is written by Irregular Zero.";
        private static readonly string emptyArgs = "You haven't provided a file to read.\n\n";
        private static readonly string tooManyArgs = "Too many arguments.\nPlease provide a file name as the only argument.\n\n";
        private static readonly string noFile = "File not found.\n";
        private static readonly string wrongExtension =
            "You have attempted to open a file with not supported extension.\n" +
            "Please use an emerald hints .prs file or a .csv file previously created by this tool.\n";

        public static void Main(string[] args)
        {
            SetAppTitle();
            if (CheckCondition(args.Length == 0, emptyArgs + usage)) return;
            if (CheckCondition(args.Length > 1, tooManyArgs + usage)) return;
            if (CheckCondition(!File.Exists(args[0]), noFile)) return;

            string fileExtension = GetFileExtension(args[0]);
            if (CheckCondition(fileExtension != ".prs" && fileExtension != ".csv", wrongExtension)) return;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding encoding = SetEncoding(args[0]);

            if (fileExtension == ".prs")
            {
                var fileContents = PrsFile.Read(args[0], encoding);
                string outputFile = GetOutputFileName(args[0], ".csv");
                CsvFile.Write(outputFile, fileContents);
                Console.WriteLine($"CSV file \"{Path.GetFileName(outputFile)}\" successfully created!");
            }
            else // if .csv
            {
                var fileContents = CsvFile.Read(args[0]);
                string outputFile = GetOutputFileName(args[0], ".prs");
                PrsFile.Write(outputFile, fileContents, encoding);
                Console.WriteLine($"PRS file \"{Path.GetFileName(outputFile)}\" successfully created!");
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

        private static string GetFileExtension(string fileName)
        {
            return new FileInfo(fileName).Extension;
        }

        private static string GetOutputFileName(string inputFile, string extension)
        {
            return inputFile.Substring(0, inputFile.Length - 4) + extension;
        }

        private static Encoding SetEncoding(string fileName)
        {
            return Path.GetFileNameWithoutExtension(fileName).Last() == 'j' ? Encoding.GetEncoding(932) : Encoding.GetEncoding(1251);
        }


        /// <summary>
        /// For testing purposes
        /// </summary>
        private static void DisplayFileContents(List<CsvEmeraldHintData> contents, Encoding encoding)
        {
            Console.OutputEncoding = encoding;
            
            foreach (var item in contents)
            {
                Console.WriteLine($"{item.Flag2P} --- {item.Text}");
            }

            Console.ReadKey();
        }
    }
}