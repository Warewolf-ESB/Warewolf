
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Ex = System.Exception;

namespace Tamir.SharpSsh.java
{
	/// <summary>
	/// Summary description for Exception.
	/// </summary>
	public class Exception : Ex
	{
		public Exception() : base()
		{
		}
		public Exception(string msg) : base(msg)
		{
		}

		public virtual string toString()
		{
			return ToString();
		}
	}
}
