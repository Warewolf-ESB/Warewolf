using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.OBJRecords
{
	// http://msdn.microsoft.com/en-us/library/dd952807(v=office.12).aspx
	internal class Obj
	{
		#region Member Variables

		private FtCmo cmo;
		private FtGmo gmo;
		private FtCf pictFormat;
		private FtPioGrbit pictFlags;
		private FtCbls cbls;
		private FtRbo rbo;
		private FtSbs sbs;
		private FtNts nts;
		private FtMacro macro;
		private FtPictFmla pictFmla;
		private ObjLinkFmla linkFmla;
		private FtCblsData checkBox;
		private FtRboData radioButton;
		private FtEdoData edit;
		private FtLbsData list;
		private FtGboData gbo;

		#endregion // Member Variables

		#region Constructors

		public Obj() { }

		public Obj(WorksheetShape shape)
		{
			if (shape is WorksheetCellComment)
			{
				this.cmo = new FtCmo(ObjectType.Comment);
				this.nts = new FtNts();
			}
			else if (shape is WorksheetImage)
			{
				this.cmo = new FtCmo(ObjectType.Picture);
				this.pictFormat = new FtCf();
				this.pictFlags = new FtPioGrbit();
			}
			else if (shape is WorksheetShapeGroup)
			{
				this.cmo = new FtCmo(ObjectType.Group);
				this.gmo = new FtGmo();
			}
			else if (shape is WorksheetChart)
			{
				this.cmo = new FtCmo(ObjectType.Chart);
			}
			else if (shape is PredefinedShapes.RectangleShape)
			{
				this.cmo = new FtCmo(ObjectType.Rectangle);
			}
			else if (shape is PredefinedShapes.LineShape)
			{
				this.cmo = new FtCmo(ObjectType.Line);
			}
			else if (shape is PredefinedShapes.EllipseShape)
			{
				this.cmo = new FtCmo(ObjectType.Oval);
			}
			else if (Enum.IsDefined(typeof(PredefinedShapeType), (PredefinedShapeType)shape.Type2003))
			{
				this.cmo = new FtCmo(ObjectType.MicrosoftOfficeDrawing);
			}
			else
			{
				Utilities.DebugFail("This shape should have had round-trip data.");
				this.cmo = new FtCmo(ObjectType.Polygon);
			}
		}

		#endregion // Constructors

		#region Methods

		#region Load

		public void Load(Biff8RecordStream stream)
		{
			this.cmo = FtCmo.Load(stream);

			ObjectType objectType = this.cmo.Ot;
			switch (objectType)
			{
				case ObjectType.Group:
					this.gmo = FtGmo.Load(stream);
					break;

				case ObjectType.Picture:
					this.pictFormat = FtCf.Load(stream);
					this.pictFlags = FtPioGrbit.Load(stream);
					break;

				case ObjectType.CheckBox:
				case ObjectType.OptionButton:
					this.cbls = FtCbls.Load(stream);

					if (objectType == ObjectType.OptionButton)
						this.rbo = FtRbo.Load(stream);
					break;

				case ObjectType.Spinner:
				case ObjectType.ScrollBar:
				case ObjectType.ListBox:
				case ObjectType.ComboBox:
					this.sbs = FtSbs.Load(stream);
					break;

				case ObjectType.Comment:
					this.nts = FtNts.Load(stream);
					break;
			}

			this.macro = FtMacro.TryLoad(stream);

			switch (objectType)
			{
				case ObjectType.Picture:
					this.pictFmla = FtPictFmla.TryLoad(stream, this);
					break;

				case ObjectType.CheckBox:
				case ObjectType.OptionButton:
					this.linkFmla = ObjLinkFmla.TryLoad(stream, this);
					this.checkBox = FtCblsData.Load(stream, this);

					if (objectType == ObjectType.OptionButton)
						this.radioButton = FtRboData.Load(stream);
					break;

				case ObjectType.Spinner:
				case ObjectType.ScrollBar:
				case ObjectType.ListBox:
				case ObjectType.ComboBox:
					this.linkFmla = ObjLinkFmla.TryLoad(stream, this);

					if (objectType == ObjectType.ListBox || objectType == ObjectType.ComboBox)
						this.list = FtLbsData.Load(stream, this);
					break;

				case ObjectType.EditBox:
					this.edit = FtEdoData.Load(stream);
					break;

				case ObjectType.GroupBox:
					this.gbo = FtGboData.Load(stream);
					break;
			}

			if (objectType != ObjectType.ListBox && objectType != ObjectType.ComboBox)
			{
				uint reserved = stream.ReadUInt32();
				Debug.Assert(reserved == 0, "The end of this stream should be zero.");
			}
		}

		#endregion  // Load

		#region Save

		public void Save(Biff8RecordStream stream)
		{
			if (this.cmo == null)
			{
				Utilities.DebugFail("The cmo field must not be null.");
				return;
			}

			this.cmo.Save(stream);

			ObjectType objectType = this.cmo.Ot;
			switch (objectType)
			{
				case ObjectType.Group:
					if (this.gmo == null)
					{
						Utilities.DebugFail("The gmo field must not be null.");
						return;
					}

					this.gmo.Save(stream);
					break;

				case ObjectType.Picture:
					if (this.pictFormat == null)
					{
						Utilities.DebugFail("The pictFormat field must not be null.");
						return;
					}

					if (this.pictFlags == null)
					{
						Utilities.DebugFail("The pictFlags field must not be null.");
						return;
					}

					this.pictFormat.Save(stream);
					this.pictFlags.Save(stream);
					break;

				case ObjectType.CheckBox:
				case ObjectType.OptionButton:
					if (this.cbls == null)
					{
						Utilities.DebugFail("The cbls field must not be null.");
						return;
					}

					this.cbls.Save(stream);

					if (objectType == ObjectType.OptionButton)
					{
						if (this.rbo == null)
						{
							Utilities.DebugFail("The rbo field must not be null.");
							return;
						}

						this.rbo.Save(stream);
					}
					break;

				case ObjectType.Spinner:
				case ObjectType.ScrollBar:
				case ObjectType.ListBox:
				case ObjectType.ComboBox:
					if (this.sbs == null)
					{
						Utilities.DebugFail("The sbs field must not be null.");
						return;
					}

					this.sbs.Save(stream);
					break;

				case ObjectType.Comment:
					if (this.nts == null)
					{
						Utilities.DebugFail("The nts field must not be null.");
						return;
					}

					this.nts.Save(stream);
					break;
			}

			if (this.macro != null)
				this.macro.Save(stream);

			switch (objectType)
			{
				case ObjectType.Picture:
					if (this.pictFmla != null)
						this.pictFmla.Save(stream, this);
					break;

				case ObjectType.CheckBox:
				case ObjectType.OptionButton:
					if (this.linkFmla != null)
						this.linkFmla.Save(stream, this);

					if (this.checkBox == null)
					{
						Utilities.DebugFail("The checkBox field must not be null.");
						return;
					}

					this.checkBox.Save(stream, this);

					if (objectType == ObjectType.OptionButton)
					{
						if (this.radioButton == null)
						{
							Utilities.DebugFail("The radioButton field must not be null.");
							return;
						}

						this.radioButton.Save(stream);
					}
					break;

				case ObjectType.Spinner:
				case ObjectType.ScrollBar:
				case ObjectType.ListBox:
				case ObjectType.ComboBox:
					if (this.linkFmla != null)
						this.linkFmla.Save(stream, this);

					if (objectType == ObjectType.ListBox || objectType == ObjectType.ComboBox)
					{
						if (this.list == null)
						{
							Utilities.DebugFail("The list field must not be null.");
							return;
						}

						this.list.Save(stream, this);
					}
					break;

				case ObjectType.EditBox:
					if (this.edit == null)
					{
						Utilities.DebugFail("The edit field must not be null.");
						return;
					}

					this.edit.Save(stream);
					break;

				case ObjectType.GroupBox:
					if (this.gbo == null)
					{
						Utilities.DebugFail("The gbo field must not be null.");
						return;
					}

					this.gbo.Save(stream);
					break;
			}

			if (objectType != ObjectType.ListBox && objectType != ObjectType.ComboBox)
				stream.Write((uint)0);
		}

		#endregion  // Save

		#endregion // Methods

		#region Properties

		public FtCmo Cmo
		{
			get { return this.cmo; }
		}

		public ObjLinkFmla LinkFmla
		{
			get { return this.linkFmla; }
		}

		public FtLbsData List
		{
			get { return this.list; }
		}

		public FtMacro Macro
		{
			get { return this.macro; }
		}

		public FtPioGrbit PictFlags
		{
			get { return this.pictFlags; }
		}

		public FtPictFmla PictFmla
		{
			get { return this.pictFmla; }
		}

		public FtRboData RadioButton
		{
			get { return this.radioButton; }
		}

		#endregion // Properties
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