/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;

namespace Dev2.Tests.Activities.Mocks
{
    public class ActivityOperationBrokerMock : IActivityOperationsBroker
    {
        public IActivityIOOperationsEndPoint Source { get; set; }
        public IActivityIOOperationsEndPoint Destination { get; set; }
        public IDev2CRUDOperationTO Dev2CrudOperationTO { get; set; }
        public IDev2PutRawOperationTO Dev2PutRawOperationTo { get; set; }
        public IDev2ZipOperationTO Dev2ZipOperationTO { get; set; }
        public IDev2UnZipOperationTO Dev2UnZipOperationTO   { get; set; }

        public string Get(IActivityIOOperationsEndPoint path) => Get(path, false);
        public byte[] GetBytes(IActivityIOOperationsEndPoint path) => GetBytes(path, false);
        public string Get(IActivityIOOperationsEndPoint path, bool deferredRead)
        {
            return Encoding.UTF8.GetString(GetBytes(path, deferredRead));
        }

        public byte[] GetBytes(IActivityIOOperationsEndPoint path, bool deferredRead)
        {
            Source = path;
            DeferredRead = deferredRead;
            return Encoding.UTF8.GetBytes("Successful");
        }

        public string PutRaw(IActivityIOOperationsEndPoint dst, IDev2PutRawOperationTO args)
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

        public string Create(IActivityIOOperationsEndPoint dst, IDev2CRUDOperationTO args, bool createToFile)
        {
            CreateToFile = createToFile;
            Destination = dst;
            Dev2CrudOperationTO = args;
            return "Successful";
        }

        public string Copy(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, IDev2CRUDOperationTO args)
        {
            Source = src;
            Destination = dst;
            Dev2CrudOperationTO = args;
            return "Successful";
        }

        /// <summary>
        /// Renames a file or folder from src to dst as per the value of args
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="args"></param>
        public string Rename(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, IDev2CRUDOperationTO args)
        {
            Source = src;
            Destination = dst;
            Dev2CrudOperationTO = args;
            return "Successful";
        }

        public string Move(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, IDev2CRUDOperationTO args)
        {
            Source = src;
            Destination = dst;
            Dev2CrudOperationTO = args;
            return "Successful";
        }

        public string Zip(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, IDev2ZipOperationTO args)
        {
            Source = src;
            Destination = dst;
            Dev2ZipOperationTO = args;
            return "Successful";
        }

        public string UnZip(IActivityIOOperationsEndPoint src, IActivityIOOperationsEndPoint dst, IDev2UnZipOperationTO args)
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
