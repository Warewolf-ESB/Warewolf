using System;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.PathOperations
{

    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide a factory for creating ActivityIO objects
    /// </summary>
    public static class ActivityIOFactory
    {

        private static IList<Type> _endPoints = null;
        private static object _endPointsLock = new object();
        // used to check what type services what
        private static IList<IActivityIOOperationsEndPoint> _referenceCheckers = null;

        /// <summary>
        /// Return an IActivityIOPath based upont the path string
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IActivityIOPath CreatePathFromString(string path, bool IsNotCertVerifiable)
        {
            return CreatePathFromString(path, string.Empty, string.Empty, IsNotCertVerifiable);
        }

        /// <summary>
        /// Return an IActivityIOPath based upont the path string
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IActivityIOPath CreatePathFromString(string path, string user, string pass)
        {
            return CreatePathFromString(path, user, pass, false);
        }

        /// <summary>
        /// Return an IActivityIOPath based upont the path string
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IActivityIOPath CreatePathFromString(string path, string user, string pass, bool IsNotCertVerifiable)
        {
            // Fetch path type
            enActivityIOPathType type = Dev2ActivityIOPathUtils.ExtractPathType(path);
            if(type == enActivityIOPathType.Invalid)
            {
                // Default to file system
                type = enActivityIOPathType.FileSystem;
            }

            return new Dev2ActivityIOPath(type, path, user, pass, IsNotCertVerifiable);
        }

        /// <summary>
        /// Return the appropriate operation end point based upon the IOPath type
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static IActivityIOOperationsEndPoint CreateOperationEndPointFromIOPath(IActivityIOPath target)
        {

            lock(_endPointsLock)
            {
                // load end-points if need be... aka first load
                if(_endPoints == null)
                {
                    _endPoints = new List<Type>();
                    _referenceCheckers = new List<IActivityIOOperationsEndPoint>();

                    var type = typeof(IActivityIOOperationsEndPoint);

                    List<Type> types = typeof(IActivityIOOperationsEndPoint).Assembly.GetTypes()
                                                                             .Where(t => (type.IsAssignableFrom(t)))
                                                                             .ToList();

                    foreach(Type t in types)
                    {
                        if(t != typeof(IActivityIOOperationsEndPoint))
                        {
                            _endPoints.Add(t);
                            _referenceCheckers.Add((IActivityIOOperationsEndPoint)Activator.CreateInstance(t));
                        }
                    }
                }
            }

            // now find the right match ;)
            int pos = 0;

            while(pos < _referenceCheckers.Count && !_referenceCheckers[pos].HandlesType(target.PathType))
            {
                pos++;
            }

            // will throw exception if cannot find handling type
            IActivityIOOperationsEndPoint result =
                (IActivityIOOperationsEndPoint)Activator.CreateInstance(_endPoints[pos]);
            result.IOPath = target;


            return result;
        }

        /// <summary>
        /// Create an operations broker object
        /// </summary>
        /// <returns></returns>
        public static IActivityOperationsBroker CreateOperationsBroker()
        {
            return new Dev2ActivityIOBroker();
        }

        /// <summary>
        /// Create an PutRawOperationTo object
        /// </summary>
        /// <returns></returns>
        public static Dev2PutRawOperationTO CreatePutRawOperationTO(WriteType writeType, string contents)
        {
            return new Dev2PutRawOperationTO(writeType, contents);
        }

        /// <summary>
        /// Create an UnZipOperationTO object
        /// </summary>
        /// <returns></returns>
        public static Dev2UnZipOperationTO CreateUnzipTO(string passwd, bool overwrite)
        {
            return new Dev2UnZipOperationTO(passwd, overwrite);
        }

        /// <summary>
        /// Create an ZipOperationTO object
        /// </summary>
        /// <returns></returns>
        public static Dev2ZipOperationTO CreateZipTO(string ratio, string passwd, string name, bool overwrite = false)
        {
            return new Dev2ZipOperationTO(ratio, passwd, name, overwrite);
        }
    }
}
