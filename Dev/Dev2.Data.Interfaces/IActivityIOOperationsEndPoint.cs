/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.Interfaces.Enums;
using System.Collections.Generic;
using System.IO;

namespace Dev2.Data.Interfaces
{
    public interface IActivityIOOperationsEndPoint
    {        
        IActivityIOPath IOPath { get; set; }
        
        Stream Get(IActivityIOPath path, List<string> filesToCleanup);
        
        int Put(Stream src, IActivityIOPath dst, IDev2CRUDOperationTO args, string whereToPut, List<string> filesToCleanup);
        
        bool Delete(IActivityIOPath src);
        
        IList<IActivityIOPath> ListDirectory(IActivityIOPath src);
        
        bool CreateDirectory(IActivityIOPath dst, IDev2CRUDOperationTO args);
        
        bool PathExist(IActivityIOPath dst);
        
        bool RequiresLocalTmpStorage();
        
        bool HandlesType(enActivityIOPathType type);
        
        enPathType PathIs(IActivityIOPath path);
        
        string PathSeperator();
        
        IList<IActivityIOPath> ListFoldersInDirectory(IActivityIOPath src);
        
        IList<IActivityIOPath> ListFilesInDirectory(IActivityIOPath src);
    }
}
