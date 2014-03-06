using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Calculations;
using System.Diagnostics;
using System.Windows.Threading;
using Infragistics.Calculations.Engine;
using PerCederberg.Grammatica.Parser;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using Infragistics.Controls.Interactions.Primitives;

namespace Infragistics.Controls.Interactions
{
	internal class FormulaCompiler
	{
		#region Member Variables

		private static readonly TimeSpan CompileDelay = TimeSpan.FromSeconds(1);

		private DispatcherTimer _delayedCompileTimer;
		private FormulaEditorBase _editor;
		private FormulaEditorDialog _editorDialog;
		
		#endregion  // Member Variables

		#region Constructor

		public FormulaCompiler(FormulaEditorBase editor)
		{
			_editor = editor;
			_editorDialog = _editor as FormulaEditorDialog;
		}

		#endregion  // Constructor

		#region Methods

		#region Compile

		public void Compile()
		{
			this.DelayedCompileTimer.Stop();

			IFormulaProvider formulaProvider = _editor.FormulaProvider;
			if (formulaProvider == null)
				return;

			string formula = _editor.Formula;

			ICalculationFormula compiledFormula = null;
			if (String.IsNullOrEmpty(formula) == false &&
				formulaProvider.CalculationManager != null &&
				formulaProvider.Reference != null)
			{
				compiledFormula = formulaProvider.CalculationManager.CompileFormula(formulaProvider.Reference, formula, true);
			}

			this.PopulateSyntaxErrors(compiledFormula);
		}

		#endregion  // Compile

		#region CompileAsync

		public void CompileAsync()
		{
			this.DelayedCompileTimer.Stop();

			if (_editor.FormulaProvider != null)
			{
				string formula = _editor.Formula;

				if (String.IsNullOrEmpty(formula))
				{
					this.PopulateSyntaxErrors(null);
				}
				else
				{
					this.DelayedCompileTimer.Start();
				}
			}
		}

		#endregion  // CompileAsync

		#region OnDelayedCompileTimerTick

		private void OnDelayedCompileTimerTick(object sender, EventArgs e)
		{
			this.Compile();
		}

		#endregion  // OnDelayedCompileTimerTick

		#region PopulateSyntaxErrors

		private void PopulateSyntaxErrors(ICalculationFormula compiledFormula)
		{
			if (compiledFormula == null)
			{
				_editor.SyntaxError = null;

				if (_editorDialog != null)
					_editorDialog.SyntaxErrorInfos = null;

				return;
			}

			_editor.SyntaxError = compiledFormula.SyntaxError;

			if (_editorDialog == null)
				return;

			List<SyntaxErrorInfo> syntaxErrors = null;

			try
			{
				UltraCalcFormula compiledFormulaInternal = compiledFormula as UltraCalcFormula;
				if (compiledFormulaInternal == null)
				{
					Debug.Assert(false, "Unexpected class implementing ICalculationFormula.");
					return;
				}

				Exception parseException = compiledFormulaInternal.ParseException;
				if (parseException == null)
				{
					Debug.Assert(compiledFormulaInternal.SyntaxError == null, "The SyntaxError should not be valid when the ParseException is null.");
					return;
				}

				syntaxErrors = new List<SyntaxErrorInfo>();

				ParserLogException logException = parseException as ParserLogException;
				if (logException == null)
				{
					syntaxErrors.Add(new SyntaxErrorInfo(_editorDialog, parseException));
					return;
				}

				int count = logException.GetErrorCount();

				for (int i = 0; i < count; i++)
					syntaxErrors.Add(new SyntaxErrorInfo(_editorDialog, logException.GetError(i)));
			}
			finally
			{
				if (syntaxErrors == null)
					_editorDialog.SyntaxErrorInfos = null;
				else
					_editorDialog.SyntaxErrorInfos = syntaxErrors.AsReadOnly();
			}
		}

		#endregion  // PopulateSyntaxErrors

		#endregion  // Methods

		#region Properties

		#region DelayedCompileTimer

		private DispatcherTimer DelayedCompileTimer
		{
			get
			{
				if (_delayedCompileTimer == null)
				{
					_delayedCompileTimer = new DispatcherTimer();
					_delayedCompileTimer.Interval = CompileDelay;
					_delayedCompileTimer.Tick += this.OnDelayedCompileTimerTick;
				}

				return _delayedCompileTimer;
			}
		}

		#endregion  // DelayedCompileTimer

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