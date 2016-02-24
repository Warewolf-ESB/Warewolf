using System.Collections.Generic;
using Dev2.Common;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Communication;
using Dev2.PerformanceCounters.Counters;

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

        public IList<IPerformanceCounter> LoadOrCreate()
        {
            return LoadOrCreate(EnvironmentVariables.ServerPerfmonSettingsFile);
        }

        public IList<IPerformanceCounter> LoadOrCreate(string fileName)
        {
            var serialiser = new Dev2JsonSerializer();
            if (!_file.Exists(fileName))
            {
                return CreateDefaultPerfCounters();
            }
            return serialiser.Deserialize<IList<IPerformanceCounter>>(_file.ReadAllText(fileName));
        }

        private IList<IPerformanceCounter> CreateDefaultPerfCounters()
        {
            var toSerialise = DefaultCounters;
            Save(toSerialise);
            return toSerialise;
        }

        public IList<IPerformanceCounter> DefaultCounters
        {
            get
            {
                return new List<IPerformanceCounter>{
                                                       new WarewolfCurrentExecutionsPerformanceCounter(),
                                                       new WarewolfNumberOfErrors(),
                                                       new WarewolfRequestsPerSecondPerformanceCounter(),
                                                       new WarewolfAverageExecutionTimePerformanceCounter(),
                                                       new WarewolfNumberOfAuthErrors(),
                                                       new WarewolfServicesNotFoundCounter()
                                                    };
            }
        }

        #endregion
    }
}