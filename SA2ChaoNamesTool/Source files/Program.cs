using System.Text;

namespace SA2ChaoNamesTool
{
    internal class Program
    {
        private static readonly string title = "SA2 Chao Names Tool";
        private static readonly string usage = "Please provide a compressed PRS file to export the data into .txt file and edit it however you want.\n" +
            "By providing a .txt exported via this tool you can make a new PRS file with new data.\n\n" +
            "Use it only with MsgAlKinderFoName_X.prs/csv files, where X - language (j/e).\n\n" +
            "The tool uses custom codepage specific to Chao names\n";            
        private static readonly string emptyArgs = "You haven't provided a file to read.\n\n";
        private static readonly string tooManyArgs = "Too many arguments.\nPlease provide a file name as the only argument.\n\n";
        private static readonly string noFile = "File not found.\n";
        private static readonly string wrongName = "Wrong file name.\nSupported file names: MsgAlKinderFoName_X.prs/csv, where X - language (j/e).\n";
        private static readonly string wrongExtension = "Wrong extension.\nPlease provide a compressed PRS file or a CSV table exported via this tool before.\n";
        private static readonly string longName = "One or more names' length exceeds 7.\nResult file is not created!\nMake sure your Chao names have no more than 7 characters!";


        public static void Main(string[] args)
        {
            SetAppTitle();
            if (CheckCondition(args.Length == 0, emptyArgs + usage)) return;
            if (CheckCondition(args.Length > 1, tooManyArgs + usage)) return;
            if (CheckCondition(!File.Exists(args[0]), noFile)) return;
            if (CheckCondition(!Path.GetFileNameWithoutExtension(args[0]).ToLower().StartsWith("msgalkinderfoname_"), wrongName)) return;

            string fileExtension = GetFileExtension(args[0]);
            if (CheckCondition(fileExtension != ".prs" && fileExtension != ".txt", wrongExtension)) return;

            if (Path.GetFileNameWithoutExtension(args[0]).Last() == 'j')
                TextConversion.CharTable[13] = 'ー';


            if (fileExtension == ".prs")
            {
                var fileContents = PrsFile.Read(args[0]);
                string outputFile = GetOutputFileName(args[0], ".txt");
                TxtFile.Write(outputFile, fileContents);
                Console.WriteLine($"TXT file \"{Path.GetFileName(outputFile)}\" successfully created!");
            }
            else // if .txt
            {
                var fileContents = TxtFile.Read(args[0]);
                if (CheckCondition(SomeNameLengthExceedsSeven(fileContents), longName)) return;

                string outputFile = GetOutputFileName(args[0], ".prs");
                PrsFile.Write(outputFile, fileContents);
                Console.WriteLine($"PRS file \"{Path.GetFileName(outputFile)}\" successfully created! You can use it in your mod!");
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

        private static bool SomeNameLengthExceedsSeven(string[] names)
        {
            foreach (var name in names)
            {
                if (name.Length > 7)
                {
                    return true;
                }
            }

            return false;
        }
    }
}