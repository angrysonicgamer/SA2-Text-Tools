using SA2CutsceneTextTool.Common;

namespace SA2CutsceneTextTool.Extensions
{
    public static class ExString
    {
        private static readonly Dictionary<char, char> cyrillicReplacementMap = new Dictionary<char, char>()
        {
            { '№', 'Ё' },
            { 'є', 'ё' },
        };


        public static string ModifyCyrillicCP(this string text, TextConversionMode mode = TextConversionMode.Default)
        {
            foreach (var pair in cyrillicReplacementMap)
            {
                if (mode == TextConversionMode.Default)
                    text = text.Replace(pair.Key, pair.Value);
                else
                    text = text.Replace(pair.Value, pair.Key);
            }

            return text;
        }
    }
}
