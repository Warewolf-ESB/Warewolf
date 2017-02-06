using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Communication;
using Dev2.PerformanceCounters.Counters;
// ReSharper disable UnusedParameter.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Dev2.PerformanceCounters.Management
{
    public class PerformanceCounterPersistence : IPerformanceCounterPersistence
    {

        private readonly IFile _file;

        #region Implementation of IPerformanceCounterPersistence

        public PerformanceCounterPersistence(IFile file)
        {

            _file = file;
        }

        public void Save(IList<IPerformanceCounter> counters, string fileName)
        {
            var serialiser = new Dev2JsonSerializer();
            _file.WriteAllText(fileName, serialiser.Serialize(counters));
        }
        public void Save(IList<IPerformanceCounter> counters)
        {
            var path = EnvironmentVariables.ServerPerfmonSettingsFile;
            Save(counters, path);
        }

        public void Save(IList<IResourcePerformanceCounter> counters)
        {
            var path = EnvironmentVariables.ServerResourcePerfmonSettingsFile;
            Save(counters.Cast<IPerformanceCounter>().ToList(), path);
        }

        public IList<IPerformanceCounter> LoadOrCreate()
        {
            return LoadOrCreate(EnvironmentVariables.ServerPerfmonSettingsFile);
        }
             [ExcludeFromCodeCoverage]
        public IList<IResourcePerformanceCounter> LoadOrCreateResourcesCounters(IList<IResourcePerformanceCounter> resourcePerformanceCounters)
        {
            return LoadOrCreateResourceCounters(EnvironmentVariables.ServerResourcePerfmonSettingsFile);
        }

             public IList<IPerformanceCounter> LoadOrCreate(string fileName)
             {
                 try
                 {


                     var serialiser = new Dev2JsonSerializer();
                     if (!_file.Exists(fileName))
                     {
                         return CreateDefaultPerfCounters();
                     }
                     return serialiser.Deserialize<IList<IPerformanceCounter>>(_file.ReadAllText(fileName));
                 }
                 catch (Exception e)
                 {
                     Dev2Logger.Error(e);
                     return CreateDefaultPerfCounters();
                 }
             }
        public IList<IResourcePerformanceCounter> LoadOrCreateResourceCounters(string fileName)
        {
            try
            {

                var serialiser = new Dev2JsonSerializer();
                if (!_file.Exists(fileName))
                {
                    return DefaultResourceCounters;
                }
                return serialiser.Deserialize<IList<IResourcePerformanceCounter>>(_file.ReadAllText(fileName));
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e);
                return DefaultResourceCounters;
            }

        }

        private IList<IPerformanceCounter> CreateDefaultPerfCounters()
        {
            var toSerialise = DefaultCounters;
            Save(toSerialise);
            return toSerialise;
        }

        public IList<IPerformanceCounter> DefaultCounters => new List<IPerformanceCounter>{
            new WarewolfCurrentExecutionsPerformanceCounter(),
            new WarewolfNumberOfErrors(),
            new WarewolfRequestsPerSecondPerformanceCounter(),
            new WarewolfAverageExecutionTimePerformanceCounter(),
            new WarewolfNumberOfAuthErrors(),
            new WarewolfServicesNotFoundCounter()
        };

        public IList<IResourcePerformanceCounter> DefaultResourceCounters => new List<IResourcePerformanceCounter>();

        #endregion
    }
}