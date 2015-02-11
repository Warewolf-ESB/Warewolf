using System;

namespace Dev2.Common.Interfaces
{
    public interface IEsbRequestResult<T>
    {
        bool HasErrors { get; set; }
        Exception Error { get; set; }
        T Value { get; set; }
    }
}
