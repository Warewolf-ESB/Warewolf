
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
	/// Summary description for Enumeration.
	/// </summary>
	public class Enumeration
	{
		private IEnumerator e;
		private bool hasMore;
		public Enumeration(IEnumerator e)
		{
			this.e=e;
			hasMore = e.MoveNext();
		}

		public bool hasMoreElements()
		{
			return hasMore;
		}

		public object nextElement()
		{
			object o = e.Current;
			hasMore = e.MoveNext();
			return o;
		}
	}
}
