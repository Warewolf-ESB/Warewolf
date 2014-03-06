using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using Infragistics.Windows.Editors;
using System.Windows.Automation;
using System.Windows.Automation.Text;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;

namespace Infragistics.Windows.Automation.Peers.Editors
{
	/// <summary>
	/// Exposes <see cref="XamMaskedEditor"/> types to UI Automation
	/// </summary>
	public class XamMaskedEditorAutomationPeer : ValueEditorAutomationPeer,
		ITextProvider,
        // AS 9/5/08 NA 2008 Vol 2
        IExpandCollapseProvider
	{
        #region Member Variables

        // AS 9/5/08 NA 2008 Vol 2
        private ExpandCollapseState? _lastReturnedExpandState;

        #endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Creates a new instance of the <see cref="XamMaskedEditorAutomationPeer"/> class
		/// </summary>
		/// <param name="owner">The <see cref="XamMaskedEditor"/> for which the peer is being created</param>
		public XamMaskedEditorAutomationPeer(XamMaskedEditor owner)
			: base(owner)
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region GetAutomationControlTypeCore
		/// <summary>
		/// Returns an enumeration indicating the type of control represented by the automation peer.
		/// </summary>
		/// <returns>The <b>Edit</b> enumeration value</returns>
		protected override AutomationControlType GetAutomationControlTypeCore()
		{
			return AutomationControlType.Edit;
		}

		#endregion //GetAutomationControlTypeCore

		#region GetClassNameCore
		/// <summary>
		/// Returns the name of the <see cref="XamMaskedEditor"/>
		/// </summary>
		/// <returns>A string that contains 'XamMaskedEditor'</returns>
		protected override string GetClassNameCore()
		{
			return "XamMaskedEditor";
		}

		#endregion //GetClassNameCore

		#region GetPattern
		/// <summary>
		/// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the element that is associated with this <see cref="LabelAutomationPeer"/>.
		/// </summary>
		/// <param name="patternInterface">The pattern being requested</param>
		public override object GetPattern(PatternInterface patternInterface)
		{
			if (patternInterface == PatternInterface.Text)
				return this;

            // AS 9/5/08 NA 2008 Vol 2
            // AS 10/6/08 TFS7567
            // Meant to return expand/collapse and not toggle - which is not implemented.
            //
            //if (patternInterface == PatternInterface.Toggle)
            if (patternInterface == PatternInterface.ExpandCollapse)
                return this;

			return base.GetPattern(patternInterface);
		}
		#endregion //GetPattern

		#endregion //Base class overrides	

		#region Properties

		#region Editor

		private XamMaskedEditor Editor
		{
			get { return (XamMaskedEditor)this.Owner; }
		}

				#endregion //Editor

		#region LastCharacterIndex
		private int LastCharacterIndex
		{
            // AS 10/6/08 TFS7567
			//get { return this.Editor.EditInfo.GetTotalNumberOfDisplayChars(); }
			get { return XamMaskedEditor.GetTotalNumberOfDisplayChars( this.Editor.Sections ); }
		}
		#endregion //LastCharacterIndex

		#endregion //Properties	
	
		#region Methods

		#region ElementFromProvider
		internal DependencyObject ElementFromProvider(IRawElementProviderSimple provider)
		{
			DependencyObject owner = null;
			AutomationPeer peer = this.PeerFromProvider(provider);

			if (peer is UIElementAutomationPeer)
			{
				return ((UIElementAutomationPeer)peer).Owner;
			}
			if (peer is ContentElementAutomationPeer)
			{
				owner = ((ContentElementAutomationPeer)peer).Owner;
			}
			return owner;
		}
        #endregion //ElementFromProvider

        // AS 9/5/08 NA 2008 Vol 2
        #region RaiseExpandCollapseChanged
        internal void RaiseExpandCollapseChanged()
        {
            if (this._lastReturnedExpandState.HasValue &&
                AutomationPeer.ListenerExists(AutomationEvents.PropertyChanged))
            {
                ExpandCollapseState newState = ((IExpandCollapseProvider)this).ExpandCollapseState;

                if (newState != this._lastReturnedExpandState.Value)
                {
                    this.RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty, this._lastReturnedExpandState.Value, newState);
                }
            }
        }
        #endregion //RaiseExpandCollapseChanged

		#endregion //Methods

		#region ITextProvider Members

		#region DocumentRange
		ITextRangeProvider ITextProvider.DocumentRange
		{
			get
			{
				return new MaskedEditTextRange(this, 0, this.LastCharacterIndex);
			}
		} 
		#endregion //DocumentRange

		#region GetSelection
		ITextRangeProvider[] ITextProvider.GetSelection()
		{
			return new ITextRangeProvider[] { new MaskedEditTextRange(this, this.Editor.SelectionStart, this.Editor.SelectionStart + this.Editor.SelectionLength) };
		} 
		#endregion //GetSelection

		#region GetVisibleRanges
		ITextRangeProvider[] ITextProvider.GetVisibleRanges()
		{
            EditInfo info = this.Editor.EditInfo;

            // AS 10/6/08 TFS7567
            if (null == info)
                return new ITextRangeProvider[0];

			int firstChar = info.GetNearestDisplayChar(new Point(0,0), true);
			int lastChar = info.GetNearestDisplayChar(new Point(this.Editor.ActualWidth, this.Editor.ActualHeight), true);

			return new ITextRangeProvider[] { new MaskedEditTextRange(this, firstChar, lastChar) };
		} 
		#endregion //GetVisibleRanges

		#region RangeFromChild
		ITextRangeProvider ITextProvider.RangeFromChild(IRawElementProviderSimple childElement)
		{
			if (childElement == null)
				throw new ArgumentNullException("childElement");

			DependencyObject obj = this.ElementFromProvider(childElement);

			if (false == ((XamMaskedEditor)this.Owner).IsAncestorOf(obj))
				throw new InvalidOperationException(XamMaskedEditor.GetString("LE_InvalidOperationException_2"));

			return new MaskedEditTextRange(this, 0, this.LastCharacterIndex);
		} 
		#endregion //RangeFromChild

		#region RangeFromPoint
		ITextRangeProvider ITextProvider.RangeFromPoint(System.Windows.Point screenLocation)
		{
            EditInfo info = this.Editor.EditInfo;

            // AS 10/6/08 TFS7567
            if (null == info)
                return null;

            // JJD 11/06/07 - Calll PointFromScreenSafe so we don't get an exception throw
            // in XBAP semi-trust applications
            //Point clientPt = this.Editor.PointFromScreen(screenLocation);
			Point clientPt = Utilities.PointFromScreenSafe( this.Editor, screenLocation );
			int charIndex = info.GetNearestDisplayChar(clientPt, true);

			return new MaskedEditTextRange(this, charIndex, charIndex);
		} 
		#endregion //RangeFromPoint

		#region SupportedTextSelection
		SupportedTextSelection ITextProvider.SupportedTextSelection
		{
			get
			{
				return SupportedTextSelection.Single;
			}
		} 
		#endregion //SupportedTextSelection

		#endregion // ITextProvider

        // AS 9/5/08 NA 2008 Vol 2
        #region IExpandCollapseProvider Members

        private void VerifyCanExpandCollapse()
        {
            if (false == this.IsEnabled())
                throw new ElementNotEnabledException();

            if (this.Editor.HasDropDown == false)
                throw new InvalidOperationException();
        }

        void IExpandCollapseProvider.Collapse()
        {
            this.VerifyCanExpandCollapse();

            if (this.Editor.HasOpenDropDown)
                this.Editor.ToggleDropDown();
        }

        void IExpandCollapseProvider.Expand()
        {
            this.VerifyCanExpandCollapse();

            if (this.Editor.HasDropDown == false)
                throw new InvalidOperationException();

            if (false == this.Editor.HasOpenDropDown)
                this.Editor.ToggleDropDown();
        }

        ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState
        {
            get 
            {
                if (false == this.Editor.HasDropDown)
                    this._lastReturnedExpandState = ExpandCollapseState.LeafNode;
                else if (this.Editor.HasOpenDropDown)
                    this._lastReturnedExpandState = ExpandCollapseState.Expanded;
                else
                    this._lastReturnedExpandState = ExpandCollapseState.Collapsed;

                return this._lastReturnedExpandState.Value;
            }
        }

        #endregion //IExpandCollapseProvider

		#region MaskedEditTextRange





		internal class MaskedEditTextRange : ITextRangeProvider
		{
			#region Member Variables

			private XamMaskedEditorAutomationPeer _peer;
			private int _start;
			private int _end;

			#endregion //Member Variables

			#region Constructor
			internal MaskedEditTextRange(XamMaskedEditorAutomationPeer peer, int start, int end)
			{
				this._peer = peer;
				this._start = start;
				this._end = end;
			}

			#endregion //Constructor

			#region Properties

			#region EndingDisplayCharacterIndex
			private int EndingDisplayCharacterIndex
			{
				get
				{
					return Math.Max(this._start, this._end - 1);
				}
			}
			#endregion //EndingDisplayCharacterIndex

			#endregion //Properties

			#region Methods

			#region VerifyEndPoint
			private void VerifyEndPoint(TextPatternRangeEndpoint endPoint)
			{
				if (endPoint == TextPatternRangeEndpoint.End)
					this._end = Math.Max(this._end, this._start);
				else // if moving the end point, make sure the start point is before the end
					this._start = Math.Min(this._end, this._start);
			}
			#endregion //VerifyEndPoint

			#region GetOtherEndPoint
			private static TextPatternRangeEndpoint GetOtherEndPoint(TextPatternRangeEndpoint endPoint)
			{
				return endPoint == TextPatternRangeEndpoint.Start
					? TextPatternRangeEndpoint.End
					: TextPatternRangeEndpoint.Start;
			}
			#endregion //GetOtherEndPoint

			#endregion //Methods

			#region ITextRangeProvider Members

			#region AddToSelection
			void ITextRangeProvider.AddToSelection()
			{
				throw new InvalidOperationException();
			}
			#endregion //AddToSelection

			#region Clone
			ITextRangeProvider ITextRangeProvider.Clone()
			{
				return new MaskedEditTextRange(this._peer, this._start, this._end);
			}
			#endregion //Clone

			#region Compare
			bool ITextRangeProvider.Compare(ITextRangeProvider range)
			{
				if (null == range)
					throw new ArgumentNullException("range");

				MaskedEditTextRange editRange = range as MaskedEditTextRange;

				if (null == editRange)
					throw new ArgumentException( XamMaskedEditor.GetString( "LE_ArgumentException_1" ), "range" );

				return editRange._peer == this._peer &&
					editRange._start == this._start &&
					editRange._end == this._end;
			}
			#endregion //Compare

			#region CompareEndpoints
			int ITextRangeProvider.CompareEndpoints(TextPatternRangeEndpoint endpoint, ITextRangeProvider targetRange, TextPatternRangeEndpoint targetEndpoint)
			{
				if (null == targetRange)
					throw new ArgumentNullException("targetRange");

				MaskedEditTextRange editRange = targetRange as MaskedEditTextRange;

				if (null == editRange)
					throw new ArgumentException( XamMaskedEditor.GetString( "LE_ArgumentException_1" ), "targetRange" );

				int targetPt = targetEndpoint == TextPatternRangeEndpoint.Start ? editRange._start : editRange._end;
				int thisPt = endpoint == TextPatternRangeEndpoint.Start ? this._start : this._end;

				return thisPt.CompareTo(targetPt);
			}
			#endregion //CompareEndpoints

			#region ExpandToEnclosingUnit
			void ITextRangeProvider.ExpandToEnclosingUnit(TextUnit unit)
			{
				if (unit == TextUnit.Character)
				{
					// if this is a degenerate range then select the next character
					if (this._start == this._end)
					{
						if (this._start < this._peer.LastCharacterIndex)
							this._end++;
					}
				}
				else if (unit == TextUnit.Word || unit == TextUnit.Format)
				{
                    // AS 10/6/08 TFS7567
                    //EditInfo info = this._peer.Editor.EditInfo;
					//SectionBase startSection = info.GetSectionContainingPosition(this._start);
					//SectionBase endSection = info.GetSectionContainingPosition(this.EndingDisplayCharacterIndex) ?? info.LastSection;
                    SectionsCollection sections = this._peer.Editor.Sections;
					SectionBase startSection = EditInfo.GetSectionContainingPosition(this._start, sections);
					SectionBase endSection = EditInfo.GetSectionContainingPosition(this.EndingDisplayCharacterIndex, sections) ?? EditInfo.GetLastSection(sections);

					// if we couldn't get the section, then make this a degenerate range at the end
					if (startSection == null)
						this._start = this._end = this._peer.LastCharacterIndex;
					else
					{
						// start at the beginning of the first section
						this._start = startSection.FirstDisplayChar.OverallIndexInEdit;

						// end just after the last character in the section
						this._end = endSection.LastDisplayChar.OverallIndexInEdit + 1;
					}
				}
				else
				{
					// we only support character and word so 
					// for any larger unit select everything
					this._start = 0;
					this._end = this._peer.LastCharacterIndex;
				}
			}
			#endregion //ExpandToEnclosingUnit

			#region FindAttribute
			ITextRangeProvider ITextRangeProvider.FindAttribute(int attribute, object value, bool backward)
			{
				// we don't support returning the text attributes. if we do then we need to use the 
				// AutomationTextAttribute.LookupById which should return one of the TextPatternIdentifiers
				// attributes
				return null;
			}
			#endregion //FindAttribute

			#region FindText
			ITextRangeProvider ITextRangeProvider.FindText(string text, bool backward, bool ignoreCase)
			{
				string thisText = ((ITextRangeProvider)this).GetText(-1) ?? string.Empty;
				StringComparison comparison = ignoreCase
					? StringComparison.CurrentCultureIgnoreCase
					: StringComparison.CurrentCulture;

				int index;

				if (backward)
					index = thisText.LastIndexOf(text);
				else
					index = thisText.IndexOf(text);

				return index < 0
					? null
					: new MaskedEditTextRange(this._peer, index + this._start, index + this._start + text.Length);
			}
			#endregion //FindText

			#region GetAttributeValue
			object ITextRangeProvider.GetAttributeValue(int attribute)
			{
				// we don't support returning the text attributes. if we do then we need to use the 
				// AutomationTextAttribute.LookupById which should return one of the TextPatternIdentifiers
				// attributes
				return null;
			}
			#endregion //GetAttributeValue

			#region GetBoundingRectangles
			double[] ITextRangeProvider.GetBoundingRectangles()
			{
				ITextRangeProvider[] ranges = ((ITextProvider)this._peer).GetVisibleRanges();
				double[] bounds = new double[0];

				if (ranges != null && ranges.Length > 0)
				{
					Debug.Assert(ranges.Length == 1 && ranges[0] is MaskedEditTextRange, "We're expecting a single masked edit text range!");

					MaskedEditTextRange maskRange = ranges[0] as MaskedEditTextRange;

					if (null != maskRange)
					{
						// get the first and last character in view for this range
						int start = Math.Max(this._start, maskRange._start);
						int end = Math.Min(this._end, maskRange._end);

						Debug.Assert(start >= 0 || end < 0, "We should either have no characters in view or a valid start/end index");

						if (start >= 0)
						{
							// since the range could extend beyond the last character,
							// we need to decrement the position by 1
							int lastCharIndex = this._peer.LastCharacterIndex;
							int charStart = Math.Min(start, lastCharIndex - 1);
							int charEnd = Math.Max(charStart, Math.Min(end - 1, lastCharIndex - 1));

							XamMaskedEditor editor = this._peer.Editor;
                            // AS 10/6/08 TFS7567
                            EditInfo info = editor.EditInfo;
                            Debug.Assert(null != info);

							DisplayCharBase dc = info.GetDisplayCharAtPosition(charStart);
							Debug.Assert(dc != null, "We should be able to get to the display char if its in view!");
							Rect rect = editor.GetDisplayCharLocation(dc);

							if (charEnd != charStart)
							{
								dc = info.GetDisplayCharAtPosition(charEnd);
								Debug.Assert(dc != null, "We should be able to get to the display char if its in view!");
								Rect endRect = editor.GetDisplayCharLocation(dc);

								rect.Union(endRect);
							}
							else if (start == end)// same start and end character
							{
								// if this was a degenerate range at the end then 
								// shift to the right edge of the last character
								if (end == this._peer.LastCharacterIndex)
									rect.X = rect.Right;

								// if this was a degenerate range then remove the width...
								rect.Width = 0;
							}

							bounds = new double[]{
								rect.X,
								rect.Top,
								rect.Width,
								rect.Height
							};
						}
					}
				}

				return bounds;
			}
			#endregion //GetBoundingRectangles

			#region GetChildren
			IRawElementProviderSimple[] ITextRangeProvider.GetChildren()
			{
				// follow the text box impl - no children
				return null;
			}
			#endregion //GetChildren

			#region GetEnclosingElement
			IRawElementProviderSimple ITextRangeProvider.GetEnclosingElement()
			{
				return this._peer.ProviderFromPeer(this._peer);
			}
			#endregion //GetEnclosingElement

			#region GetText
			string ITextRangeProvider.GetText(int maxLength)
			{
				if (maxLength < -2)
					throw new ArgumentException();

                // AS 10/6/08 TFS7567
                //EditInfo info = this._peer.Editor.EditInfo;

				int thisLen = this._end - this._start;
				int length = maxLength < 0
					? thisLen
					: Math.Min(thisLen, maxLength);

                // AS 10/6/08 TFS7567
                //return null != info
				//	? info.GetText(info.MaskInfo.DisplayMode, this._start, length)
				//	: string.Empty;
                MaskInfo maskInfo = this._peer.Editor.MaskInfo;
                return EditInfo.GetText(maskInfo.DisplayMode, this._start, length, maskInfo.Sections);
			}
			#endregion //GetText

			#region Move
			int ITextRangeProvider.Move(TextUnit unit, int count)
			{
				if (count == 0)
					return 0;

				int moved = 0;
				bool backward = count < 0;

				if (unit == TextUnit.Character)
				{
					#region Character
					// if there is a range, we need to make this a degenerate
					// range for the first move
					if (this._end != this._start)
					{
						if (backward)
						{
							this._end = this._start;
							count++;
						}
						else //if (count > 0)
						{
							this._start = this._end;
							count--;
						};

						moved++;
					}

					// if there are left over moves...
					if (count != 0)
					{
						// move the start/end using the other method
						int movedMore = ((ITextRangeProvider)this).MoveEndpointByUnit(backward ? TextPatternRangeEndpoint.Start : TextPatternRangeEndpoint.End, unit, count);

						if (movedMore >= 0)
							moved += movedMore;
						else
							moved = -1;

						// if we moved backward then sync the end point with the starting point
						if (backward)
							this._end = this._start;
						else // if we moved forward, sync the start with the end
							this._start = this._end;
					}
					#endregion //Character
				}
				else if (unit == TextUnit.Word || unit == TextUnit.Format)
				{
					#region Word

					// move the start/end using the other method
					moved = ((ITextRangeProvider)this).MoveEndpointByUnit(backward ? TextPatternRangeEndpoint.Start : TextPatternRangeEndpoint.End, unit, count);

					// if we didn't move, then make sure the other end point is the same 
					if (moved == 0 && this._end != this._start)
						moved = -1;

					#endregion //Word
				}
				else
				{
					#region Document
					if (backward)
					{
						if (this._start != 0)
						{
							this._start = 0;
							moved = -1;
						}
					}
					else
					{
						if (this._end != this._peer.LastCharacterIndex)
						{
							this._end = this._peer.LastCharacterIndex;
							moved = -1;
						}
					}
					#endregion //Document
				}

				// the routines above only moved the start or end point depending on the direction
				// now we need to sync the other point with the point that was moved
				if (moved != 0)
				{
					if (backward)
						this._end = this._start;
					else
						this._start = this._end;

					// if we've moved to the beginning or end of the document range then return -1
					if (moved > 0 && (this._start == 0 || this._end == this._peer.LastCharacterIndex))
						moved = -1;
				}

				return moved;
			}
			#endregion //Move

			#region MoveEndpointByRange
			void ITextRangeProvider.MoveEndpointByRange(TextPatternRangeEndpoint endpoint, ITextRangeProvider targetRange, System.Windows.Automation.Text.TextPatternRangeEndpoint targetEndpoint)
			{
				if (null == targetRange)
					throw new ArgumentNullException("targetRange");

				MaskedEditTextRange editRange = targetRange as MaskedEditTextRange;

				if (null == editRange)
					throw new ArgumentException( XamMaskedEditor.GetString( "LE_ArgumentException_1" ), "range" );

				// find the new position from the target
				int newEndPt = targetEndpoint == TextPatternRangeEndpoint.Start ? editRange._start : editRange._end;

				// update the appropriate end point
				if (endpoint == TextPatternRangeEndpoint.Start)
					this._start = newEndPt;
				else
					this._end = newEndPt;

				// if moving the starting point, make sure the end point is after the start
				this.VerifyEndPoint(GetOtherEndPoint(endpoint));
			}
			#endregion //MoveEndpointByRange

			#region MoveEndpointByUnit
			int ITextRangeProvider.MoveEndpointByUnit(TextPatternRangeEndpoint endpoint, TextUnit unit, int count)
			{
				// exit if there are no moves to make
				if (count == 0)
					return 0;
				else if (count < 0 && endpoint == TextPatternRangeEndpoint.Start && this._start == 0)
					return 0;
				else if (count > 0 && endpoint == TextPatternRangeEndpoint.End && this._end == this._peer.LastCharacterIndex)
					return 0;

				bool backward = count < 0;
				bool isStart = endpoint == TextPatternRangeEndpoint.Start;
				int moved = 0;

				if (unit == TextUnit.Character)
				{
					#region Character

					// get the starting position
					int startPos = isStart ? this._start : this._end;

					// offset the position
					int newIndex = startPos + count;

					// make sure the position is within range
					newIndex = Math.Max(0, Math.Min(this._peer.LastCharacterIndex, newIndex));

					// update the moved count by the number of positions shifted
					moved += Math.Abs(newIndex - startPos);

					if (isStart)
						this._start = newIndex;
					else
						this._end = newIndex;

					#endregion //Character
				}
				else if (unit == TextUnit.Word || unit == TextUnit.Format)
				{
					#region Word

                    // AS 10/6/08 TFS7567
                    //EditInfo info = this._peer.Editor.EditInfo;
                    MaskInfo info = this._peer.Editor.MaskInfo;

					// get the starting pos based on whether we're moving forward or backward
					int startPos = backward ? this._start : this._end;

					// this is used to store where we will ultimately move to
					int newPos = startPos;

					// get the section that contains the start pos
                    // AS 10/6/08 TFS7567
                    //SectionBase section = info.GetSectionContainingPosition(startPos);
					SectionBase section = EditInfo.GetSectionContainingPosition(startPos, info.Sections);

					// if there was a section, then we might have an initial move to the
					// beginning or end of that section
					if (section != null)
					{
						// if backwards and we're not at the beginning of the section
						if (backward && startPos != section.FirstDisplayChar.OverallIndexInEdit)
						{
							// move to the beginning
							newPos = section.FirstDisplayChar.OverallIndexInEdit;
							moved++;
							count++;
						}
						else if (backward == false && startPos != section.LastDisplayChar.OverallIndexInEdit + 1)
						{
							// move to the end
							newPos = section.LastDisplayChar.OverallIndexInEdit + 1;
							moved++;
							count--;
						}
					}

					// move to the start/end of the specified section offset
					if (count != 0)
					{
						int startingIndex = section == null ? info.Sections.Count : section.Index;

						// offset by the remaining count
						int sectionIndex = startingIndex + count;

						// make sure its in range
						sectionIndex = Math.Max(0, Math.Min(info.Sections.Count, sectionIndex));

						// update the number of sections moved
						moved += Math.Abs(sectionIndex - startingIndex);

						// update the new pos
						if (sectionIndex == info.Sections.Count)
							newPos = this._peer.LastCharacterIndex;
						else if (backward)
							newPos = info.Sections[sectionIndex].FirstDisplayChar.OverallIndexInEdit;
						else
							newPos = info.Sections[sectionIndex].LastDisplayChar.OverallIndexInEdit + 1;
					}

					// update the start/end pos
					if (backward)
						this._start = newPos;
					else
						this._end = newPos;

					#endregion //Word
				}
				else
				{
					#region Document
					if (isStart)
						this._start = backward ? 0 : this._peer.LastCharacterIndex;
					else
						this._end = backward ? 0 : this._peer.LastCharacterIndex;

					moved = -1;
					#endregion //Document
				}

				// if moving the starting point, make sure the end point is after the start
				this.VerifyEndPoint(isStart ? TextPatternRangeEndpoint.End : TextPatternRangeEndpoint.Start);

				// if we've moved to the beginning or end of the document range then return -1
				if (moved > 0 && (this._start == 0 || this._end == this._peer.LastCharacterIndex))
					moved = -1;

				return moved;
			}
			#endregion //MoveEndpointByUnit

			#region RemoveFromSelection
			void ITextRangeProvider.RemoveFromSelection()
			{
				throw new InvalidOperationException();
			}
			#endregion //RemoveFromSelection

			#region ScrollIntoView
			void ITextRangeProvider.ScrollIntoView(bool alignToTop)
			{
				EditInfo info = this._peer.Editor.EditInfo;

                // AS 10/6/08 TFS7567
                if (null == info)
                    return;

				info.ScrollDisplayCharIntoView(this.EndingDisplayCharacterIndex);
				info.ScrollDisplayCharIntoView(this._start);
			}
			#endregion //ScrollIntoView

			#region Select
			void ITextRangeProvider.Select()
			{
				this._peer.Editor.SelectionStart = this._start;
				this._peer.Editor.SelectionLength = this._end - this._start;
			}
			#endregion //Select

			#endregion //ITextRangeProvider
		} 
		#endregion //MaskedEditTextRange
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