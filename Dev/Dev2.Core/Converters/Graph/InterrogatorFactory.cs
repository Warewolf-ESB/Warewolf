/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Data;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Converters.Graph.DataTable;
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
            Interrogators = new Dictionary<Type, IInterrogator>
            {
                {typeof (string), new StringInterrogator()},
                {typeof (DataTable), new DataTableInterrogator()}
            };

            DefaultInterrogator = new PocoInterrogator();
        }

        #endregion Constructors

        #region Properties

        private static Dictionary<Type, IInterrogator> Interrogators { get; set; }

        private static IInterrogator DefaultInterrogator { get; set; }

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