
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
using IO = System.IO;

namespace Tamir.SharpSsh.java.io
{
	/// <summary>
	/// Summary description for FileInputStream.
	/// </summary>
	public class FileOutputStream : OutputStream
	{
		IO.FileStream fs;
		public FileOutputStream(string file):this(file, false)
		{
		}

		public FileOutputStream(File file):this(file.info.Name, false)
		{
		}

		public FileOutputStream(string file, bool append)
		{
			if(append)
				fs = new IO.FileStream(file, IO.FileMode.Append); // append
			else
				fs = new IO.FileStream(file, IO.FileMode.Create);
		}

		public FileOutputStream(File file, bool append):this(file.info.Name)
		{
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			fs.Write(buffer, offset, count);
		}

		public override void Flush()
		{
			fs.Flush();
		}

		public override void Close()
		{
			fs.Close();
		}

		public override bool CanSeek
		{
			get
			{
				return fs.CanSeek;
			}
		}

		public override long Seek(long offset, IO.SeekOrigin origin)
		{
			return fs.Seek(offset, origin);
		}
	}
}
