
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

namespace Tamir.SharpSsh.java.util
{
	/// <summary>
	/// Summary description for Arrays.
	/// </summary>
	public class Arrays
	{
		internal static bool equals(byte[] foo, byte[] bar)
		{
			int i=foo.Length;
			if(i!=bar.Length) return false;
			for(int j=0; j<i; j++){ if(foo[j]!=bar[j]) return false; }
			//try{while(true){i--; if(foo[i]!=bar[i])return false;}}catch(Exception e){}
			return true;
		}
	}
}
