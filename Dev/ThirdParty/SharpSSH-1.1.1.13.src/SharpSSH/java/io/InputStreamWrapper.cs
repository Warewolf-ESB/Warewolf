
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Tamir.SharpSsh.java.io;

namespace Tamir.Streams
{
	/// <summary>
	/// Summary description for InputStreamWrapper.
	/// </summary>
	public class InputStreamWrapper : InputStream
	{
		System.IO.Stream s;
		public InputStreamWrapper(System.IO.Stream s)
		{
			this.s = s;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return s.Read(buffer, offset, count);
		}
	}
}
