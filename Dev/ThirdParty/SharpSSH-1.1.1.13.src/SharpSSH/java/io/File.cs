
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
	/// Summary description for File.
	/// </summary>
	public class File
	{
		string file;
		internal FileInfo info;

		public File(string file)
		{
			this.file = file;
			info = new FileInfo(file);
		}

		public string getCanonicalPath()
		{
			return Path.GetFullPath(file);
		}

		public bool isDirectory()
		{
			return Directory.Exists(file);
		}

		public long Length()
		{
			return info.Length;
		}
		
		public long length()
		{
			return Length();
		}

		public bool isAbsolute()
		{
			return Path.IsPathRooted(file);
		}

		public java.String[] list()
		{
			string [] dirs = Directory.GetDirectories(file);
			string [] files = Directory.GetFiles(file);
			java.String[] _list = new java.String[dirs.Length+files.Length];
			System.arraycopy(dirs, 0, _list, 0, dirs.Length);
			System.arraycopy(files, 0, _list, dirs.Length, files.Length);
			return _list;
		}

		public static string separator
		{
			get
			{
				return Path.DirectorySeparatorChar.ToString();
			}
		}

		public static char separatorChar
		{
			get
			{
				return Path.DirectorySeparatorChar;
			}
		}
	}
}
