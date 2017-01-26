using System;
using System.Collections.Generic;

namespace Dev2.Common.Interfaces
{
    public interface IConstructor
    {
        IList<IConstructorParameter> Inputs { get; set; }
        string ReturnObject { get; set; }
        string ConstructorName { get; set; }
        string GetIdentifier();
    }

    public interface IPluginConstructor : IConstructor
    {
        bool IsExistingObject { get; set; }
        Guid ID { get; set; }
    }
}