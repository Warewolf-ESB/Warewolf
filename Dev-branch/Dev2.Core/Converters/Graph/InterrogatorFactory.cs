using System;
using System.Collections.Generic;
using Unlimited.Framework.Converters.Graph.Interfaces;
using Unlimited.Framework.Converters.Graph.Poco;
using Unlimited.Framework.Converters.Graph.String;

namespace Unlimited.Framework.Converters.Graph
{
    public class InterrogatorFactory
    {
        #region Class Members

        private static IInterrogator _defaultInterrogator;
        private static Dictionary<Type, IInterrogator> _interrogators;

        #endregion Class Members

        #region Constructors

        static InterrogatorFactory()
        {
            Interrogators = new Dictionary<Type, IInterrogator>();

            Interrogators.Add(typeof(string), new StringInterrogator());

            DefaultInterrogator = new PocoInterrogator();
        }

        #endregion Constructors

        #region Properties

        private static Dictionary<Type, IInterrogator> Interrogators
        {
            get
            {
                return _interrogators;
            }
            set
            {
                _interrogators = value;
            }
        }

        private static IInterrogator DefaultInterrogator
        {
            get
            {
                return _defaultInterrogator;
            }
            set
            {
                _defaultInterrogator = value;
            }
        }

        #endregion Properties

        #region Methods

        public static IInterrogator CreateInteregator(Type dataType)
        {
            IInterrogator interrogatror = null;
            if (Interrogators.TryGetValue(dataType, out interrogatror))
            {
                return interrogatror;
            }

            return DefaultInterrogator;
        }

        #endregion Methods
    }
}
