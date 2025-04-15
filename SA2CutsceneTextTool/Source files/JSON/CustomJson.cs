using SA2CutsceneTextTool.Common;

namespace SA2CutsceneTextTool.JSON
{
    public class CustomJson
    {
        private List<string> strings;

        public CustomJson()
        {
            strings = new List<string>();
        }


        public void AddString(string str, JsonIndentationLevel indLvl)
        {
            strings.Add($"{new string('\t', (int)indLvl)}{str}");
        }

        public void AddString(string str)
        {
            strings.Add(str);
        }

        public void RemoveTrailingComma()
        {
            strings[strings.Count - 1] = strings[strings.Count - 1].TrimEnd(',');
        }

        public string Serialize()
        {
            return string.Join('\n', strings);
        }
    }
}
