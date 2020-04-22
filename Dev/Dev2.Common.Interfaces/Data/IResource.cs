#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Security;
using Newtonsoft.Json;
using Warewolf.Data;

namespace Dev2.Common.Interfaces.Data
{
    public interface IResource : IWarewolfResource, IEquatable<IResource>
    {
        Guid ResourceID { get; set; }
        
        IVersionInfo VersionInfo { get; set; }
        
        string ResourceName { get; set; }
        
        string ResourceType { get; set; }
        
        string FilePath { get; set; }
        
        string AuthorRoles { get; set; }
        
        bool IsUpgraded { get; set; }
        
        bool IsNewResource { get; set; }

        IList<IResourceForTree> Dependencies { get; set; }

        bool IsValid { get; set; }

        List<IErrorInfo> Errors { get; set; }

        StringBuilder DataList { get; set; }

        [JsonIgnore]
        string Inputs { get; set; }

        [JsonIgnore]
        string Outputs { get; set; }
        Permissions UserPermissions { get; set; }

        XElement ToXml();
        
        StringBuilder ToStringBuilder();
        
        XElement UpgradeXml(XElement xml, IResource resource);

        void ReadDataList(XElement xml);

        void GetInputsOutputs(XElement xml);

        void SetIsNew(XElement xml);
        
        void UpdateErrorsBasedOnXML(XElement xml);
        
       bool IsSource { get; }
       bool IsService { get; }
       bool IsFolder { get; }
       bool IsReservedService { get; }
       bool IsServer { get; }
       bool IsResourceVersion { get; }
        bool HasDataList { get; }

        void LoadDependencies(XElement xml);

        string GetResourcePath(Guid workspaceID);

        string GetSavePath();
    }
}