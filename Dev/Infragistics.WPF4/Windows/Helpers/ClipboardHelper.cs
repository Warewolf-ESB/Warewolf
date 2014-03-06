using System;
using System.Collections.Generic;
using System.Text;
using Wpf = System.Windows;
using Win = System.Windows.Forms;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Security;

namespace Infragistics.Windows.Helpers
{
    // AS 3/18/09 TFS15623
    // According to the documentation, plain text clipboard operations are allowed 
    // but they seem to generate security exceptions as  well. We can fall back to 
    // using the winforms clipboard so I created a helper class we can use.
    //
    // AS 4/7/09 Clipboard Support
    /// <summary>
    /// Static class with helper methods for interacting with the clipboard.
    /// </summary>
    [InfragisticsFeatureAttribute(Version=FeatureInfo.Version_9_2, FeatureName=FeatureInfo.FeatureName_ClipboardSupport)]
    public static class ClipboardHelper
    {
        #region Member Variables

        private static bool _wpfClipboardFailed;
        private static bool _winFormClipboardFailed;
		private static readonly bool _isClr4;

		private const int DefaultRetryCount = 10;
		private const int DefaultRetryDelay = 100;

        #endregion //Member Variables

		#region Constructor
		static ClipboardHelper()
		{
			_isClr4 = System.Environment.Version.Major >= 4;
		} 
		#endregion //Constructor

        #region Public methods

        #region Clear
        /// <summary>
        /// Removes all data from the clipboard.
        /// </summary>
        public static void Clear()
        {
            if (!_wpfClipboardFailed)
            {
                try
                {






			           Wpf.Clipboard.Clear();
                    return;
                }
                catch (System.Security.SecurityException)
                {
                    _wpfClipboardFailed = true;
                }
            }

            ClearWinFormClipboard();
        }
        #endregion //Clear

        #region CreateDataObject
        /// <summary>
        /// Helper method to create a new instance of an <see cref="System.Windows.IDataObject"/>
        /// </summary>
        /// <returns>A <see cref="System.Windows.IDataObject"/> implementation or null if one could not be created.</returns>
        public static Wpf.IDataObject CreateDataObject()
        {
            if (!_wpfClipboardFailed)
            {
                try
                {
                    return new Wpf.DataObject();
                }
                catch (System.Security.SecurityException)
                {
                    _wpfClipboardFailed = true;
                }
            }

            return CreateWinFormDataObject();
        } 
        #endregion //CreateDataObject

        #region GetDataObject
        /// <summary>
        /// Returns a data object that represents the contents of the clipboard.
        /// </summary>
        /// <returns>A data object that provides access to the entire contents of the clipboard or null if there is no data on the clipboard.</returns>
        public static Wpf.IDataObject GetDataObject()
        {
			// AS 12/2/09 TFS25338
			//if (!_wpfClipboardFailed)
			//{
			//    try
			//    {
			//        return Wpf.Clipboard.GetDataObject();
			//    }
			//    catch (System.Security.SecurityException)
			//    {
			//        _wpfClipboardFailed = true;
			//    }
			//}
			//
			//return GetWinFormDataObject();
			Wpf.IDataObject data;

			if (!GetDataObject(DefaultRetryCount, DefaultRetryDelay, out data))
				data = GetWinFormDataObject();

			return data;
        }

		// AS 12/2/09 TFS25338
		// Added a private overload that would attempt to retry accessing the 
		// clipboard a specified number of times. Like the winforms version we'll 
		// keep this private.
		//
		private static bool GetDataObject(int retryTimes, int retryDelay, out Wpf.IDataObject dataObject)
		{
			dataObject = null;

			if (retryTimes < 0)
				throw new ArgumentOutOfRangeException("retryTimes");

			if (retryDelay < 0)
				throw new ArgumentOutOfRangeException("retryDelay");

			// CLR4 already has logic to retry 10 times
			if (_isClr4)
			{
				Debug.Assert(retryTimes == 10 && retryDelay == 100, "Non standard delay?");
				retryTimes = 0;
			}

			if (!_wpfClipboardFailed)
			{
				for (int i = retryTimes; i >= 0; i--)
				{
					try
					{
						dataObject = Wpf.Clipboard.GetDataObject();
						return true;
					}
					catch (System.Security.SecurityException)
					{
						_wpfClipboardFailed = true;
						break;
					}
					catch (System.Runtime.InteropServices.ExternalException)
					{
						// when we run out of retry attempts and still fail
						// just rethrow the caught exception
						if (i == 0)
							throw;

						// otherwise wait and try again
						System.Threading.Thread.Sleep(retryDelay);
					}
				}
			}

			return false;
		}
        #endregion //GetDataObject

        #region GetText
        /// <summary>
        /// Returns a string containing the UnicodeText data on the clipboard.
        /// </summary>
        /// <returns>The unicode text on the clipboard or an empty string.</returns>
        public static string GetText()
        {
            return GetText(Wpf.TextDataFormat.UnicodeText);
        }

        /// <summary>
        /// Returns a string for the specified text format on the clipboard.
        /// </summary>
        /// <param name="format">The text formation to retreive</param>
        /// <returns>The string for the specified format on the clipboard or an empty string.</returns>
        public static string GetText(Wpf.TextDataFormat format)
        {
            if (!_wpfClipboardFailed)
            {
                try
                {


#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

	                    return Wpf.Clipboard.GetText(format);
                }
                catch (System.Security.SecurityException)
                {
                    _wpfClipboardFailed = true;
                }
            }

            return GetWinFormText(format);
        }
        #endregion //GetText

        #region SetDataObject
        /// <summary>
        /// Places a data object on the clipboard.
        /// </summary>
        /// <param name="data">An IDataObject to place on the clipboard.</param>
        public static bool SetDataObject(object data)
        {
            return SetDataObject(data, false);
        }

        /// <summary>
        /// Places a data object on the clipboard and conditionally leaves it on the clipboard when the application exits.
        /// </summary>
        /// <param name="data">The data object to place on the clipboard</param>
        /// <param name="copy">True to leave the data on the system clipboard when the application exits.</param>
        public static bool SetDataObject(object data, bool copy)
        {
			// AS 12/2/09 TFS25338
			// Added retryTimes/Delay.
			//
			return SetDataObject(data, copy, DefaultRetryCount, DefaultRetryDelay);
		}

		// AS 12/2/09 TFS25338
		// Added overload that takes a number of retry times and the delay between each.
		//
		/// <summary>
		/// Places a data object on the clipboard and conditionally leaves it on the clipboard when the application exits.
		/// </summary>
		/// <param name="data">The data object to place on the clipboard</param>
		/// <param name="copy">True to leave the data on the system clipboard when the application exits.</param>
		/// <param name="retryTimes">The number of times to retry placing the data on the clipboard</param>
		/// <param name="retryDelay">The number of milliseconds to pause between attempts</param>
		/// <exception cref="ArgumentNullException">data is null</exception>
		/// <exception cref="ArgumentOutOfRangeException">retryTimes or retryDelay is less than zero.</exception>
		/// <exception cref="System.Runtime.InteropServices.ExternalException">Data could not be placed on the clipboard</exception>
		public static bool SetDataObject(object data, bool copy, int retryTimes, int retryDelay)
		{
            Utilities.ThrowIfNull(data, "data");

			// AS 12/2/09 TFS25338
			if (retryTimes < 0)
				throw new ArgumentOutOfRangeException("retryTimes");

			// AS 12/2/09 TFS25338
			if (retryDelay < 0)
				throw new ArgumentOutOfRangeException("retryDelay");

            if (!_wpfClipboardFailed)
            {
				int wpfRetryTimes = retryTimes;

				if (_isClr4)
				{
					// in clr4, wpf will retry 10 times with a 100ms delay between
					wpfRetryTimes = Math.Max((retryTimes - 1) / 10, 0);
				}

				// AS 12/2/09 TFS25338
				// Loop based on the # of times to retry.
				//
				for (int i = wpfRetryTimes; i >= 0; i--)
				{
					try
					{
						Wpf.Clipboard.SetDataObject(data, copy);
						return true;
					}
					catch (System.Security.SecurityException)
					{
						_wpfClipboardFailed = true;
						break;
					}
					// AS 12/2/09 TFS25338
					catch (System.Runtime.InteropServices.ExternalException)
					{
						// when we run out of retry attempts and still fail
						// just rethrow the caught exception
						if (i == 0)
							throw;

						// otherwise wait and try again
						System.Threading.Thread.Sleep(retryDelay);
					}
				}
            }

			return SetWinFormDataObject(data, copy,  retryTimes, retryDelay);
        }
        #endregion //SetDataObject

        #region SetText
        /// <summary>
        /// Stores the specified UnicodeText string on the clipboard.
        /// </summary>
        /// <param name="text">The string to store on the clipboard</param>
        public static bool SetText(string text)
        {
            return SetText(text, Wpf.TextDataFormat.UnicodeText);
        }

        /// <summary>
        /// Stores the specified UnicodeText string on the clipboard in the specified format.
        /// </summary>
        /// <param name="text">The string to store on the clipboard</param>
        /// <param name="format">Indicates the format with which to store the text</param>
        public static bool SetText(string text, Wpf.TextDataFormat format)
        {
            VerifyNotNull(text, "text");

            if (!_wpfClipboardFailed)
            {
                try
                {


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	                    Wpf.Clipboard.SetText(text, format);
                    return true;
                }
                catch (System.Security.SecurityException)
                {
                    _wpfClipboardFailed = true;
                }
            }

            return SetWinFormText(text, format);
        } 
        #endregion //SetText

        #endregion //Public methods

        #region Private methods

        #region ClearWinFormClipboard
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ClearWinFormClipboard()
        {
            if (!_winFormClipboardFailed)
            {
                try
                {
                    Win.Clipboard.Clear();
                }
                catch (SecurityException)
                {
                    _winFormClipboardFailed = true;
                }
            }
        } 
        #endregion //ClearWinFormClipboard 

        #region CreateWinFormDataObject
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Wpf.IDataObject CreateWinFormDataObject()
        {
            Wpf.IDataObject obj = null;

            if (!_winFormClipboardFailed)
            {
                try
                {
                    obj = new WinToWpfDataObject(new Win.DataObject());
                }
                catch (SecurityException)
                {
                    _winFormClipboardFailed = true;
                }
            }

            return obj;
        }
        #endregion //CreateWinFormDataObject

        #region GetDataFormat
        private static string GetDataFormat(Wpf.TextDataFormat format)
        {
            switch (format)
            {
                case Wpf.TextDataFormat.UnicodeText:
                    return Wpf.DataFormats.UnicodeText;
                case Wpf.TextDataFormat.CommaSeparatedValue:
                    return Wpf.DataFormats.CommaSeparatedValue;
                case Wpf.TextDataFormat.Html:
                    return Wpf.DataFormats.Html;
                case Wpf.TextDataFormat.Rtf:
                    return Wpf.DataFormats.Rtf;
                case Wpf.TextDataFormat.Text:
                    return Wpf.DataFormats.Text;
                case Wpf.TextDataFormat.Xaml:
                    return Wpf.DataFormats.Xaml;
            }

            return Wpf.DataFormats.UnicodeText;
        }
        #endregion //GetDataFormat

        #region GetWinFormDataObject
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Wpf.IDataObject GetWinFormDataObject()
        {
            Wpf.IDataObject obj = null;

            if (!_winFormClipboardFailed)
            {
                try
                {
                    Win.IDataObject dataObject = Win.Clipboard.GetDataObject();

                    if (null != dataObject)
                        obj = new WinToWpfDataObject(dataObject);
                }
                catch (SecurityException)
                {
                    _winFormClipboardFailed = true;
                }
            }

            return obj;
        } 
        #endregion //GetWinFormDataObject

        #region GetWinFormData
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static object GetWinFormData(string format)
        {
            object data = null;

            if (!_winFormClipboardFailed)
            {
                try
                {
                    Win.IDataObject dataObject = Win.Clipboard.GetDataObject();

                    if (null != dataObject)
                    {
                        bool autoConvert = ShouldAutoConvert(format);
                        data = dataObject.GetData(format, autoConvert);
                    }
                }
                catch (SecurityException)
                {
                    _winFormClipboardFailed = true;
                }
            }

            return data;
        }
        #endregion //GetWinFormData

        #region GetWinFormText
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string GetWinFormText(Wpf.TextDataFormat format)
        {
            string strFormat = GetDataFormat(format);
            return (string)GetWinFormData(strFormat) ?? string.Empty;
        }
        #endregion //GetWinFormText

		#region RetryOperation


#region Infragistics Source Cleanup (Region)















































#endregion // Infragistics Source Cleanup (Region)

		#endregion //RetryOperation

        #region SetWinFormText
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool SetWinFormText(string text, Wpf.TextDataFormat format)
        {
            string strFormat = GetDataFormat(format);
            return SetWinFormData(strFormat, text);
        } 
        #endregion //SetWinFormText

        #region SetWinFormData
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool SetWinFormData(string format, object data)
        {
            if (!_winFormClipboardFailed)
            {
                try
                {
                    Win.DataObject dataObject = new Win.DataObject();
                    bool convert = ShouldAutoConvert(format);
                    dataObject.SetData(format, convert, data);

                    Win.Clipboard.SetDataObject(dataObject, true);

                    return true;
                }
                catch (SecurityException)
                {
                    _winFormClipboardFailed = true;
                }
            }

            return false;
        } 
        #endregion //SetWinFormData

        #region SetWinFormDataObject
        [MethodImpl(MethodImplOptions.NoInlining)]
		private static bool SetWinFormDataObject(object data, bool copy,  int retryTimes, int retryDelay)
        {
            if (!_winFormClipboardFailed)
            {
                try
                {
                    Wpf.IDataObject wpfDataObject = data as Wpf.IDataObject;

                    if (null != wpfDataObject)
                        data = new WpfToWinDataObject(wpfDataObject);

					Win.Clipboard.SetDataObject(data, copy,  retryTimes, retryDelay);
                    return true;
                }
                catch (SecurityException)
                {
                    _winFormClipboardFailed = true;
                }
            }

            return false;
        } 
        #endregion //SetWinFormDataObject

        #region ShouldAutoConvert
        private static bool ShouldAutoConvert(string format)
        {
            return format == Wpf.DataFormats.FileDrop
                || format == Wpf.DataFormats.Bitmap
                ;
        }
        #endregion //ShouldAutoConvert

        #region VerifyNotNull
        private static void VerifyNotNull(object value, string name)
        {
            if (null == value)
                throw new ArgumentNullException(name);
        } 
        #endregion //VerifyNotNull

        #endregion //Private methods

        #region WinToWpfDataObject
        private class WinToWpfDataObject : Wpf.IDataObject
        {
            #region Member Variables

            private Win.IDataObject _winDataObject;

            #endregion //Member Variables

            #region Constructor
            internal WinToWpfDataObject(Win.IDataObject winDataObject)
            {
                Utilities.ThrowIfNull(winDataObject, "winDataObject");

                _winDataObject = winDataObject;
            }
            #endregion //Constructor

            #region IDataObject Members

            public object GetData(string format, bool autoConvert)
            {
                return _winDataObject.GetData(format, autoConvert);
            }

            public object GetData(Type format)
            {
                return _winDataObject.GetData(format);
            }

            public object GetData(string format)
            {
                return _winDataObject.GetData(format);
            }

            public bool GetDataPresent(string format, bool autoConvert)
            {
                return _winDataObject.GetDataPresent(format, autoConvert);
            }

            public bool GetDataPresent(Type format)
            {
                return _winDataObject.GetDataPresent(format);
            }

            public bool GetDataPresent(string format)
            {
                return _winDataObject.GetDataPresent(format);
            }

            public string[] GetFormats(bool autoConvert)
            {
                return _winDataObject.GetFormats(autoConvert);
            }

            public string[] GetFormats()
            {
                return _winDataObject.GetFormats();
            }

            public void SetData(string format, object data, bool autoConvert)
            {
                _winDataObject.SetData(format, autoConvert, data);
            }

            public void SetData(Type format, object data)
            {
                _winDataObject.SetData(format, data);
            }

            public void SetData(string format, object data)
            {
                _winDataObject.SetData(format, data);
            }

            public void SetData(object data)
            {
                _winDataObject.SetData(data);
            }

            #endregion
        }
        #endregion //WinToWpfDataObject

        #region WpfToWinDataObject
        private class WpfToWinDataObject : Win.IDataObject
        {
            #region Member Variables

            private Wpf.IDataObject _wpfDataObject;

            #endregion //Member Variables

            #region Constructor
            internal WpfToWinDataObject(Wpf.IDataObject wpfDataObject)
            {
                _wpfDataObject = wpfDataObject;
            }
            #endregion //Constructor

            #region IDataObject Members

            public object GetData(Type format)
            {
                return _wpfDataObject.GetData(format);
            }

            public object GetData(string format)
            {
                return _wpfDataObject.GetData(format);
            }

            public object GetData(string format, bool autoConvert)
            {
                return _wpfDataObject.GetData(format, autoConvert);
            }

            public bool GetDataPresent(Type format)
            {
                return _wpfDataObject.GetDataPresent(format);
            }

            public bool GetDataPresent(string format)
            {
                return _wpfDataObject.GetDataPresent(format);
            }

            public bool GetDataPresent(string format, bool autoConvert)
            {
                return _wpfDataObject.GetDataPresent(format, autoConvert);
            }

            public string[] GetFormats()
            {
                return _wpfDataObject.GetFormats();
            }

            public string[] GetFormats(bool autoConvert)
            {
                return _wpfDataObject.GetFormats(autoConvert);
            }

            public void SetData(object data)
            {
                _wpfDataObject.SetData(data);
            }

            public void SetData(Type format, object data)
            {
                _wpfDataObject.SetData(format, data);
            }

            public void SetData(string format, object data)
            {
                _wpfDataObject.SetData(format, data);
            }

            public void SetData(string format, bool autoConvert, object data)
            {
                _wpfDataObject.SetData(format, data, autoConvert);
            }

            #endregion
        } 
        #endregion //WpfToWinDataObject
    }
}

#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved