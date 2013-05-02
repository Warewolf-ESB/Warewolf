using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Dev2.Studio.Core.AppResources.Exceptions;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Network;
using Unlimited.Framework;

namespace Dev2.Studio.Core.AppResources.Repositories
{
    /// <summary>
    /// Repository for web resources for a website.
    /// This repository serves as the back-end abstraction for all processes required to interact with web resources.
    /// </summary>
    public class WebResourceRepository : IFrameworkRepository<IWebResourceViewModel>
    {
        private readonly List<IWebResourceViewModel> _db;
        private IContextualResourceModel _resource;
        private bool _isDisposed;

        public WebResourceRepository(IContextualResourceModel resource)
        {
            _db = new List<IWebResourceViewModel>();
            _resource = resource;
        }

        public ICollection<IWebResourceViewModel> All()
        {
            return _db;
        }

        public ICollection<IWebResourceViewModel> Find(System.Linq.Expressions.Expression<Func<IWebResourceViewModel, bool>> expression)
        {
            throw new NotImplementedException();
        }

        public IWebResourceViewModel FindSingle(System.Linq.Expressions.Expression<Func<IWebResourceViewModel, bool>> expression)
        {
            throw new NotImplementedException();
        }

        public event EventHandler ItemAdded;

        protected void OnItemAdded()
        {
            if(ItemAdded != null)
            {
                ItemAdded(this, new System.EventArgs());
            }
        }

        public void Load()
        {
            _db.Clear();
            if(!string.IsNullOrEmpty(_resource.ResourceName))
            {
                if(string.IsNullOrEmpty(StringResources.Supported_WebResource_Folders))
                {
                    throw new ArgumentNullException("Supported WebResource Folders", "The Supported_WebResource_Folders Application Setting has not be configured correctly");
                }

                if(_resource == null)
                {
                    throw new ArgumentNullException("Resource");
                }

                if(_resource.Environment == null)
                {
                    throw new ArgumentNullException("Resource.Environment");
                }
                if(WebServer.IsServerUp(_resource))
                {
                    StringResources.Supported_WebResource_Folders
                        .Split(new char[] { ',' })
                        .ToList()
                        .ForEach(c =>
                        {

                            string uri = string.Format("{0}list/themes/{1}/{2}/", _resource.Environment.Connection.WebServerUri.AbsoluteUri, _resource.ResourceName, c);
                            string folderUri = string.Format("themes/{0}/{1}", _resource.ResourceName, c);

                            var folderRoot = new WebResourceViewModel(null) { Name = c, IsFolder = true, Uri = folderUri };
                            try
                            {
                                string output = ResourceHelper.Get(uri);

                                UnlimitedObject resourceList = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(output);
                                if(!resourceList.HasError)
                                {
                                    var files = resourceList.GetAllElements("File");
                                    foreach(dynamic file in files)
                                    {
                                        string fileName = file.Inner();
                                        //Brendon.Page, 2012-10-26, This if statement is unacceptable. It is a hardcoded mechanism to deal with
                                        //                          websites under SVN source control!
                                        if(!fileName.Contains(".svn"))
                                        {
                                            folderRoot.AddChild(new WebResourceViewModel(folderRoot) { Name = file.Inner(), Uri = folderRoot.Uri });
                                        }
                                    }
                                }
                            }
                            catch(WebException)
                            {

                            }

                            _db.Add(folderRoot);
                        });
                }
            }
        }

        public void Remove(ICollection<IWebResourceViewModel> instanceObjs)
        {
            throw new NotImplementedException();
        }

        public void Remove(IWebResourceViewModel instanceObj)
        {
            string uri = string.Format("{0}services/{1}", _resource.Environment.Connection.WebServerUri.AbsoluteUri, StringResources.Services_Delete_Resource);

            dynamic filePathData = new UnlimitedObject();
            filePathData.FilePath = instanceObj.Name;

            string result = ResourceHelper.Post(uri, filePathData.Inner());

            var validate = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(result);

            if(validate.HasError)
            {
                throw new WebResourceUploadFailedException(instanceObj, validate.Error);
            }

            if(instanceObj.Parent != null)
            {
                instanceObj.Parent.Children.Remove(instanceObj);
            }
            else
            {
                _db.Remove(instanceObj);
            }
        }

        public void Save(ICollection<IWebResourceViewModel> instanceObjs)
        {
            throw new NotImplementedException();
        }

        public void Save(IWebResourceViewModel instanceObj)
        {
            dynamic xml = new UnlimitedObject();
            xml.dstPathPart = instanceObj.Uri.Replace('/', '\\');
            xml.Dev2WebsiteResourceUpload_filename = instanceObj.Name;
            // Travis.Frisinger - Required to facilitate the upload module
            xml.Dev2WebsiteResourceUpload = "Content-Type:BASE64" + instanceObj.Base64Data;

            string uri = string.Format("{0}services/{1}", _resource.Environment.Connection.WebServerUri.AbsoluteUri, StringResources.Services_Add_Resource);
            string result = ResourceHelper.Post(uri, xml.XmlString);
            var validate = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(result);

            if(validate.HasError)
            {
                throw new WebResourceUploadFailedException(instanceObj, validate.Error);
            }

            instanceObj.Name = string.Format("/{0}/{1}", instanceObj.Uri, instanceObj.Name);

            if(instanceObj.Parent != null)
            {
                instanceObj.Parent.Children.Add(instanceObj);
            }
            else
            {
                instanceObj.Children.Add(instanceObj);
            }

        }

        #region Implementation of IDisposable

        ~WebResourceRepository()
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
