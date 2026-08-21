using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Core.Graph;


namespace Unlimited.Framework.Converters.Graph.String.Json
{
    public class StringMapper : IMapper
    {
        #region Methods

        public IEnumerable<IPath> Map(object data) => new List<IPath>
            {
                new StringPath
                {
                    ActualPath = "Response",
                    DisplayPath = "Response",
                    SampleData = "",
                    OutputExpression = ""
                }
            };

        #endregion Methods

    }


}