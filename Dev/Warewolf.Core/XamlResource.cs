using System;
using System.Text;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Versioning;

namespace Warewolf.Core
{
    public class XamlResource : IXamlResource
    {
        IResourceDefinition _resource;
        Guid _resourceID;

        public  XamlResource(IResourceDefinition resource,StringBuilder xaml)
        {
            Xaml = xaml;
            _resource = resource;
        }
        #region Implementation of IResourceDefinition

        /// <summary>
        /// The resource ID that uniquely identifies the resource.
        /// </summary>
        // ReSharper disable InconsistentNaming
        public Guid ResourceID
        {
            get
            {
                return _resource.ResourceID;
            }
            set
            {
                _resource.ResourceID = value;
            }
        }
        /// <summary>
        /// The display name of the resource.
        /// </summary>
        public string ResourceName
        {
            get
            {
                return _resource.ResourceName;
            }
            set
            {
                _resource.ResourceName = value;
            }
        }
        /// <summary>
        /// Gets or sets the type of the resource.
        /// </summary>
        public ResourceType ResourceType
        {
            get
            {
                return _resource.ResourceType;
            }
            set
            {
                _resource.ResourceType = value;
            }
        }
        /// <summary>
        /// Gets or sets the category of the resource.
        /// </summary>   
        public string ResourceCategory
        {
            get
            {
                return _resource.ResourceCategory;
            }
            set
            {
                _resource.ResourceCategory = value;
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether [is valid].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is valid]; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid
        {
            get
            {
                return _resource.IsValid;
            }
            set
            {
                _resource.IsValid = value;
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether [is new resource].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is new resource]; otherwise, <c>false</c>.
        /// </value>
        public bool IsNewResource
        {
            get
            {
                return _resource.IsNewResource;
            }
            set
            {
                _resource.IsNewResource = value;
            }
        }
        /// <summary>
        /// Gets or sets the data list.
        /// </summary>
        /// <value>
        /// The data list.
        /// </value>
        public string DataList
        {
            get
            {
                return _resource.DataList;
            }
            set
            {
                _resource.DataList = value;
            }
        }
        /// <summary>
        /// Gets or sets the inputs.
        /// </summary>
        /// <value>
        /// The inputs.
        /// </value>
        public string Inputs
        {
            get
            {
                return _resource.Inputs;
            }
            set
            {
                _resource.Inputs = value;
            }
        }
        /// <summary>
        /// Gets or sets the outputs.
        /// </summary>
        /// <value>
        /// The outputs.
        /// </value>
        public string Outputs
        {
            get
            {
                return _resource.Outputs;
            }
            set
            {
                _resource.Outputs = value;
            }
        }
        public IVersionInfo VersionInfo
        {
            get
            {
                return _resource.VersionInfo;
            }
            set
            {
                _resource.VersionInfo = value;
            }
        }
        /// <summary>
        /// Gets or sets the permissions of the resource
        /// </summary>
        public Permissions Permissions
        {
            get
            {
                return _resource.Permissions;
            }
            set
            {
                _resource.Permissions = value;
            }
        }

        #endregion

        #region Implementation of IXamlResource

        public StringBuilder Xaml { get; set; }

        #endregion
    }
}