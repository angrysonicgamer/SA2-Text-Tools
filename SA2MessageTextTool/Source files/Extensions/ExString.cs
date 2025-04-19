using System.Text;
using SA2MessageTextTool.Common;

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

        private static readonly Dictionary<string, string> cyrillicButtonsMap = new()
        {
            { "±", "{A}" },
            { "¶", "{B}" },
            { "·", "{Y}" },
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


        public static string ReplaceKeyboardButtons(this string text, Encoding encoding, TextConversionMode mode = TextConversionMode.Default)
        {
            if (encoding.CodePage == (int)Encodings.Windows1251)
            {
                return Replace(text, cyrillicButtonsMap, mode);
            }
            else
            {
                return Replace(text, buttonsMap, mode);
            }            
        }
    }
}
