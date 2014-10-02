
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
	public class FileInputStream : InputStream
	{
		IO.FileStream fs;
		public FileInputStream(string file)
		{
			fs = IO.File.OpenRead(file);
		}

		public FileInputStream(File file):this(file.info.Name)
		{
		}

		public override void Close()
		{
			fs.Close();
		}


		public override int Read(byte[] buffer, int offset, int count)
		{
			return fs.Read(buffer, offset, count);
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
