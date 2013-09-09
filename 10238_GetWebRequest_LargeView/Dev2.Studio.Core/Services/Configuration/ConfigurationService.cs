using System;
using System.IO;
using Newtonsoft.Json;

namespace Dev2.Services.Configuration
{
    public class ConfigurationService : DisposableObject
    {
        public static readonly string DefaultPath = Path.Combine(new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            StringResources.App_Data_Directory,
            StringResources.User_Interface_Layouts_Directory
        });

        readonly string _filePath;

        #region CTOR

        public ConfigurationService(string filePath)
        {
            VerifyArgument.IsNotNull("filePath", filePath);

            _filePath = filePath;
        }

        #endregion

        #region Write

        public void Write()
        {
            var dirPath = Path.GetDirectoryName(_filePath);
            if(dirPath != null && !Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            var data = JsonConvert.SerializeObject(this);
            File.WriteAllText(_filePath, data);
        }


        #endregion

        #region Read

        public static T Read<T>(string filePath)
            where T : new()
        {
            var type = typeof(T);

            if(!File.Exists(filePath))
            {
                return (T)Activator.CreateInstance(type, filePath);
            }

            var value = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<T>(value);
        }

        #endregion

        #region Overrides of DisposableObject

        protected override void OnDisposed()
        {
            Write();
        }

        #endregion

    }
}