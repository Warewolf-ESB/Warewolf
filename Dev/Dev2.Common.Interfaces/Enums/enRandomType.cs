using System.ComponentModel;

namespace Dev2.Common.Interfaces.Enums
{
    public enum enRandomType
    {
        [Description("Letters")]
        Letters,
        [Description("Numbers")]
        Numbers,
        [Description("Letters & Numbers")]
        LetterAndNumbers,
        [Description("GUID")]
        Guid
    }
}
