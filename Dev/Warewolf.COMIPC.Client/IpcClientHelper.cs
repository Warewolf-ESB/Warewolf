/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using WarewolfCOMIPC.Client;

namespace Warewolf.COMIPC.Client
{
    public class IpcClientHelper
    {
        readonly bool _disposed;
        readonly INamedPipeClientStreamWrapper _pipeWrapper;

        object result;

        public IpcClientHelper(bool disposed, INamedPipeClientStreamWrapper pipeWrapper)
        {
            _disposed = disposed;
            _pipeWrapper = pipeWrapper;
        }

        public object Invoke(Guid clsid, string function, Execute execute, ParameterInfoTO[] args)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(IpcClient));
            }

            var info = new CallData
            {
                CLSID = clsid,
                MethodToCall = function,
                Parameters = args,
                ExecuteType = execute.ToString(),
                Execute = execute
            };

            // Write request to server
            var serializer = new JsonSerializer();
            var sw = new StreamWriter(_pipeWrapper.GetInternalStream());
            serializer.Serialize(sw, JsonConvert.SerializeObject(info));
            sw.Flush();

            var sr = new StreamReader(_pipeWrapper.GetInternalStream());
            var jsonTextReader = new JsonTextReader(sr);

            switch (info.Execute)
            {

                case Execute.GetType:
                    {
                        return GetType(serializer, jsonTextReader);
                    }
                case Execute.GetMethods:
                    {
                        return GetMethodInfo(serializer, jsonTextReader);
                    }
                case Execute.GetNamespaces:
                    {
                        return GetNamespaces(serializer, jsonTextReader);

                    }
                case Execute.ExecuteSpecifiedMethod:
                    {
                        return ExecuteSpecifiedMethod(serializer, jsonTextReader);
                    }

                default:
                    return null;
            }

        }

        private object ExecuteSpecifiedMethod(JsonSerializer serializer, JsonTextReader jsonTextReader)
        {
            try
            {
                var obj = serializer.Deserialize(jsonTextReader);
                result = obj.ToString();
                var exception = JsonConvert.DeserializeObject<Exception>(result.ToString());
                if (exception != null)
                {
                    throw exception;
                }
            }
            catch (Exception ex)
            {
                // Do nothing was not an exception
                var baseException = ex.GetBaseException();
                return new KeyValuePair<bool, string>(true, baseException.Message);
            }
            return result;
        }

        private object GetNamespaces(JsonSerializer serializer, JsonTextReader jsonTextReader)
        {
            result = serializer.Deserialize(jsonTextReader, typeof(List<string>));
            if (result is Exception exception)
            {
                throw exception;
            }
            return result;
        }

        private object GetMethodInfo(JsonSerializer serializer, JsonTextReader jsonTextReader)
        {
            result = serializer.Deserialize(jsonTextReader, typeof(string));
            if (result is Exception exception)
            {
                throw exception;
            }

            var value = result?.ToString();
            return value == null ? new List<MethodInfoTO>() : JsonConvert.DeserializeObject<List<MethodInfoTO>>(value);
        }

        private object GetType(JsonSerializer serializer, JsonTextReader jsonTextReader)
        {
            result = serializer.Deserialize(jsonTextReader, typeof(string));
            if (result is Exception exception)
            {
                throw exception;
            }
            var ipCreturn = result as string;
            var reader = new StringReader(ipCreturn ?? "");

            try
            {
                return serializer.Deserialize(reader, typeof(Type));
            }
            catch (Exception ex)
            {
                // Do nothing was not an exception
                var baseException = ex.GetBaseException();
                return new KeyValuePair<bool, string>(true, baseException.Message);
            }
        }
    }

}
