using System.Collections.Generic;
using System.IO;
using Dev2.Common;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Communication;

namespace Dev2.PerformanceCounters
{
    public class PerformanceCounterPersistence :IPerformanceCounterPersistence
    {
        private readonly IWarewolfPerformanceCounterRegister _register;

        #region Implementation of IPerformanceCounterPersistence

        public  PerformanceCounterPersistence(IWarewolfPerformanceCounterRegister register)
        {
            _register = register;
        }

        public void Save(IList<IPerformanceCounter> counters, string fileName)
        {
           
        }
        public void Save(IList<IPerformanceCounter> counters)
        {
            var path = EnvironmentVariables.ServerPerfmonSettingsFile;
            var serialiser = new Dev2JsonSerializer();
            File.WriteAllText(path,serialiser.Serialize(counters));
        }
        public IList<IPerformanceCounter> LoadOrCreate(string fileName)
        {
            var path = fileName;
            var serialiser = new Dev2JsonSerializer();
            if(!File.Exists(fileName))
            {
                return CreateDefaultPerfCounters();
            }
            else
            {
                return serialiser.Deserialize<IList<IPerformanceCounter>>(File.ReadAllText(fileName));
            }
        }

        private IList<IPerformanceCounter> CreateDefaultPerfCounters()
        {
            var toSerialise =  _register.DefaultCounters;
            Save(toSerialise);
            return toSerialise;
        }

        #endregion
    }
}