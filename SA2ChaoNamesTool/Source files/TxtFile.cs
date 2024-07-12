namespace SA2ChaoNamesTool
{
    public static class TxtFile
    {
        public static string[] Read(string inputFile)
        {
            return File.ReadAllLines(inputFile);
        }
        
        public static void Write(string outputFile, List<string> contents)
        {
            File.WriteAllLines(outputFile, contents);
        }
    }
}
