using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using PerCederberg.Grammatica.Parser;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Diagnostics;
using Infragistics.Calculations.Engine;

namespace Infragistics.Controls.Interactions.Primitives
{
	/// <summary>
	/// Provides information about a syntax error in a formula.
	/// </summary>
	public class SyntaxErrorInfo : DependencyObject
	{
		#region Member Variables

		private FormulaEditorDialog _editorDialog;
		private Exception _exception;
		private Regex _stringFormat2Arguments;

		#endregion  // Member Variables

		#region Constructor

		internal SyntaxErrorInfo(FormulaEditorDialog editorDialog, Exception exception)
		{
			_editorDialog = editorDialog;
			_exception = exception;
		}

		#endregion  // Constructor

		#region Methods

		#region CreateHyperlink

		private Hyperlink CreateHyperlink(string text, int line, int column)
		{
			Hyperlink hyperlink = new Hyperlink();
			hyperlink.Inlines.Add(FormulaEditorUtilities.CreateRun(string.Format(text, line, column)));

			FormulaEditorTextBox textBox = _editorDialog.TextBox;
			if (textBox != null)
				hyperlink.Click += (sender, e) => textBox.GotoPosition(line - 1, column - 1);

			return hyperlink;
		}

		#endregion  // CreateHyperlink

		#region CreateInlinesCollection

		private List<Inline> CreateInlinesCollection()
		{
			List<Inline> inlineCollection = new List<Inline>();

			string syntaxErrorLabel;
			string syntaxErrorLabelLink;
			int line;
			int column;

			ParseException parseException = _exception as ParseException;
			CalculationException calculationException = _exception as CalculationException;

			if (parseException != null)
			{
				syntaxErrorLabel = FormulaEditorUtilities.GetString("SyntaxErrorLabel");
				syntaxErrorLabelLink = FormulaEditorUtilities.GetString("SyntaxErrorLabelLink");
				line = parseException.GetLine();
				column = parseException.GetColumn();
			}
			else if (calculationException != null && calculationException.HasDynamicMessage)
			{
				syntaxErrorLabel = calculationException.MessageRaw;
				syntaxErrorLabelLink = calculationException.ErrorLocationRaw;
				line = calculationException.Line;
				column = calculationException.Column;
			}
			else
			{
				inlineCollection.Add(FormulaEditorUtilities.CreateRun(_exception.Message));
				return inlineCollection;
			}

			Match match = this.StringFormat2Arguments.Match(syntaxErrorLabel);
			if (match.Success == false)
			{
				inlineCollection.Add(FormulaEditorUtilities.CreateRun(_exception.Message));
				return inlineCollection;
			}

			SortedList<int, int> formatArguments = new SortedList<int, int>();

			Group argumentGroup = match.Groups["arg"];
			for (int i = 0; i < argumentGroup.Captures.Count; i++)
			{
				Capture capture = argumentGroup.Captures[i];
				Debug.Assert(capture.Length == 3, "Invalid string format argument length.");
				formatArguments.Add(capture.Index, int.Parse(capture.Value[1].ToString()));
			}

			int lastMarkerIndex = 0;
			foreach (KeyValuePair<int, int> formatArgument in formatArguments)
			{
				int length = (formatArgument.Key - lastMarkerIndex);

				if (length != 0)
					inlineCollection.Add(FormulaEditorUtilities.CreateRun(syntaxErrorLabel.Substring(lastMarkerIndex, length)));

				int argumentIndex = formatArgument.Value;

				bool replaceWithLocation = false;
				object replacementParam = null;

				if (parseException != null)
				{
					switch (argumentIndex)
					{
						case 0:
							replacementParam = parseException.GetErrorMessage();
							break;

 						case 1:
							replaceWithLocation = true;
							break;

						default:
							Debug.Assert(false, "Unknown argument index.");
							break;
					}
				}
				else if (calculationException != null)
				{
					replacementParam = calculationException.MessageParams[argumentIndex];
					replaceWithLocation = (replacementParam == CalculationException.LocationPlaceholder);
				}
				else
				{
					Debug.Assert(false, "Unknown exception type.");
					continue;
				}

				if (replaceWithLocation)
				{
					inlineCollection.Add(this.CreateHyperlink(syntaxErrorLabelLink, line, column));
				}
				else
				{
					string replacementParamStr = null;
					if (replacementParam != null)
						replacementParamStr = replacementParam.ToString();

					inlineCollection.Add(FormulaEditorUtilities.CreateRun(replacementParamStr));
				}

				lastMarkerIndex = formatArgument.Key + 3;
			}

			int extraLength = syntaxErrorLabel.Length - lastMarkerIndex;
			if (extraLength != 0)
				inlineCollection.Add(FormulaEditorUtilities.CreateRun(syntaxErrorLabel.Substring(lastMarkerIndex, extraLength)));

			return inlineCollection;
		}

		#endregion  // CreateInlinesCollection

		#endregion  // Methods

		#region Properties

		#region Public Properties

		#region DisplayedError

		/// <summary>
		/// Identifies the DisplayedError attached dependency property
		/// </summary>
		/// <seealso cref="GetDisplayedError"/>
		/// <seealso cref="SetDisplayedError"/>
		public static readonly DependencyProperty DisplayedErrorProperty = DependencyPropertyUtilities.RegisterAttached("DisplayedError",
			typeof(SyntaxErrorInfo), typeof(SyntaxErrorInfo),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnDisplayedErrorChanged))
			);

		private static void OnDisplayedErrorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RichTextBox rtb = d as RichTextBox;
			if (rtb == null)
				return;

			BlockCollection blockCollection;





			rtb.IsDocumentEnabled = true;

			FlowDocument document = new FlowDocument();
			rtb.Document = document;
			blockCollection = document.Blocks;


			Paragraph paragraph = new Paragraph();
			blockCollection.Add(paragraph);

			SyntaxErrorInfo syntaxErrorInfo = (SyntaxErrorInfo)e.NewValue;
			if (syntaxErrorInfo == null)
				return;

			List<Inline> formattingInlines = syntaxErrorInfo.CreateInlinesCollection();

			for (int i = 0; i < formattingInlines.Count; i++)
				paragraph.Inlines.Add(formattingInlines[i]);
		}

		/// <summary>
		/// Gets the value of the attached DisplayedError DependencyProperty.
		/// </summary>
		/// <param name="rtb">The RichTextBox whose value is to be returned</param>
		/// <seealso cref="DisplayedErrorProperty"/>
		/// <seealso cref="SetDisplayedError"/>
		public static SyntaxErrorInfo GetDisplayedError(RichTextBox rtb)
		{
			return (SyntaxErrorInfo)rtb.GetValue(SyntaxErrorInfo.DisplayedErrorProperty);
		}

		/// <summary>
		/// Sets the value of the attached DisplayedError DependencyProperty.
		/// </summary>
		/// <param name="rtb">The RichTextBox whose value is to be modified</param>
		/// <param name="value">The new <see cref="SyntaxErrorInfo"/> value.</param>
		/// <seealso cref="DisplayedErrorProperty"/>
		/// <seealso cref="GetDisplayedError"/>
		public static void SetDisplayedError(RichTextBox rtb, SyntaxErrorInfo value)
		{
			rtb.SetValue(SyntaxErrorInfo.DisplayedErrorProperty, value);
		}

		#endregion //DisplayedError

		#region Exception

		/// <summary>
		/// Gets the exception which occurred when parsing the formula.
		/// </summary>
		public Exception Exception
		{
			get { return _exception; }
		}

		#endregion  // Exception

		#endregion  // Public Properties

		#region Private Properties

		#region StringFormat2Arguments

		private Regex StringFormat2Arguments
		{
			get
			{
				if (_stringFormat2Arguments == null)
				{
					_stringFormat2Arguments = new Regex(@"\A(([^\{\}]+)|(\{\{)|(\}\})|(?<arg>(?<!\{)\{[0-9]\}(?!\})))+\Z"

, RegexOptions.Compiled

);
				}

				return _stringFormat2Arguments;
			}
		}

		#endregion  // StringFormat2Arguments

		#endregion  // Private Properties

		#endregion  // Properties
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