
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
using Dev2.PathOperations;

namespace Dev2.Tests.Activities.Mocks
{
    public class ActivityOperationBrokerMock : IActivityOperationsBroker
    {
        public IActivityIOOperationsEndPoint Source { get; set; }
        public IActivityIOOperationsEndPoint Destination { get; set; }
        public Dev2CRUDOperationTO Dev2CRUDOperationTO { get; set; }
        public Dev2PutRawOperationTO Dev2PutRawOperationTo { get; set; }
        public Dev2ZipOperationTO Dev2ZipOperationTO { get; set; }
        public Dev2UnZipOperationTO Dev2UnZipOperationTO   { get; set; }

        public string Get(IActivityIOOperationsEndPoint path, bool deferredRead = false)
        {
            Source = path;
            DeferredRead = deferredRead;
            return "Successful";
        }

        public Stream GetRaw(IActivityIOOperationsEndPoint path)
        {
            throw new NotImplementedException();
        }

        public string PutRaw(IActivityIOOperationsEndPoint dst, Dev2PutRawOperationTO args)
        {
            Destination = dst;
            Dev2PutRawOperationTo = args;
            return "Successful";
        }

        public string Delete(IActivityIOOperationsEndPoint src)
        {
            Source = src;
            return "Successful";
        }

        public IList<IActivityIOPath> ListDirectory(IActivityIOOperationsEndPoint src,ReadTypes readTypes)
        {
            throw new NotImplementedException();
        }

        public string Create(IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args, bool createToFile)
        {
            CreateToFile = createToFile;
            Destination = dst;
            Dev2CRUDOperationTO = args;
            return "Successful";
        }

        public string Copy(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args)
        {
            Source = src;
            Destination = dst;
            Dev2CRUDOperationTO = args;
            return "Successful";
        }

        /// <summary>
        /// Renames a file or folder from src to dst as per the value of args
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="args"></param>
        public string Rename(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args)
        {
            Source = src;
            Destination = dst;
            Dev2CRUDOperationTO = args;
            return "Successful";
        }

        public string Move(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, Dev2CRUDOperationTO args)
        {
            Source = src;
            Destination = dst;
            Dev2CRUDOperationTO = args;
            return "Successful";
        }

        public string Zip(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, Dev2ZipOperationTO args)
        {
            Source = src;
            Destination = dst;
            Dev2ZipOperationTO = args;
            return "Successful";
        }

        public string UnZip(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, Dev2UnZipOperationTO args)
        {
            Source = src;
            Destination = dst;
            Dev2UnZipOperationTO = args;
            return "Successful";
        }

        public bool CreateToFile { get; set; }

        public bool DeferredRead { get; set; }
    }
}
