using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Core.Graph;

// ReSharper disable once CheckNamespace
namespace Unlimited.Framework.Converters.Graph.String.Json
{
    [Serializable]
    public class StringMapper : IMapper
    {
        #region Methods

        public IEnumerable<IPath> Map(object data)
        {
            return new List<IPath>
            {
                new StringPath
                {
                    ActualPath = "Response",
                    DisplayPath = "Response",
                    SampleData = "",
                    OutputExpression = ""
                }
            };
        }

        #endregion Methods

    }


}