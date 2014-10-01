
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
using System.Text;

namespace Tamir.SharpSsh.java.lang
{
	/// <summary>
	/// Summary description for StringBuffer.
	/// </summary>
	public class StringBuffer
	{
		StringBuilder sb;
		public StringBuffer()
		{
			sb = new StringBuilder();
		}

		public StringBuffer(string s)
		{
			sb = new StringBuilder(s);
		}

		public StringBuffer(StringBuilder sb):this(sb.ToString())
		{
		}

		public StringBuffer(Tamir.SharpSsh.java.String s):this(s.ToString())
		{
		}

		public StringBuffer append(string s)
		{
			sb.Append(s);
			return this;
		}
		
		public StringBuffer append(char s)
		{
			sb.Append(s);
			return this;
		}

		public StringBuffer append(Tamir.SharpSsh.java.String s)
		{
			return append(s.ToString());
		}

		public StringBuffer delete(int start, int end) 
		{
			sb.Remove(start, end-start);
			return this;
		}

		public override string ToString()
		{
			return sb.ToString();
		}

		public string toString()
		{
			return ToString();
		}

	}
}
