
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
using System.Runtime.InteropServices;

namespace CubicOrange.Windows.Forms.ActiveDirectory
{

	/// <summary>
	/// The object picker dialog box.
	/// </summary>
	[ComImport, Guid("17D6CCD8-3B7B-11D2-B9E0-00C04FD8DBF7")]
	internal class DSObjectPicker
	{
	}

	/// <summary>
	/// The IDsObjectPicker interface is used by an application to initialize and display an object picker dialog box. 
	/// </summary>
	[ComImport, Guid("0C87E64E-3B7A-11D2-B9E0-00C04FD8DBF7"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDsObjectPicker
	{
        [PreserveSig()]
        int Initialize(ref DSOP_INIT_INFO pInitInfo);
        [PreserveSig()]
		int InvokeDialog(IntPtr HWND, out IDataObject lpDataObject);
	}

	/// <summary>
	/// Interface to enable data transfers
	/// </summary>
	[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	Guid("0000010e-0000-0000-C000-000000000046")]
    internal interface IDataObject
	{
		[PreserveSig()]
		int GetData(ref FORMATETC pFormatEtc, ref STGMEDIUM b);
		void GetDataHere(ref FORMATETC pFormatEtc, ref STGMEDIUM b);
		[PreserveSig()]
		int QueryGetData(IntPtr a);
		[PreserveSig()]
		int GetCanonicalFormatEtc(IntPtr a, IntPtr b);
		[PreserveSig()]
		int SetData(IntPtr a, IntPtr b, int c);
		[PreserveSig()]
		int EnumFormatEtc(uint a, IntPtr b);
		[PreserveSig()]
		int DAdvise(IntPtr a, uint b, IntPtr c, ref uint d);
		[PreserveSig()]
		int DUnadvise(uint a);
		[PreserveSig()]
		int EnumDAdvise(IntPtr a);
	}


}
