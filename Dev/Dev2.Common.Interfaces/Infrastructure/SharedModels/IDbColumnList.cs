using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Infrastructure.SharedModels
{
    public interface IDbColumnList
    {
        bool HasErrors { get; set; }
        string Errors { get; set; }
        List<IDbColumn> Items { get; }
    }
}