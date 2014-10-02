
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
using System.Net.Sockets;

namespace Tamir.SharpSsh.java.net
{
	/// <summary>
	/// Summary description for ServerSocket.
	/// </summary>
	public class ServerSocket : TcpListener
	{
		public ServerSocket(int port, int arg, InetAddress addr) : base(addr.addr, port)
		{
			this.Start();
		}

		public Tamir.SharpSsh.java.net.Socket accept()
		{
			return new Tamir.SharpSsh.java.net.Socket( this.AcceptSocket() );
		}

		public void close()
		{
			this.Stop();
		}
	}
}
