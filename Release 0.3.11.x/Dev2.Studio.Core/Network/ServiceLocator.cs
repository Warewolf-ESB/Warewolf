using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Dev2.Composition;

namespace Dev2.Studio.Core.Network
{
    [Export(typeof(IServiceLocator))]
    public class ServiceLocator : IServiceLocator, IPostCompositionInitializable
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceLocator" /> class.
        /// </summary>
        public ServiceLocator()
        {
            EndPointGenerationStrategies = new Dictionary<string, object>();
            StaticEndpoints = new Dictionary<string, Uri>();
        }

        #endregion Constructor

        #region Properties

        [ImportMany(typeof(IServiceEndpointGenerationStrategyProvider))]
        public List<IServiceEndpointGenerationStrategyProvider> EndpointGenerationStrategyProviders { get; set; }

        #endregion Properties

        #region Private Properties

        /// <summary>
        /// Gets or sets the end point generation strategies.
        /// </summary>
        /// <value>
        /// The end point generation strategies.
        /// </value>
        private Dictionary<string, object> EndPointGenerationStrategies { get; set; }

        /// <summary>
        /// Gets or sets the static endpoints.
        /// </summary>
        /// <value>
        /// The static endpoints.
        /// </value>
        private Dictionary<string, Uri> StaticEndpoints { get; set; }

        #endregion Private Properties

        #region Methods

        /// <summary>
        /// Registers an enpoint generation strategy.
        /// </summary>
        /// <typeparam name="T">The type of the parameter that the endpoint strategy accepts.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="enpointGenerationStrategy">The function containing the logic for generating the enpoint.</param>
        public void RegisterEnpoint<T>(string key, Func<T, Uri> enpointGenerationStrategy)
        {
            //
            // Check parameters
            //
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (enpointGenerationStrategy == null)
            {
                throw new ArgumentNullException("enpointGenerationStrategy");
            }

            //
            // Check if key has been used to register a static endpoint
            //
            Uri staticEndpoint;
            if (StaticEndpoints.TryGetValue(key, out staticEndpoint))
            {
                throw new Exception("The key '" + key + "' has already been used to register a static enpoint.");
            }

            //
            // Update or add new generation strategy
            //
            object untypedEnpointGenerationStrategy;
            if (!EndPointGenerationStrategies.TryGetValue(key, out untypedEnpointGenerationStrategy))
            {
                EndPointGenerationStrategies.Add(key, enpointGenerationStrategy);
            }
            else
            {
                EndPointGenerationStrategies[key] = enpointGenerationStrategy;
            }
        }

        /// <summary>
        /// Registers a static endpoint.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="endpoint">The static endpoint.</param>
        public void RegisterEnpoint(string key, Uri endpoint)
        {
            //
            // Check parameters
            //
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (endpoint == null)
            {
                throw new ArgumentNullException("endpoint");
            }

            //
            // Check if key has been used to register an endpoint generation strategy
            //
            object untypedEnpointGenerationStrategy;
            if (EndPointGenerationStrategies.TryGetValue(key, out untypedEnpointGenerationStrategy))
            {
                throw new Exception("The key '" + key + "' has already been used to register an enpoint generation strategy.");
            }

            //
            // Update or add new generation strategy
            //
            Uri staticEndpoint;
            if (!StaticEndpoints.TryGetValue(key, out staticEndpoint))
            {
                StaticEndpoints.Add(key, endpoint);
            }
            else
            {
                StaticEndpoints[key] = endpoint;
            }            
        }

        /// <summary>
        /// Gets a static endpoint.
        /// </summary>
        /// <param name="key">The key.</param>
        public Uri GetEndpoint(string key)
        {
            //
            // Check parameters
            //
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (!AlreadyInitialized)
            {
                Initialize();
            }

            //
            // Try get the static endpoint for the given key
            //
            Uri staticEndpoint;
            if (!StaticEndpoints.TryGetValue(key, out staticEndpoint))
            {
                staticEndpoint = null;
            }

            return staticEndpoint;
        }

        /// <summary>
        /// Gets the endpoint.
        /// </summary>
        /// <typeparam name="T">The type of the parameter that the endpoint strategy accepts.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="arg">The argument for the generation strategy.</param>
        public Uri GetEndpoint<T>(string key, T arg)
        {
            //
            // Check parameters
            //
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (!AlreadyInitialized)
            {
                Initialize();
            }

            //
            // Try get the static endpoint generation strategy for the given key
            //
            object untypedEnpointGenerationStrategy;
            if (!EndPointGenerationStrategies.TryGetValue(key, out untypedEnpointGenerationStrategy))
            {
                return null;
            }

            //
            // Cast the strategy to the correct type
            //
            Func<T, Uri> endpointGenerationStrategy = untypedEnpointGenerationStrategy as Func<T, Uri>;
            if (endpointGenerationStrategy == null)
            {
                throw new Exception("The endpoint generation strategy registered for '" + key + "' was registered as '" + untypedEnpointGenerationStrategy.GetType().ToString() + "' but the caller expected a type of '" + typeof(Func<T, Uri>).ToString() + "'.");
            }

            //
            // Run the generation strategy
            //
            Uri endpoint = null;
            try
            {
                endpoint = endpointGenerationStrategy(arg);
            }
            catch (Exception e)
            {
                throw new Exception("An error occured while generating the Uri for '" + key + "'.", e);
            }

            return endpoint;
        }

        #endregion Methods

        public void Initialize()
        {
            if (AlreadyInitialized || EndpointGenerationStrategyProviders == null)
            {
                return;
            }

            AlreadyInitialized = true;

            foreach (IServiceEndpointGenerationStrategyProvider provider in EndpointGenerationStrategyProviders)
            {
                provider.RegisterEndpointGenerationStrategies(this);
            }
        }

        public bool AlreadyInitialized { get; set; }
    }
}
