﻿using System.Text.Json.Serialization;

namespace SA2MessageTextTool.Common
{
    public enum Endianness
    {
        BigEndian,
        LittleEndian
    }

    public enum Encodings
    {
        Windows1251 = 1251,
        Windows1252 = 1252,
        ShiftJIS = 932
    }

    public enum JsonStyle
    {
        Indented,
        SingleLinePerEntry
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CenteringMethod : short
    {
        NotCentered,        // ignored as null
        Block = 7,          // \a
        EachLine = 9        // \t
    }

    public enum TextConversionMode
    {
        Default,
        Reversed,
    }

    public enum JsonIndentationLevel
    {
        One = 1,
        Two,
        Three,
    }
}
