using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace Infragistics
{

    // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
    /// <summary>
    /// For internal use only
    /// </summary>
    internal static class VisualStateUtilities
    {
        #region Constants

#pragma warning disable 1591

        public const string GroupActive                         = "ActiveStates";
        public const string GroupCalendar                       = "CalendarStates";
        public const string GroupChange                         = "ChangeStates";
        public const string GroupCheck                          = "CheckStates";
        public const string GroupCommon                         = "CommonStates";
        public const string GroupContextual                     = "ContextualStates";
        public const string GroupCurrent                        = "CurrentStates";
        public const string GroupDay                            = "DayStates";
        public const string GroupDrag                           = "DragStates";
        public const string GroupEdit                           = "EditStates";
        public const string GroupEmbedded                       = "EmbeddedStates";
        public const string GroupError                          = "ErrorStates";
        public const string GroupExpansion                      = "ExpansionStates";
        public const string GroupFilter                         = "FilterStates";
        public const string GroupFirstLastItem                  = "FirstLastItemStates";
        public const string GroupFixed                          = "FixedStates";
        public const string GroupFocus                          = "FocusStates";
        public const string GroupHasItems                       = "HasItemsStates";
        public const string GroupHighlight                      = "HighlightStates";
        public const string GroupInteraction                    = "InteractionStates";
        public const string GroupLeadingOrTrailing              = "LeadingOrTrailingStates";
        public const string GroupLocation                       = "LocationStates";
        public const string GroupMajor                          = "MajorStates";
        public const string GroupMinimized                      = "MinimizedStates";
        public const string GroupOpen                           = "OpenStates";
        public const string GroupPaneLocation                   = "PaneLocationStates";
        public const string GroupRecord                         = "RecordStates";
        public const string GroupSelection                      = "SelectionStates";
        public const string GroupSort                           = "SortStates";
        public const string GroupSplitterLocation               = "SplitterLocationStates";
        public const string GroupValidationEx                   = "ValidationStatesEx";
        public const string GroupWorkDay                        = "WorkDayStates";

		public const string GroupOrientation					= "OrientationStates";
		public const string GroupWorkingHour					= "WorkingHourStates";
		public const string GroupDirection						= "DirectionStates";

        public const string StateActive                         = "Active";
        public const string StateActiveDocument                 = "ActiveDocument";
        public const string StateAddRecord                      = "AddRecord";
        public const string StateAppMenu                        = "AppMenu";
        public const string StateAppMenuFooterToolbar           = "AppMenuFooterToolbar";
        public const string StateAppMenuRecentItems             = "AppMenuRecentItems";
        public const string StateAppMenuSubMenu                 = "AppMenuSubMenu";
        public const string StateBottom                         = "Bottom";
        public const string StateBottomMouseOverTab             = "Bottom_MouseOverTab";
        public const string StateCellArea                       = "CellArea";
        public const string StateCentury                        = "Century";
        public const string StateChecked                        = "Checked";
        public const string StateClosed                         = "Closed";
        public const string StateCollapsed                      = "Collapsed";
        public const string StateContextual                     = "Contextual";
        public const string StateCurrent                        = "Current";
        public const string StateChanged                        = "Changed";
        public const string StateDay                            = "Day";
        public const string StateDataRecord                     = "DataRecord";
        public const string StateDataRecordAlternateRow         = "DataRecord_AlternateRow";
        public const string StateDecade                         = "Decade";
        public const string StateDisabled                       = "Disabled";
        public const string StateDisplay                        = "Display";
        public const string StateDockedBottom                   = "DockedBottom";
        public const string StateDockedLeft                     = "DockedLeft";
        public const string StateDockedRight                    = "DockedRight";
        public const string StateDockedTop                      = "DockedTop";
        public const string StateDocument                       = "Document";
        public const string StateDragging                       = "Dragging";
        public const string StateDraggingDeferred				= "DraggingDeferred"; // AS 11/16/10 NA 11.1 - CalendarGroup Sizing/Scrolling
        public const string StateDragSource                     = "DragSource";
        public const string StateDropdownArea                   = "DropdownArea";
        public const string StateEditable                       = "Editable";
        public const string StateEditing                        = "Editing";
        public const string StateEmbedded                       = "Embedded";
        public const string StateError							= "Error";
        public const string StateExpanded                       = "Expanded";
        public const string StateFieldChooser                   = "FieldChooser";
        public const string StateFieldSelected                  = "FieldSelected";
        public const string StateFilterRecord                   = "FilterRecord";
        public const string StateFilteredIn                     = "FilteredIn";
        public const string StateFilteredOut                    = "FilteredOut";
        public const string StateFixed                          = "Fixed";
        public const string StateFloating                       = "Floating";
        public const string StateFocused                        = "Focused";
        public const string StateFocusedDropDown                = "FocusedDropDown";
        public const string StateGroupByArea                    = "GroupByArea";
        public const string StateHasItems                       = "HasItems";
        public const string StateHeaderArea                     = "HeaderArea";
        public const string StateHighlight                      = "Highlight";
        public const string StateInactive                       = "Inactive";
        public const string StateIndeterminate                  = "Indeterminate";
        public const string StateInvalidEx                      = "InvalidEx";
        public const string StateInvalidFocusedEx               = "InvalidFocusedEx";
        public const string StateInvalidUnfocusedEx             = "InvalidUnfocusedEx";
        public const string StateIsHighlighted                  = "IsHighlighted";
		public const string StateIsNotHighlighted				= "IsNotHighlighted";
		public const string StateIsLeadingOrTrailing			= "IsLeadingOrTrailing";
		public const string StateIsNotLeadingOrTrailing			= "IsNotLeadingOrTrailing";
        public const string StateLeft                           = "Left";
        public const string StateLeftMouseOverTab               = "Left_MouseOverTab";
        public const string StateMaximized                      = "Maximized";
        public const string StateMenu                           = "Menu";
        public const string StateMinimized                      = "Minimized";
        public const string StateMinimizedExpanded              = "MinimizedExpanded";
        public const string StateMonth                          = "Month";
        public const string StateMouseOver                      = "MouseOver";
        public const string StateNavigationArea                 = "NavigationArea";
        public const string StateNoError						= "NoError";
        public const string StateNoItems                        = "NoItems";
        public const string StateNonContextual                  = "NonContextual";
        public const string StateNonWorkDay                     = "NonWorkDay";
        public const string StateNormal                         = "Normal";
        public const string StateNotDragging                    = "NotDragging";
        public const string StateNotEmbedded                    = "NotEmbedded";
        public const string StateNotMinimized                   = "NotMinimized";
        public const string StateOpen                           = "Open";
        public const string StateOverflowArea                   = "OverflowArea";
        public const string StatePressed                        = "Pressed";
        public const string StatePreviewArea                    = "PreviewArea";
        public const string StateQAT                            = "QAT";
        public const string StateReadOnly                       = "ReadOnly";
        public const string StateRegularDay                     = "RegularDay";
        public const string StateRibbon                         = "Ribbon";
        public const string StateRight                          = "Right";
        public const string StateRightMouseOverTab              = "Right_MouseOverTab";
        public const string StateSelected                       = "Selected";
        public const string StateSelectedInactive               = "SelectedInactive";
        public const string StateSelectedUnfocused              = "SelectedUnfocused";
        public const string StateSortAscending                  = "SortAscending";
        public const string StateSortDescending                 = "SortDescending";
        public const string StateSwapTarget                     = "SwapTarget";
        public const string StateToday                          = "Today";
        public const string StateTop                            = "Top";
        public const string StateTopMouseOverTab                = "Top_MouseOverTab";
        public const string StateUnchanged                      = "Unchanged";
        public const string StateUnchecked                      = "Unchecked";
        public const string StateUneditable                     = "Uneditable";
        public const string StateUnfixed                        = "Unfixed";
        public const string StateUnfocused                      = "Unfocused";
        public const string StateUnpinned                       = "Unpinned";
        public const string StateUnselected                     = "Unselected";
        public const string StateUnsorted                       = "Unsorted";
        public const string StateValidEx                        = "ValidEx";
        public const string StateWorkDay                        = "WorkDay";
        public const string StateYear                           = "Year";

		public const string StateUp								= "Up";
		public const string StateDown                           = "Down";

		// Orientation
		public const string StateHorizontal						= "Horizontal";
		public const string StateVertical						= "Vertical";

		// WorkingHour
		public const string StateWorkingHour					= "WorkingHour";
		public const string StateNonWorkingHour					= "NonWorkingHour";

		// IsFirstLast
		public const string StateIsFirstItem			        = "IsFirstItem";
		public const string StateIsFirstAndLastItem	            = "IsFirstAndLastItem";
        public const string StateIsLastItem                     = "IsLastItem";
		public const string StateIsNotFirstOrLastItem		    = "IsNotFirstOrLastItem";

		// Day
		public const string StateIsFirstInDay			        = "IsFirstInDay";
		public const string StateIsFirstAndLastInDay	        = "IsFirstAndLastInDay";
        public const string StateIsLastInDay                    = "IsLastInDay";
		public const string StateIsNotFirstOrLastInDay			= "IsNotFirstOrLastInDay";

		// MajorTick
		public const string StateIsFirstInMajor			        = "IsFirstInMajor";
		public const string StateIsFirstAndLastInMajor	        = "IsFirstAndLastInMajor";
        public const string StateIsLastInMajor                  = "IsLastInMajor";
		public const string StateIsNotFirstOrLastInMajor		= "IsNotFirstOrLastInMajor";


#pragma warning restore 1591


        #endregion //Constants	
    

        /// <summary>
        /// For internal use only
        /// </summary>
        public static bool GetHasVisualStateGroups(Control control)
        {
            if (control == null)
                return false;

            int count = VisualTreeHelper.GetChildrenCount(control);

            for (int i = 0; i < count; i++ )
            {
                FrameworkElement feChild = VisualTreeHelper.GetChild(control, i) as FrameworkElement;

                if ( feChild != null && feChild.TemplatedParent == control )
                {
                    ValueSource valueSource = DependencyPropertyHelper.GetValueSource(feChild, VisualStateManager.VisualStateGroupsProperty);

                    return (valueSource.BaseValueSource != BaseValueSource.Default);
                }
            }

            return false;
        }


        /// <summary>
        /// For internal use only
        /// </summary>
        public static void GoToState(Control control, bool useTransitions, params string[] states)
        {
            if (states != null)
            {
                int count = states.Length;

                for (int i = 0; i < count; i++)
                {
                    if (VisualStateManager.GoToState(control, states[i], useTransitions))
                        return;
                }
            }
        }

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