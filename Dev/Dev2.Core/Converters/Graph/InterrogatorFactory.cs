using System;
using System.Collections.Generic;
using System.Data;
using Dev2.Converters.Graph.DataTable;
using Unlimited.Framework.Converters.Graph.Interfaces;
using Unlimited.Framework.Converters.Graph.Poco;
using Unlimited.Framework.Converters.Graph.String;

namespace Unlimited.Framework.Converters.Graph
{
    public class InterrogatorFactory
    {
        #region Class Members

        #endregion Class Members

        #region Constructors

        static InterrogatorFactory()
        {
            Interrogators = new Dictionary<Type, IInterrogator> { { typeof(string), new StringInterrogator() }, { typeof(DataTable), new DataTableInterrogator() } };

            DefaultInterrogator = new PocoInterrogator();
        }

        #endregion Constructors

        #region Properties

        static Dictionary<Type, IInterrogator> Interrogators { get; set; }

        static IInterrogator DefaultInterrogator { get; set; }

        #endregion Properties

        #region Methods

        public static IInterrogator CreateInteregator(Type dataType)
        {
            IInterrogator interrogatror;
            return Interrogators.TryGetValue(dataType, out interrogatror) ? interrogatror : DefaultInterrogator;
        }

        #endregion Methods
    }
}
