using System;
namespace Dev2.DataList.Contract
{
    /// <summary>
    /// Interface for the IRecsetSearch class
    /// </summary>
    public interface IRecsetSearch
    {
        string FieldsToSearch { get; set; }
        string Result { get; set; }
        string SearchCriteria { get; set; }
        string SearchType { get; set; }
        string StartIndex { get; set; }
        bool MatchCase { get; set; }
    }
}
