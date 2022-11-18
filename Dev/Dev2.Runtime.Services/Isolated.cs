#pragma warning disable
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
using System.Reflection;
using System.Runtime.Loader;


namespace Dev2.Runtime
{
    public sealed class Isolated<T> : IDisposable where T : MarshalByRefObject
    {
        AssemblyLoadContext _domain;
        readonly T _value;

        public Isolated()
        {
            _domain = new AssemblyLoadContext(name: "Isolated:" + Guid.NewGuid(), isCollectible: true);

            var type = typeof(T);
            Assembly loadContextAssembly = _domain.LoadFromAssemblyName(type.Assembly.GetName());
            _value = (T)loadContextAssembly.CreateInstance((type.FullName == null ? "" : type.FullName))!;
        }

        public T Value => _value;

        public void Dispose()
        {
            if (_domain != null)
            {
                _domain.Unload();

                _domain = null;
            }
        }
    }
}