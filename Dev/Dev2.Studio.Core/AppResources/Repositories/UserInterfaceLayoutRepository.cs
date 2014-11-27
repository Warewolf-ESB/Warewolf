
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
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Dev2.Common.Interfaces;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.AppResources.Repositories
{
    public class UserInterfaceLayoutRepository : IFrameworkRepository<UserInterfaceLayoutModel>
    {
        #region Class Members

        private readonly Dictionary<UserInterfaceLayoutModel, string> _userInterfaceLayoutModels;
        public event EventHandler ItemAdded;
        private bool _isDisposed;

        #endregion Class Members

        #region Constructors

        public UserInterfaceLayoutRepository()
        {
            _userInterfaceLayoutModels = new Dictionary<UserInterfaceLayoutModel, string>();
        }

        #endregion Constructors

        #region Properties

        public IFilePersistenceProvider FilePersistenceProvider { get; set; }

        #endregion Properties

        #region Methods

        public ICollection<UserInterfaceLayoutModel> All()
        {
            return _userInterfaceLayoutModels.Keys;
        }

        public ICollection<UserInterfaceLayoutModel> Find(Expression<Func<UserInterfaceLayoutModel, bool>> expression)
        {
            Func<UserInterfaceLayoutModel, bool> func = expression.Compile();
            return All().Where(func).ToList();
        }

        public UserInterfaceLayoutModel FindSingle(Expression<Func<UserInterfaceLayoutModel, bool>> expression)
        {
            Func<UserInterfaceLayoutModel, bool> func = expression.Compile();
            return All().FirstOrDefault(func);
        }

        public string Save(UserInterfaceLayoutModel instanceObj)
        {
            if(instanceObj == null) return "Not Saved";

            string file;
            if(_userInterfaceLayoutModels.TryGetValue(instanceObj, out file))
            {
                if(Path.GetFileNameWithoutExtension(file) != instanceObj.LayoutName)
                {
                    DirectoryInfo di = new DirectoryInfo(file);
                    string newFile = Path.Combine(di.FullName, instanceObj.LayoutName + ".xml");

                    if(File.Exists(newFile))
                    {
                        throw new Exception("Layout with that name already exists.");
                    }

                    FilePersistenceProvider.Delete(file);

                    _userInterfaceLayoutModels[instanceObj] = newFile;
                    file = newFile;
                }
            }
            else
            {
                string path = GetLayoutDirectory();

                DirectoryInfo di = new DirectoryInfo(path);
                if(!di.Exists)
                {
                    di.Create();
                }

                string newFile = Path.Combine(di.FullName, instanceObj.LayoutName + ".xml");
                _userInterfaceLayoutModels.Add(instanceObj, newFile);
                file = newFile;

                OnItemAdded(instanceObj);
            }

            FilePersistenceProvider.Write(file, instanceObj.MainViewDockingData);
            return "Saved";
        }

        public void Save(ICollection<UserInterfaceLayoutModel> instanceObjs)
        {
            foreach(UserInterfaceLayoutModel userInterfaceLayoutModel in instanceObjs)
            {
                Save(userInterfaceLayoutModel);
            }
        }

        public void Load()
        {
            _userInterfaceLayoutModels.Clear();

            foreach(string file in GetLayoutFiles())
            {
                UserInterfaceLayoutModel userInterfaceLayoutModel = ModelFromFile(file);
                if(userInterfaceLayoutModel != null)
                {
                    _userInterfaceLayoutModels.Add(userInterfaceLayoutModel, file);
                }
            }
        }

        public void Remove(UserInterfaceLayoutModel instanceObj)
        {
            if(instanceObj == null) return;

            string file;
            if(_userInterfaceLayoutModels.TryGetValue(instanceObj, out file) && File.Exists(file))
            {
                FilePersistenceProvider.Delete(file);
                _userInterfaceLayoutModels.Remove(instanceObj);
            }
        }

        public void Remove(ICollection<UserInterfaceLayoutModel> instanceObjs)
        {
            foreach(UserInterfaceLayoutModel userInterfaceLayoutModel in instanceObjs)
            {
                Remove(userInterfaceLayoutModel);
            }
        }

        #endregion Methods

        #region Private Methods

        private IEnumerable<string> GetLayoutFiles()
        {
            string path = GetLayoutDirectory();
            if(!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return Directory.GetFiles(path);
        }

        private static string GetLayoutDirectory()
        {
            string path = Path.Combine(new[] 
                { 
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                    StringResources.App_Data_Directory, 
                    StringResources.User_Interface_Layouts_Directory 
                });

            return path;
        }

        private UserInterfaceLayoutModel ModelFromFile(string file)
        {
            UserInterfaceLayoutModel userInterfaceLayoutModel = null;

            if(FilePersistenceProvider != null)
            {
                userInterfaceLayoutModel = new UserInterfaceLayoutModel
                {
                    LayoutName = Path.GetFileNameWithoutExtension(file),
                    MainViewDockingData = FilePersistenceProvider.Read(file)
                };
            }
            return userInterfaceLayoutModel;
        }

        #endregion Private Methods

        #region Protected Methods

        protected void OnItemAdded(object sender)
        {
            if(ItemAdded != null)
            {
                ItemAdded.Invoke(sender, new System.EventArgs());
            }
        }

        #endregion Protected Methods

        #region Implementation of IDisposable

        ~UserInterfaceLayoutRepository()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);

            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if(!_isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if(disposing)
                {
                    // Dispose managed resources.                    
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                _isDisposed = true;
            }
        }

        #endregion
    }
}
