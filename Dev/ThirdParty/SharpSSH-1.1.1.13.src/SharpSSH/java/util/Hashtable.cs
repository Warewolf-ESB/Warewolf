
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
using Collections = System.Collections;

namespace Tamir.SharpSsh.java.util
{
	/// <summary>
	/// Summary description for Hashtable.
	/// </summary>
	public class Hashtable
	{
		internal Collections.Hashtable h;

		public Hashtable()
		{
			h= new Collections.Hashtable();
		}
		public Hashtable(Collections.Hashtable h)
		{
			this.h=h;
		}

		public void put(object key, object item)
		{
			h.Add(key, item);
		}

		public object get(object key)
		{
			return h[key];
		}

		public Enumeration keys()
		{
			return new Enumeration( h.Keys.GetEnumerator() );
		}

		public object this[object key]
		{
			get{return get(key);}
			set{h[key]=value;}
		}
	}
}
