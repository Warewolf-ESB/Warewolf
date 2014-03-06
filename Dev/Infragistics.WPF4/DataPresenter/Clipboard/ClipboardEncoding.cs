using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.IO;
using System.Windows;
using System.Globalization;

namespace Infragistics.Windows.DataPresenter
{
	#region ClipboardEncoding
	internal abstract class ClipboardEncoding
	{
		#region Constructor
		protected ClipboardEncoding()
		{
		}
		#endregion //Constructor

		#region Methods

		#region Public Methods

		/// <summary>
		/// Used to obtain the cell values from the clipboard data.
		/// </summary>
		/// <param name="format">The format for which the values are being requested.</param>
		/// <param name="dataObject">The clipboard dataobject from which the values should be obtained.</param>
		/// <returns>An array of values based on the values in the clipboard.</returns>
		public virtual CellValueHolder[,] GetCellValues(string format, IDataObject dataObject)
		{
			return null;
		}

		/// <summary>
		/// Provides the data that should be put into the clipboard for the specified cell values.
		/// </summary>
		/// <param name="cellValues">A collection of <see cref="CellValueHolder"/> instances whose values should be put on the clipboard. The values should be converted to display text already.</param>
		/// <param name="includeHeaders">A boolean indicating whether the values for the field labels should be included in the output.</param>
		/// <returns>Returns an object that provides a specific representation of that format for the specified values</returns>
		public abstract object GetClipboardData(ClipboardCellValueHolderCollection cellValues, bool includeHeaders);

		#endregion //Public Methods

		#region Protected Methods
		/// <summary>
		/// Returns the string representation of the label for the specified field.
		/// </summary>
		/// <param name="cvh">The value holder for the field whose label is to be returned.</param>
		/// <returns>The string representation of the specified CellValueHolder</returns>
		protected virtual string GetHeaderText(CellValueHolder cvh)
		{
			return ClipboardOperationInfo.GetText(cvh);
		}

		/// <summary>
		/// Returns the text representation of the specified value.
		/// </summary>
		/// <param name="cvh">The CellValueHolder whose value is to be returned as a string.</param>
		/// <returns>The string representation of the specified CellValueHolder</returns>
		protected virtual string GetCellText(CellValueHolder cvh)
		{
			return ClipboardOperationInfo.GetText(cvh);
		} 
		#endregion //Protected Methods

		#region Private methods

		#endregion //Private methods

		#endregion //Methods
	} 
	#endregion //ClipboardEncoding

	#region ClipboardEncodingFactory
	internal static class ClipboardEncodingFactory
	{
		// excel's csv/tsv do not delimit the text base on the presence of a quote. the 
		// standard (or as close as exists for it) for CSV (see http://tools.ietf.org/html/rfc4180)
		// indicates that you should delimit quotes within the values of a CSV. also excel
		// does not delimit text that contains leading/trailing spaces
		private static readonly char[] CSVCriteria = new char[] { ',', '"', '\r', '\n', ' ' };
		private static readonly char[] ExcelCSVCriteria = new char[] { ',', '\r', '\n' };

		private static readonly char[] TSVCriteria = new char[] { '\t', '"', '\r', '\n' };
		private static readonly char[] ExcelTSVCriteria = new char[] { '\t', '\r', '\n' };

		private static readonly char[] AlwaysUseDelimiter = new char[0];

		// excel cannot read csv put into the clipboard unless it is in UTF8 format
		private static readonly Encoding TsvEncoding = null;
		private static readonly Encoding CsvEncoding = Encoding.UTF8;

		public static ClipboardEncoding CreateEncoding(DataPresenterBase dp, IDataObject dataObject, string format)
		{
			if (format == DataFormats.Html)
			{
				return new HtmlClipboardEncoding();
			}
			else if (format == DataFormats.CommaSeparatedValue)
			{
				if (IsExcelClipboardData(dataObject))
					return new PlainTextClipboardEncoding(Environment.NewLine, ",", "\"", ExcelCSVCriteria, "\"", CsvEncoding);

				return new PlainTextClipboardEncoding(Environment.NewLine, ",", "\"", CSVCriteria, "\"", CsvEncoding);
			}
			else if (format == DataFormats.Text
				|| format == DataFormats.UnicodeText
				|| format == DataFormats.StringFormat)
			{
				// if no settings have been applied then we will provide true tsv formatting
				// 
				if (IsDefault(dp, DataPresenterBase.ClipboardRecordSeparatorProperty) &&
					IsDefault(dp, DataPresenterBase.ClipboardCellDelimiterProperty) &&
					IsDefault(dp, DataPresenterBase.ClipboardCellSeparatorProperty))
				{
					// AS 10/20/10 TFS57478
					//if (IsExcelClipboardData(dataObject))
					//    return new PlainTextClipboardEncoding(Environment.NewLine, "\t", "\"", ExcelTSVCriteria, "\"", TsvEncoding);
					//
					//return new PlainTextClipboardEncoding(Environment.NewLine, "\t", "\"", TSVCriteria, "\"", TsvEncoding);
					return new PlainTextClipboardEncoding(Environment.NewLine, "\t", "\"", ExcelTSVCriteria, "\"", TsvEncoding);
				}
				else
				{
					// otherwise we will do what wingrid did which was to always use the delimiters 
					// and separators that were specified
					return new PlainTextClipboardEncoding(
						dp.ClipboardRecordSeparator, 
						dp.ClipboardCellSeparator, 
						dp.ClipboardCellDelimiter,
						AlwaysUseDelimiter, null, TsvEncoding);
				}
			}
			else if (format == ClipboardOperationInfo.GetClipboardDataFormat(typeof(ClipboardData)))
			{
				return new DataPresenterClipboardEncoding();
			}

			Debug.Fail("Unexpected format:" + format);
			return null;
		}

		#region IsDefault
		private static bool IsDefault(DependencyObject d, DependencyProperty property)
		{
			return null != d && DependencyPropertyHelper.GetValueSource(d, property).BaseValueSource == BaseValueSource.Default;
		} 
		#endregion //IsDefault

		#region IsExcelClipboardData
		private static bool IsExcelClipboardData(IDataObject dataObject)
		{
			return null != dataObject &&
				dataObject.GetDataPresent("BIFF8") &&
				dataObject.GetDataPresent("BIFF5");
		}
		#endregion //IsExcelClipboardData
	} 
	#endregion //ClipboardEncodingFactory

	#region HtmlClipboardEncoding
	/// <summary>
	/// Custom encoding class for providing cell values as Html
	/// </summary>
	internal class HtmlClipboardEncoding : ClipboardEncoding
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="HtmlClipboardEncoding"/>
		/// </summary>
		public HtmlClipboardEncoding()
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region Equals
		public override bool Equals(object obj)
		{
			return null != obj && obj.GetType() == this.GetType();
		}
		#endregion //Equals

		#region GetClipboardData
		public override object GetClipboardData(ClipboardCellValueHolderCollection cellValues, bool includeHeaders)
		{
			return GetHtmlText(cellValues, includeHeaders);
		}
		#endregion //GetClipboardData

		#region GetHashCode
		public override int GetHashCode()
		{
			return this.GetType().GetHashCode();
		}
		#endregion //GetHashCode

		#endregion //Base class overrides

		#region Methods

		#region GetHtmlText

		[MethodImpl(MethodImplOptions.NoInlining)]
		internal Stream GetHtmlText(ClipboardCellValueHolderCollection values, bool includeHeaders)
		{
			// AS 1/6/06 BR08650
			// The CF_HTML format is supposed to be encoded using UTF-8.
			// http://msdn.microsoft.com/library/default.asp?url=/workshop/networking/clipboard/htmlclipboard.asp
			// http://msdn.microsoft.com/en-us/library/ms649015(VS.85).aspx
			// 
			Encoding encoding = System.Text.Encoding.UTF8;

			StringBuilder sb = new StringBuilder();

			// Create html clipboard header mask we can use with the string.Format to
			// create the actual header.
			// 
			string NewLine = Environment.NewLine;

			string headerMask = "Version:1.0{4}StartHTML:{0:0000000000}"
				+ "{4}EndHTML:{1:0000000000}"
				+ "{4}StartFragment:{2:0000000000}"
				+ "{4}EndFragment:{3:0000000000}{4}";

			// For efficiency purposes, create a stub that's going to be the same length
			// as the final actual header. This way the StringBuilder.Replace doesn't have
			// to move memory.
			// 
			string headerStub = string.Format(headerMask, 0, 0, 0, 0, NewLine);

			sb.Append(headerStub);

			int iiStartHTML = encoding.GetByteCount(sb.ToString());

			sb.AppendFormat("<html>{1}<head>{1}<meta http-equiv=\"Content-Type\" content=\"text/html;charset={0}\" >{1}</head>{1}", encoding.WebName, NewLine);
			sb.Append("<body>").Append(NewLine);

			int iiStartFragment = encoding.GetByteCount(sb.ToString());

			sb.Append("<!--StartFragment-->").Append(NewLine);

			sb.Append("<table>").Append(NewLine);

			#region Headers
			if (includeHeaders)
			{
				sb.Append(" <tr>").Append(NewLine);

				for (int c = 0, count = values.FieldCount; c < count; c++)
				{
					CellValueHolder cvh = values.GetLabel(c);
					string text = this.GetHeaderText(cvh);

					text = ProcessHtmlValue(text);

					sb.AppendFormat("  <th>{0}</th>", text).Append(NewLine);
				}

				sb.Append(" </tr>").Append(NewLine);
			}
			#endregion //Headers

			#region Cells
			for (int r = 0, rCount = values.RecordCount; r < rCount; r++)
			{
				sb.Append(" <tr>").Append(NewLine);

				for (int c = 0, cCount = values.FieldCount; c < cCount; c++)
				{
					// NOTE: The values should already have been converted to display text.
					string text = this.GetCellText(values[r, c]);

					text = ProcessHtmlValue(text);

					sb.AppendFormat("  <td>{0}</td>", text).Append(NewLine);
				}

				sb.Append(" </tr>").Append(NewLine);
			}
			#endregion //Cells

			sb.Append("</table>").Append(NewLine);

			sb.Append("<!--EndFragment-->");

			int iiEndFragment = encoding.GetByteCount(sb.ToString());

			sb.Append(NewLine);
			sb.Append("</body></html>").Append(NewLine);

			int iiEndHTML = encoding.GetByteCount(sb.ToString());

			// Create the actual clipboard header based on the actual offsets.
			// 
			string actualHeader = string.Format(headerMask, iiStartHTML, iiEndHTML, iiStartFragment, iiEndFragment, NewLine);

			Debug.Assert(actualHeader.Length == headerStub.Length, "The lengths should be the same.");

			// Replace the stub header with the actual header.
			// 
			sb.Replace(headerStub, actualHeader, 0, headerStub.Length);

			string html = sb.ToString();

			return new System.IO.MemoryStream(encoding.GetBytes(html));
		}

		#endregion // GetHtmlText

		#region HtmlEncode
		private static string HtmlEncode(string text)
		{
			// AS 8/26/09
			// Removed dependency on System.Web assembly.
			//
			//return System.Web.HttpUtility.HtmlEncode(text);
			if (string.IsNullOrEmpty(text))
				return text;

			StringBuilder sb = null;

			for (int i = 0; i < text.Length; i++)
			{
				char ch = text[i];

				string replacement = null;

				if (ch <= '>')
				{
					switch (ch)
					{
						case '<':
							replacement = "&lt;";
							break;
						case '>':
							replacement = "&gt;";
							break;
						case '&':
							replacement = "&amp;";
							break;
						case '"':
							replacement = "&quot;";
							break;
					}
				}
				else if (ch >= 160 && ch < 256)
				{
					replacement = string.Format("&#{0};", ((int)ch).ToString(CultureInfo.InvariantCulture));
				}

				if (replacement == null)
				{
					if (sb == null)
						continue;
					else
						sb.Append(ch);
				}
				else
				{
					if (null == sb)
					{
						sb = new StringBuilder();

						// insert everything before this point
						if (i > 0)
							sb.Append(text.Substring(0, i));
					}

					sb.Append(replacement);
				}
			}

			string actual = null == sb ? text : sb.ToString();

			return actual;
		}
		#endregion //HtmlEncode

		#region ProcessHtmlValue
		private static string ProcessHtmlValue(string text)
		{
			if (null != text)
			{
				text = HtmlEncode(text);

				// in wingrid we used to always change the label to a 
				// single line but for html this isn't necessary so
				// we'll try to maintain it just as excel does
				text = text.Replace(Environment.NewLine, "<br>");
			}

			return text;
		}
		#endregion //ProcessHtmlValue

		#endregion //Methods
	} 
	#endregion //HtmlClipboardEncoding

	#region PlainTextClipboardEncoding
	/// <summary>
	/// Custom clipboard encoding for providing a string representation separated by the specified characters.
	/// </summary>
	internal class PlainTextClipboardEncoding : ClipboardEncoding
	{
		#region Member Variables

		private string _rowSeparator;
		private string _cellSeparator;
		private string _cellDelimiter;
		private char[] _delimiterCriteria;
		private string _delimiterEscape;
		private Encoding _encoding;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Custom clipboard encoding that will create separate values
		/// </summary>
		/// <param name="rowSeparator">The string used to separate different rows of data</param>
		/// <param name="cellSeparator">The string used between cell values</param>
		/// <param name="cellDelimiter">The string that should be used to wrap the cell separator</param>
		/// <param name="delimiterCriteria">A list of characters that must be present in order to use the <paramref name="cellDelimiter"/></param>
		/// <param name="delimiterEscape">The string that should be use to replace any instances of the <paramref name="cellDelimiter"/> if the delimiters are to be used.</param>
		/// <param name="encoding">Encoding to use when getting the clipboard data</param>
		public PlainTextClipboardEncoding(string rowSeparator, string cellSeparator, string cellDelimiter,
			char[] delimiterCriteria, string delimiterEscape, Encoding encoding)
		{
			_rowSeparator = rowSeparator ?? string.Empty;
			_cellDelimiter = cellDelimiter ?? string.Empty;
			_cellSeparator = cellSeparator ?? string.Empty;
			_delimiterCriteria = delimiterCriteria ?? new char[0];
			_delimiterEscape = (delimiterEscape ?? _cellDelimiter) + _cellDelimiter;
			_encoding = encoding;
		}
		#endregion //Constructor

		#region Base class overrides

		#region Equals
		public override bool Equals(object obj)
		{
			PlainTextClipboardEncoding other = obj as PlainTextClipboardEncoding;

			return null != other &&
				other._rowSeparator == _rowSeparator &&
				other._cellDelimiter == _cellDelimiter &&
				other._cellSeparator == _cellSeparator &&
				other._delimiterEscape == _delimiterEscape &&
				other._delimiterCriteria == _delimiterCriteria;
		} 
		#endregion //Equals

		#region GetCellValues
		public override CellValueHolder[,] GetCellValues(string format, IDataObject dataObject)
		{
			object actualData = dataObject.GetDataPresent(format) ? dataObject.GetData(format) : null;

			if (actualData == null)
				return null;

			Stream dataStream = actualData as Stream;
			string data = null;

			if (null != dataStream)
			{
				using (StreamReader sr = new StreamReader(dataStream))
					data = sr.ReadToEnd();
			}
			else
			{
				data = actualData as string;
			}

			if (string.IsNullOrEmpty(data))
				return null;

			CellValueHolder[,] values = null;

			if (null != data)
			{
				int position = 0;
				List<string[]> rows = new List<string[]>();
				string[] cells;
				int columnCount = 0;

				while (this.ReadLine(data, ref position, out cells))
				{
					if (cells == null)
						cells = new string[0];

					// keep a list of the rows until we're done
					rows.Add(cells);

					// keep track of the widest row since some rows may
					// have blanks
					columnCount = Math.Max(columnCount, cells.Length);
				}

				values = new CellValueHolder[rows.Count, columnCount];

				for (int r = 0, rCount = rows.Count; r < rCount; r++)
				{
					cells = rows[r];

					for (int c = 0; c < columnCount; c++)
					{
						string value;

						if (c < cells.Length)
							value = cells[c];
						else
							value = string.Empty;

						values[r, c] = new CellValueHolder(value, true);
					}
				}
			}

			return values;
		}
		#endregion //GetCellValues

		#region GetClipboardData
		public override object GetClipboardData(ClipboardCellValueHolderCollection cellValues, bool includeHeaders)
		{
			StringWriter writer = this.CreateWriter();

			string cellSeparator = _cellSeparator;
			string rowSeparator = _rowSeparator;

			if (includeHeaders)
			{
				for (int i = 0; i < cellValues.FieldCount; i++)
				{
					CellValueHolder cvh = cellValues.GetLabel(i);
					string text = this.GetHeaderText(cvh);

					if (i > 0)
						writer.Write(cellSeparator);

					text = this.FormatValue(text);

					writer.Write(text);
				}

				writer.Write(rowSeparator);
			}

			for (int r = 0, rCount = cellValues.RecordCount; r < rCount; r++)
			{
				for (int c = 0, cCount = cellValues.FieldCount; c < cCount; c++)
				{
					string text = this.GetCellText(cellValues[r, c]);

					if (c > 0)
						writer.Write(cellSeparator);

					text = this.FormatValue(text);

					writer.Write(text);
				}

				writer.Write(rowSeparator);
			}

			string strValue = writer.ToString();

			if (_encoding != null)
			{
				byte[] bytes = _encoding.GetBytes(strValue);
				return new MemoryStream(bytes);
			}

			return strValue;
		}
		#endregion //GetClipboardData

		#region GetHashCode
		public override int GetHashCode()
		{
			return _rowSeparator.GetHashCode() + _cellSeparator.GetHashCode() + _cellDelimiter.GetHashCode();
		} 
		#endregion //GetHashCode

		#endregion //Base class overrides

		#region Methods

		#region CreateWriter
		private StringWriter CreateWriter()
		{
			return new StringWriter();
		}
		#endregion //CreateWriter

		#region FormatValue
		private string FormatValue(string text)
		{
			if (_cellDelimiter.Length > 0)
			{
				bool needsDelimiter;

				// if there are no characters to match upon then we will always use 
				// the delimiter
				if (_delimiterCriteria.Length == 0)
					needsDelimiter = true;
				else
				{
					// otherwise only if a character indicated is within the text
					needsDelimiter = text.IndexOfAny(_delimiterCriteria) >= 0;
				}

				if (needsDelimiter)
				{
					// if the text contains the delimiter replace it with the appropriate
					// string
					text = text.Replace(_cellDelimiter, _delimiterEscape);

					// then wrap the value in the delimiter
					text = _cellDelimiter + text + _cellDelimiter;
				}
			}

			return text;
		}
		#endregion //FormatValue

		#region ReadLine
		private bool ReadLine(string data, ref int position, out string[] cells)
		{
			int dataLen = data.Length;

			if (position >= 0 && position < data.Length)
			{
				List<string> strings = new List<string>();
				int rowSepLen = _rowSeparator.Length;

				// if there isn't enough room for the row separator then consider 
				// the remaining text as part of the line being processed.
				// also process the line if there is enough room and it doesn't 
				// start with the row separator. if it did then we would want to treat
				// it as an empty line
				if (!StartsWith(data, position, _rowSeparator))
				{
					if (_cellDelimiter.Length == 0)
					{
						#region No Delimiter

						// if there is no cell delimiter then just find the end of the
						// row and split the string based on the separator
						int lineEnd = data.IndexOf(_rowSeparator, position);
						string line;

						if (lineEnd < 0)
						{
							// if we don't find it then the rest of the text is that line
							line = data.Substring(position);
							position = dataLen;
						}
						else
						{
							// otherwise just use the substring up to that line end
							// and offset the position to just after the separator
							line = data.Substring(position, lineEnd - position);
							position = lineEnd + _rowSeparator.Length;
						}

						string[] lineParts = line.Split(new string[] { _cellSeparator }, StringSplitOptions.None);
						strings.AddRange(lineParts);

						#endregion //No Delimiter
					}
					else
					{
						while (position < dataLen)
						{
							bool isEndOfLine;
							string cell = ReadDelimitedCell(data, ref position, out isEndOfLine);

							// AS 8/20/09 TFS20919
							// If the last line is just a null string then ignore the line.
							//
							if (position == dataLen && cell == "\0" && strings.Count == 0)
							{
								cells = null;
								return false;
							}

							strings.Add(cell);

							if (isEndOfLine)
								break;
						}
					}
				}
				else
				{
					// shift over by the row separator
					position += rowSepLen;
				}

				cells = strings.ToArray();
				return true;
			}
			else
				cells = null;

			return position < data.Length;
		} 
		#endregion //ReadLine 

		#region ReadDelimitedCell
		private string ReadDelimitedCell(string data, ref int position, out bool isEndOfLine)
		{
			// otherwise start parsing the data...
			int delimiterLen = _cellDelimiter.Length;
			int cellSepLen = _cellSeparator.Length;
			int delimiterEscapeLen = _delimiterEscape.Length;
			int dataLen = data.Length;
			int rowSepLen = _rowSeparator.Length;

			// we'll need to track whether we could be within a delimiter
			bool? inDelimiter = null;

			// assume we're not at the end of the line until we hit a row separator
			isEndOfLine = false;

			// store the starting point
			int startPosition = position;

			// see if the cell starts with a delimiter
			if (position + delimiterLen <= dataLen)
			{
				if (!StartsWith(data, position, _cellDelimiter))
				{
					// if the start isn't the delimiter then it can't be
					inDelimiter = false;
				}
				else
				{
					// it does start with what appears to be a delimiter but it 
					// may not be one. for example, excel does not delimit a cell
					// that contains " in tsv/csv unless it contains a cr/lf/,/\t

					// in any case offset the position so we don't evaluate it again
					position += delimiterLen;

					// if we don't have these delimiter criteria that we would use
					// to always wrap with the delimiter then consider this to be
					// within a delimiter
					if (_delimiterCriteria == null)
					{
						// if we always put delimiters and we have one then consider
						// us to be in a delimited text
						inDelimiter = true;

					}
				}
			}

			while (position < data.Length)
			{
				// if we are (or could be) in a delimiter and we hit an escaped delimiter
				// then we'll consider ourselves in a delimiter and move after it
				if (inDelimiter != false && StartsWith(data, position, _delimiterEscape))
				{
					inDelimiter = true;
					position += delimiterEscapeLen;
					continue;
				}

				// if we don't know yet if we're within a delimiter and we find a character 
				// for which we would have added delimiters then consider us to be in a 
				// delimiter. unfortunately this isn't fool proof. you can have values in 
				// excel that result in the same output. 
				// e.g.
				// A1 = "A
				// A2 = "
				// A3 = A<crlf>
				// If you select all 3 and copy to the clipboard, the result is indiscernable.
				// You get:
				// "A
				// "
				// "A
				// "
				// Excel really should have delimited the result of A1 and A2 but it doesn't.
				//
				if (inDelimiter == null &&
					Array.IndexOf(_delimiterCriteria, data[position]) >= 0)
				{
					inDelimiter = true;
					position++;
					continue;
				}

				// if we're not sure if we're within the delimiter and we hit the 
				// the cell separator then we've hit the end of this cell
				if (inDelimiter != true && StartsWith(data, position, _cellSeparator))
				{
					// get the starting from the beginning
					string cell = data.Substring(startPosition, position - startPosition);

					// move after the cell separator
					position += cellSepLen;

					return UnescapeDelimiter(cell, inDelimiter);
				}

				// if we're not within a delimiter and we encounter the row separator
				// then we've hit the end of the line. add the string thus far and exit
				if (inDelimiter != true && StartsWith(data, position, _rowSeparator))
				{
					// get the cell value from the beginning
					string cell = data.Substring(startPosition, position - startPosition);

					// we're finished with this line
					isEndOfLine = true;

					// move after the row separator for the next row
					position += rowSepLen;

					return UnescapeDelimiter(cell, inDelimiter);
				}

				// if we're in a delimiter (or could be) and we hit a cell delimiter...
				if (inDelimiter != false && StartsWith(data, position, _cellDelimiter))
				{
					// if its followed by a cell separator then assume the cell is done
					if (StartsWith(data, position + delimiterLen, _cellSeparator))
					{
						startPosition += delimiterLen;
						string cell = data.Substring(startPosition, position - startPosition);

						// move after the delimiter and separator
						position += delimiterLen + cellSepLen;
						return UnescapeDelimiter(cell, inDelimiter);
					}

					// if its followed by a row separator then assume the row is done
					if (StartsWith(data, position + delimiterLen, _rowSeparator))
					{
						// if we found that we needed a delimiter then remove 
						// the starting delimiter but otherwise leave them in
						if (inDelimiter == true)
							startPosition += delimiterLen;
						else
							position += delimiterLen;

						string cell = data.Substring(startPosition, position - startPosition);

						// move after the delimiter and separator
						if (inDelimiter == true)
							position += delimiterLen;
						
						// in either case move after the crlf
						position += rowSepLen;

						isEndOfLine = true;
						return UnescapeDelimiter(cell, inDelimiter);
					}

					Debug.Fail("This wasn't an escaped delimiter but its not followed by a row/cell separator");
					position += delimiterLen;
					continue;
				}

				// move to the next character
				position++;
			}

			Debug.Assert(dataLen <= position);
			return data.Substring(startPosition, dataLen - startPosition);
		} 
		#endregion //ReadDelimitedCell

		#region StartsWith
		private static bool StartsWith(string data, int index, string comparison)
		{
			int compLen = comparison.Length;

			if (index + compLen <= data.Length)
			{
				if (compLen == 0)
					return false;

				if (compLen == 1)
					return data[index] == comparison[0];

				return data.IndexOf(comparison, index, compLen) == index;
			}

			return false;
		} 
		#endregion //StartsWith

		#region UnescapeDelimiter
		private string UnescapeDelimiter(string cell, bool? inDelimiter)
		{
			if (inDelimiter == true && !string.IsNullOrEmpty(cell))
			{
				cell = cell.Replace(_delimiterEscape, _cellDelimiter);
			}

			return cell;
		}
		#endregion //UnescapeDelimiter

		#endregion //Methods
	}
	#endregion //PlainTextClipboardEncoding

	#region DataPresenterClipboardEncoding
	internal class DataPresenterClipboardEncoding : ClipboardEncoding
	{
		#region Constructor
		internal DataPresenterClipboardEncoding()
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region Equals
		public override bool Equals(object obj)
		{
			return null != obj && obj.GetType() == this.GetType();
		}
		#endregion //Equals

		#region GetCellValues
		public override CellValueHolder[,] GetCellValues(string format, IDataObject dataObject)
		{
			CellValueHolder[,] values = null;

			if (dataObject.GetDataPresent(format))
			{
				ClipboardData data = dataObject.GetData(format) as ClipboardData;

				if (null != data)
				{
					values = data.GetCellValues();
				}
			}

			return values;
		} 
		#endregion //GetCellValues

		#region GetClipboardData
		public override object GetClipboardData(ClipboardCellValueHolderCollection cellValues, bool includeHeaders)
		{
			return ClipboardData.Create(cellValues);
		}
		#endregion //GetClipboardData

		#region GetHashCode
		public override int GetHashCode()
		{
			return this.GetType().GetHashCode();
		}
		#endregion //GetHashCode

		#endregion //Base class overrides
	} 
	#endregion //DataPresenterClipboardEncoding
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