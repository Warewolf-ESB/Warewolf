/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dev2.Common
{

    public interface IDuplicateResource
    {
        Guid ResourceId { get; set; }
        string ResourceName { get; set; }
        List<string> ResourcePath { get; set; }
    }
    public class DuplicateResource:IDuplicateResource
    {
        #region Implementation of IDuplicateResource

        public Guid ResourceId { get; set; }
        public string ResourceName { get; set; }
        public List<string> ResourcePath { get; set; }

        #endregion
    }

    //TODO: these classes can be merged later 
    public class DuplicateResourceTO
    {
        public Guid OldResourceID { get; set; }
        public string DestinationPath { get; set; }
        public StringBuilder ResourceContents { get; set; }
        public IResource NewResource { get; set; }
    }
}