
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
using System.IO;

namespace Tamir.SharpSsh.java.io
{
	/// <summary>
	/// Summary description for InputStream.
	/// </summary>
	public abstract class OutputStream : Stream
	{
		public override int Read(byte[] buffer, int offset, int count)
		{
			return 0;
		}

		public override int ReadByte()
		{
			return 0;
		}

		public virtual void write(byte[] buffer, int offset, int count)
		{
			Write(buffer, offset, count);
		}

		public virtual void close()
		{
			Close();
		}

		public virtual void flush()
		{
			Flush();
		}

		public override bool CanRead
		{
			get
			{
				return false;
			}
		}
		public override bool CanWrite
		{
			get
			{
				return true;
			}
		}
		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}
		public override void Flush()
		{
		}
		public override long Length
		{
			get
			{
				return 0;
			}
		}
		public override long Position
		{
			get
			{
				return 0;
			}
			set
			{				
			}
		}
		public override void SetLength(long value)
		{			
		}
		public override long Seek(long offset, SeekOrigin origin)
		{
			return 0;
		}
	}
}
