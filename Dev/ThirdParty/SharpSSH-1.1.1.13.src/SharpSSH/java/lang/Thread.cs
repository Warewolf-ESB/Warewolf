
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
using System.Threading;
using Threading = System.Threading;

namespace Tamir.SharpSsh.java.lang
{
	/// <summary>
	/// Summary description for Thread.
	/// </summary>
	public class Thread
	{
		Threading.Thread t;

		public Thread(Threading.Thread t)
		{
			this.t=t;
		}
		public Thread(ThreadStart ts):this(new Threading.Thread(ts))
		{
		}

		public Thread(Runnable r):this(new ThreadStart(r.run))
		{
		}

		public void setName(string name)
		{
			t.Name=name;
		}

		public void start()
		{
			t.Start();
		}

		public bool isAlive()
		{
			return t.IsAlive;
		}

		public void yield()
		{
		}

		public void interrupt()
		{
			try
			{
				t.Interrupt();
			}catch
			{
			}
		}

		public void notifyAll()
		{
			Monitor.PulseAll(this);
		}

		public static void Sleep(int t)
		{
			Threading.Thread.Sleep(t);
		}

		public static void sleep(int t)
		{
			Sleep(t);
		}

		public static Thread currentThread()
		{
			return new Thread( Threading.Thread.CurrentThread );
		}
	}
}
