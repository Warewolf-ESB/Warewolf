
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
using System.Net;

namespace Tamir.SharpSsh.java.net
{
	/// <summary>
	/// Summary description for InetAddress.
	/// </summary>
	public class InetAddress
	{
		internal IPAddress addr;
		public InetAddress(string addr)
		{
			this.addr = IPAddress.Parse(addr);
		}
		public InetAddress(IPAddress addr)
		{
			this.addr = addr;
		}

		public bool isAnyLocalAddress()
		{
			return IPAddress.IsLoopback(addr);
		}

		public bool equals(InetAddress addr)
		{
			return addr.ToString().Equals( addr.ToString());
		}

		public bool equals(string addr)
		{
			return addr.ToString().Equals( addr.ToString());
		}

		public override string ToString()
		{
			return addr.ToString ();
		}

		public override bool Equals(object obj)
		{
			return equals (obj.ToString());
		}

		public string getHostAddress()
		{
			return ToString();
		}

		public override int GetHashCode()
		{
			return base.GetHashCode ();
		}

		public static InetAddress getByName(string name)
		{
			return new InetAddress( Dns.GetHostByName(name).AddressList[0] );
		}
	}
}
