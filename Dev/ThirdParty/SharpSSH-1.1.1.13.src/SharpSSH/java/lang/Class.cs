
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

namespace Tamir.SharpSsh.java.lang
{
	/// <summary>
	/// Summary description for Class.
	/// </summary>
	public class Class
	{
		Type t;
		private Class(Type t)
		{
			this.t=t;
		}
		private Class(string typeName) : this(Type.GetType(typeName))
		{
		}
		public static Class forName(string name)
		{
			return new Class(name);
		}

		public object newInstance()
		{
			return Activator.CreateInstance(t);
		}
	}
}
