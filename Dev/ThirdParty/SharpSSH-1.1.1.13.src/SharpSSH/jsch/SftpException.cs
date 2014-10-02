
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

namespace Tamir.SharpSsh.jsch
{
	public class SftpException : java.Exception
	{
		public int id;
		public String message;
		public SftpException (int id, String message):base() 
		{
			this.id=id;
			this.message=message;
		}
		public override String toString()
		{
			return message;
		}
	}
}
