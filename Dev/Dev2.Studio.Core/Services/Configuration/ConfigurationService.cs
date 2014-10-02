
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
