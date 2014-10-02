
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.ComponentModel.Composition;
//using System.ComponentModel.Composition.Hosting;
//using System.ComponentModel.Composition.Primitives;
//using System.Threading;

//namespace Dev2.Composition
//{
//    public class ImportService
//    {
//        #region Class Members

//        private static ThreadLocal<ImportServiceContext> _defaultContext = new ThreadLocal<ImportServiceContext>();
//        private static ConcurrentDictionary<ImportServiceContext, ImportService> _contexts = new ConcurrentDictionary<ImportServiceContext, ImportService>();
//        private static ThreadLocal<ImportServiceContext> _currentContext = new ThreadLocal<ImportServiceContext>();

//        #endregion Class Members

//        #region Static Properties

//        public static ImportServiceContext CurrentContext
//        {
//            get
//            {
//                return _currentContext.Value;
//            }
//            set
//            {
//                _currentContext.Value = value;
//            }
//        }

//        #endregion Static Properties

//        #region Private Properties

//        /// <summary>
//        /// The primary part catalogue
//        /// </summary>
//        private AggregateCatalog MainCatalogue { get; set; }

//        /// <summary>
//        /// The container used for part composition
//        /// </summary>
//        private CompositionContainer Container { get; set; }

//        #endregion Private Properties

//        #region Methods
//        /// <summary>
//        /// Responsible for the initial initialization of the service
//        /// </summary>
//        public static void Initialize(CompositionContainer container)
//        {
//            ImportService importService = GetContextualImportService();

//            lock (importService)
//            {
//                //
//                // Set container to the one created by mefbootstrapper
//                //
//                importService.Container = container; 
//            }
//        }

//        /// <summary>
//        /// Responsible for the initial initialization of the service
//        /// </summary>
//        public static void Initialize(IEnumerable<ComposablePartCatalog> catalogues)
//        {
//            ImportService importService = GetContextualImportService();

//            lock (importService)
//            {
//                //
//                // Instantiate catalogoue, container and engine 
//                //
//                importService.MainCatalogue = new AggregateCatalog();
//                importService.Container = new CompositionContainer(importService.MainCatalogue, true);

//                //
//                // Add the first assembly to be executed in the app domain to the main catalogue, this adds all exports it contains to the container
//                //
//                foreach (ComposablePartCatalog catalogue in catalogues)
//                {
//                    SanitizeCatalogue(catalogue);
                    
//                    importService.MainCatalogue.Catalogs.Add(catalogue);
//                }
//            }
//        }

//        private static void SanitizeCatalogue(ComposablePartCatalog catalogue)
//        {
           
//        }

//        /// <summary>
//        /// Gets export values from the composition container
//        /// </summary>
//        /// <typeparam name="T">The type being retrieved.</typeparam>
//        public static IEnumerable<T> GetExportValues<T>()
//        {
//            ImportService importService = GetContextualImportService();

//            if (importService.Container == null)
//            {
//                throw new Exception("Please initialize container before using it.");
//            }

//            lock (importService)
//            {
//                return importService.Container.GetExportedValues<T>();
//            }
//        }

//        /// <summary>
//        /// Gets export values from the composition container
//        /// </summary>
//        /// <typeparam name="T">The type being retrieved.</typeparam>
//        /// <param name="contractName">The export contract.</param>
//        public static IEnumerable<T> GetExportValues<T>(string contractName)
//        {
//            ImportService importService = GetContextualImportService();

//            if (importService.Container == null)
//            {
//                throw new Exception("Please initialize container before using it.");
//            }

//            lock (importService)
//            {
//                return importService.Container.GetExportedValues<T>(contractName);
//            }
//        }

//        /// <summary>
//        /// Gets an export value from the composition container
//        /// </summary>
//        /// <typeparam name="T">The type being retrieved.</typeparam>
//        public static T GetExportValue<T>()
//        {
//            ImportService importService = GetContextualImportService();

//            if (importService.Container == null)
//            {
//                throw new Exception("Please initialize container before using it.");
//            }

//            lock (importService)
//            {
//                T value = importService.Container.GetExportedValue<T>();

//                IPostCompositionInitializable p = value as IPostCompositionInitializable;
//                if (p != null)
//                {
//                    p.Initialize();
//                }

//                return value;
//            }
//        }

//        /// <summary>
//        /// Gets an export value from the composition container
//        /// </summary>
//        /// <typeparam name="T">The type being retrieved.</typeparam>
//        /// <param name="contractName">The export contract.</param>
//        public static T GetExportValue<T>(string contractName)
//        {
//            ImportService importService = GetContextualImportService();

//            if (importService.Container == null)
//            {
//                throw new Exception("Please initialize container before using it.");
//            }

//            lock (importService)
//            {
//                T value = importService.Container.GetExportedValue<T>(contractName);

//                IPostCompositionInitializable p = value as IPostCompositionInitializable;
//                if (p != null)
//                {
//                    p.Initialize();
//                }

//                return value;
//            }
//        }

//        /// <summary>
//        /// Tries to get an export value from the composition container
//        /// </summary>
//        /// <typeparam name="T">The type being retrieved.</typeparam>
//        public static bool TryGetExportValue<T>(out T value)
//        {
//            bool returnVal = true;

//            try
//            {
//                value = GetExportValue<T>();
//            }
//            catch
//            {
//                value = default(T);
//                returnVal = false;
//            }

//            return returnVal;
//        }

//        /// <summary>
//        /// Tries to get an export value from the composition container
//        /// </summary>
//        /// <typeparam name="T">The type being retrieved.</typeparam>
//        /// <param name="contractName">The export contract.</param>
//        /// <param name="value">The exported value.</param>
//        public static bool TryGetExportValue<T>(string contractName, out T value)
//        {
//            bool returnVal = true;

//            try
//            {
//                value = GetExportValue<T>(contractName);
//            }
//            catch
//            {
//                value = default(T);
//                returnVal = false;
//            }

//            return returnVal;
//        }

//        /// <summary>
//        /// Adds a value to the composition container
//        /// </summary>
//        /// <typeparam name="T">The type of the value being added.</typeparam>
//        /// <param name="value">The value to add.</param>
//        public static void AddExportedValueToContainer<T>(T value)
//        {
//            ImportService importService = GetContextualImportService();

//            lock (importService)
//            {
//                CompositionBatch batch = new CompositionBatch();
//                batch.AddExportedValue(value);

//                if (importService.Container == null)
//                {
//                    throw new Exception("Please initialize container before using it.");
//                }

//                importService.Container.Compose(batch);
//            }
//        }

        
//        public static void ReleaseExports<T>()
//        {
//            ImportService importService = GetContextualImportService();
//            var exports = importService.Container.GetExports<T>();
//            importService.Container.ReleaseExports(exports);
//        }

//        public static void ReleaseExports(IEnumerable<Export> exports)
//        {
//            ImportService importService = GetContextualImportService();
//            importService.Container.ReleaseExports(exports);
//        }

//        /// <summary>
//        /// Satisfies the imports on an object
//        /// </summary>
//        /// <param name="attributedPart">The part who's imports to satisfy.</param>
//        public static void SatisfyImports(object attributedPart)
//        {
//            if (attributedPart == null)
//            {
//                throw new ArgumentNullException("attributedPart");
//            }

//            ImportService importService = GetContextualImportService();

//            lock (importService)
//            {
//                ComposablePart part = AttributedModelServices.CreatePart(attributedPart);
//                CompositionBatch batch = new CompositionBatch();
//                batch.AddPart(part);

//                if (importService.Container == null)
//                {
//                    throw new Exception("Please initialize container before using it.");
//                }

//                importService.Container.Compose(batch);

//                IPostCompositionInitializable p = attributedPart as IPostCompositionInitializable;
//                if (p != null)
//                {
//                    p.Initialize();
//                }

                
//            }
//        }

//        /// <summary>
//        /// Tries to satisfy the imports on an object
//        /// </summary>
//        /// <param name="attributedPart">The part who's imports to satisfy.</param>
//        public static bool TrySatisfyImports(object attributedPart)
//        {
//            try
//            {
//                SatisfyImports(attributedPart);
//            }
//            catch
//            {
//                return false;
//            }
//            return true;
//        }

//        /// <summary>
//        /// Satisfies the imports on an object
//        /// </summary>
//        /// <param name="part">The part who's imports to satisfy.</param>
//        public static void SatisfyImports(ComposablePart part)
//        {
//            ImportService importService = GetContextualImportService();

//            lock (importService)
//            {
//                CompositionBatch batch = new CompositionBatch();
//                batch.AddPart(part);

//                if (importService.Container == null)
//                {
//                    throw new Exception("Please initialize container before using it.");
//                }

//                importService.Container.Compose(batch);

//                IPostCompositionInitializable p = part as IPostCompositionInitializable;
//                if (p != null)
//                {
//                    p.Initialize();
//                }
//            }
//        }

//        /// <summary>
//        /// Tries to satisfy the imports on an object
//        /// </summary>
//        /// <param name="part">The part who's imports to satisfy.</param>
//        public static bool TrySatisfyImports(ComposablePart part)
//        {
//            try
//            {
//                SatisfyImports(part);
//            }
//            catch
//            {
//                return false;
//            }

//            return true;
//        }

//        #endregion Methods

//        #region Private Methods

//        /// <summary>
//        /// Gets the import service based on the current context
//        /// </summary>
//        private static ImportService GetContextualImportService()
//        {
//            if (CurrentContext == null)
//            {
//                if (!_defaultContext.IsValueCreated)
//                {
//                    _defaultContext.Value = new ImportServiceContext();
//                }
//                CurrentContext = _defaultContext.Value;
//            }
            
//            ImportService importService;
//            if (!_contexts.TryGetValue(CurrentContext, out importService))
//            {
//                importService = new ImportService();
//                _contexts.TryAdd(CurrentContext, importService);
//            }

//            return importService;
//        }

//        #endregion Private Methods
//    }
//}
