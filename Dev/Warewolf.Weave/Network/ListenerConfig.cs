// Decompiled with JetBrains decompiler
// Type: System.Network.ListenerConfig
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Runtime.CompilerServices;
using System.Text;

namespace System.Network
{
	public struct ListenerConfig
	{
		private string _address;
		private int _port;
		private int _backlog;

		public string Address => this._address;

		public int Port => this._port;

		public int Backlog => this._backlog;

		public ListenerConfig(string address, int port, int backlog)
		{
			this._address = address;
			this._port = port;
			this._backlog = backlog;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(this._address);
			sb.Append(", ");
			sb.Append(this._port);
			return sb.ToString();
		}
	}
}
