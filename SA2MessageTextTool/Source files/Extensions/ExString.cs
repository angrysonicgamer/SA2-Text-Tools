namespace SA2MessageTextTool.Extensions
{
    public static class ExString
    {
        private static readonly Dictionary<string, string> buttonsMap = new()
        {
            { "±", "{A}" },
            { "¶", "{B}" },
            { "Ё", "{Y}" },
        };

        private static readonly Dictionary<string, string> cyrillicReplacementMap = new()
        {
            { "№", "Ё" },
            { "є", "ё" },

        };

        private static string Replace(string text, Dictionary<string, string> map, TextConversionMode mode = TextConversionMode.Default)
        {
            foreach (var pair in map)
            {
                if (mode == TextConversionMode.Default)
                    text = text.Replace(pair.Key, pair.Value);
                else
                    text = text.Replace(pair.Value, pair.Key);
            }

            return text;
        }


        public static string ReplaceKeyboardButtons(this string text, TextConversionMode mode = TextConversionMode.Default)
        {
            return Replace(text, buttonsMap, mode);
        }

        public static string ModifyCyrillicCP(this string text, TextConversionMode mode = TextConversionMode.Default)
        {
            return Replace(text, cyrillicReplacementMap, mode);
        }
    }
}
