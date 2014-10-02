
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
using System.Collections;

namespace Tamir.SharpSsh.java.util
{
	/// <summary>
	/// Summary description for Vector.
	/// </summary>
	public class Vector : ArrayList
	{
		public int size()
		{
			return this.Count;
		}

		public void addElement(object o)
		{
			this.Add(o);
		}

		public void add(object o)
		{
			addElement(o);
		}

		public void removeElement(object o)
		{
			this.Remove(o);
		}

		public bool remove(object o)
		{
			this.Remove(o);
			return true;
		}

		public object elementAt(int i)
		{
			return this[i];
		}

		public object get(int i)
		{
			return elementAt(i);;
		}

		public void clear()
		{
			this.Clear();
		}

		public string toString()
		{
			return ToString();
		}
	}
}
