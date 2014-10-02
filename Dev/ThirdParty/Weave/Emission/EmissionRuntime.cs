
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.IO;

namespace System.Emission
{
    public sealed class EmissionRuntime : BaseDisposable
    {
        #region Instance Fields
        private AssemblyBuilder _builder;
        private string _assemblyName;
        private string _moduleName;
        private string _moduleExtension;
        private string _moduleDirectory;
        private AssemblyBuilderAccess _assemblyAccess;

        private int _moduleCount;
        private object _builderGuard;
        #endregion

        #region Constructors
        public EmissionRuntime(string assemblyName, string modulePath, bool memoryOnly)
        {
            if (String.IsNullOrEmpty(assemblyName)) throw new ArgumentException("Assembly Name cannot be a null or empty string.", "assemblyName");
            if (String.IsNullOrEmpty(modulePath)) throw new ArgumentException("Module Path cannot be a null or empty string.", "modulePath");
            

            _builderGuard = new object();
            _assemblyName = assemblyName;

            if (Path.HasExtension(modulePath))
            {
                _moduleName = Path.GetFileNameWithoutExtension(modulePath);
                _moduleExtension = Path.GetExtension(modulePath);
                _moduleDirectory = Path.GetDirectoryName(modulePath);
            }
            else
            {
                _moduleName = Path.GetFileName(modulePath);
                _moduleExtension = ".dll";
            }

            if (String.IsNullOrEmpty(_moduleDirectory)) _moduleDirectory = null;
            _assemblyAccess = memoryOnly ? AssemblyBuilderAccess.Run : AssemblyBuilderAccess.RunAndSave;
        }
        #endregion

        #region Ensure(...) Handling
        private void EnsureAssembly()
        {
            if (IsDisposed) throw new ObjectDisposedException("EmissionRuntime");

            if (_builder == null)
            {
                AssemblyName assemblyName = new AssemblyName() { Name = _assemblyName };

                if ((_assemblyAccess & AssemblyBuilderAccess.Save) == AssemblyBuilderAccess.Save)
                {
                    _builder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, _assemblyAccess, _moduleDirectory);
                }
                else
                {
                    _builder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, _assemblyAccess);
                }

            }
        }
        #endregion

        #region Module Handling
        public EmissionModule CreateModule()
        {
            return CreateModule(new DesignatingScope());
        }

        public EmissionModule CreateModule(IDesignatingScope designatingScope)
        {
            if (designatingScope == null) throw new ArgumentNullException("designatingScope");
            return new EmissionModule(AcquireModule(), designatingScope, _assemblyName);
        }

        internal ModuleBuilder AcquireModule()
        {
            lock (_builderGuard)
            {
                EnsureAssembly();


                string moduleName = _moduleName + (_moduleCount++).ToString() + _moduleExtension;
                ModuleBuilder result;

                if ((_assemblyAccess & AssemblyBuilderAccess.Save) == AssemblyBuilderAccess.Save)
                {
                    result = _builder.DefineDynamicModule(moduleName, moduleName, false);
                }
                else
                {
                    result = _builder.DefineDynamicModule(moduleName, false);
                }

                return result;
            }
        }
        #endregion

        #region Save Handling
        public void SaveAssembly()
        {
            if ((_assemblyAccess & AssemblyBuilderAccess.Save) != AssemblyBuilderAccess.Save) return;

            lock (_builderGuard)
            {
                AssemblyBuilder assemblyBuilder;
                if (_moduleCount == 0) throw new InvalidOperationException("No assembly has been generated.");

                assemblyBuilder = _builder;
                assemblyBuilder.Save(_moduleName + (_moduleCount - 1).ToString() + _moduleExtension);
            }
        }
        #endregion

        #region Disposal Handling
        protected override void OnDispose()
        {
            lock (_builderGuard)
            {
                _builder = null;
            }
        }
        #endregion
    }
}
