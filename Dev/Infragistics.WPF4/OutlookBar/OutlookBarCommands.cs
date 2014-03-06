using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Infragistics.Windows.OutlookBar
{
    #region OutlookBarCommands class

    /// <summary>
    /// Provides the list of the RoutedCommands supported by the <see cref="XamOutlookBar"/>. 
    /// </summary>
    public static class OutlookBarCommands
    {
        #region Constructors

        static OutlookBarCommands()
        {
            ShowPopupCommand = new RoutedCommand("ShowPopup", typeof(OutlookBarCommands));
            ShowOptionsCommand = new RoutedCommand("ShowOptions", typeof(OutlookBarCommands));
            ShowFewerButtonsCommand = new RoutedCommand("ShowFewerButtons", typeof(OutlookBarCommands));
            ShowMoreButtonsCommand = new RoutedCommand("ShowMoreButtons", typeof(OutlookBarCommands));
            GroupMoveUpCommand = new RoutedCommand("GroupMoveUp", typeof(OutlookBarCommands));
            GroupMoveDownCommand = new RoutedCommand("GroupMoveDown", typeof(OutlookBarCommands));
            SelectGroupCommand = new RoutedCommand("SelectGroup", typeof(OutlookBarCommands));
        }

        #endregion //Constructors

        #region Commands
        // see OutlookBarCommands

        #region GroupMoveDownCommand
        /// <summary>
        /// Moves an <see cref="OutlookBarGroup"/> down if it is in the navigation area or moves the group right if it is in the overflow area.
		/// Supply the <see cref="OutlookBarGroup"/> to be moved via a CommandParameter (e.g., myButton.CommandParameter = aGroup).
        /// </summary>
		/// <seealso cref="GroupMoveUpCommand"/>
		/// <seealso cref="XamOutlookBar.ExecutingCommandEvent"/>
		/// <seealso cref="XamOutlookBar.ExecutedCommandEvent"/>
		/// <seealso cref="XamOutlookBar.ExecuteCommand"/>
		public static readonly RoutedCommand GroupMoveDownCommand;

        #endregion //GroupMoveDownCommand

        #region GroupMoveUpCommand

        /// <summary>
        /// Moves an <see cref="OutlookBarGroup"/> up if it is in the navigation area or moves the group left if it is in the overflow area.
		/// Supply the <see cref="OutlookBarGroup"/> to be moved via a CommandParameter (e.g., myButton.CommandParameter = aGroup).
		/// </summary>
		/// <seealso cref="GroupMoveDownCommand"/>
		/// <seealso cref="XamOutlookBar.ExecutingCommandEvent"/>
		/// <seealso cref="XamOutlookBar.ExecutedCommandEvent"/>
		/// <seealso cref="XamOutlookBar.ExecuteCommand"/>
		public static readonly RoutedCommand GroupMoveUpCommand;

        #endregion //GroupMoveUpCommand

        #region ShowPopupCommand

        /// <summary>
		/// Shows content of the selected group as popup when <see cref="XamOutlookBar.IsMinimized"/> is true.
        /// </summary>
		/// <seealso cref="XamOutlookBar.ExecutingCommandEvent"/>
		/// <seealso cref="XamOutlookBar.ExecutedCommandEvent"/>
		/// <seealso cref="XamOutlookBar.ExecuteCommand"/>
		public static readonly RoutedCommand ShowPopupCommand;

        #endregion //ShowPopupCommand

        #region ShowFewerButtonsCommand
        /// <summary>
		/// Shows fewer <see cref="OutlookBarGroup"/>s in the Navigation Area of the <see cref="XamOutlookBar"/>.
        /// </summary>
		/// <seealso cref="XamOutlookBar.ExecutingCommandEvent"/>
		/// <seealso cref="XamOutlookBar.ExecutedCommandEvent"/>
		/// <seealso cref="XamOutlookBar.ExecuteCommand"/>
		public static readonly RoutedCommand ShowFewerButtonsCommand;

        #endregion //ShringNavigationAreaCommand

        #region ShowMoreButtonsCommand
        /// <summary>
		/// Shows more <see cref="OutlookBarGroup"/>s in the Navigation Area of the <see cref="XamOutlookBar"/>.
		/// </summary>
		/// <seealso cref="XamOutlookBar.ExecutingCommandEvent"/>
		/// <seealso cref="XamOutlookBar.ExecutedCommandEvent"/>
		/// <seealso cref="XamOutlookBar.ExecuteCommand"/>
		public static readonly RoutedCommand ShowMoreButtonsCommand;

        #endregion //ShowMoreButtonsCommand

        #region ShowOptionsCommand
        /// <summary>
		/// Displays the <see cref="NavigationPaneOptionsControl"/> which let's the user set the visibility and position of the <see cref="OutlookBarGroup"/>s.
        /// </summary>
		/// <seealso cref="XamOutlookBar.ExecutingCommandEvent"/>
		/// <seealso cref="XamOutlookBar.ExecutedCommandEvent"/>
		/// <seealso cref="XamOutlookBar.ExecuteCommand"/>
		public static readonly RoutedCommand ShowOptionsCommand;

        #endregion //ShowOptionsCommand

        #region SelectGroupCommand
        /// <summary>
        /// Selects an <see cref="OutlookBarGroup"/>.
		/// Supply the <see cref="OutlookBarGroup"/> to be selected via a CommandParameter (e.g., myButton.CommandParameter = aGroup).
		/// </summary>
		/// <seealso cref="XamOutlookBar.ExecutingCommandEvent"/>
		/// <seealso cref="XamOutlookBar.ExecutedCommandEvent"/>
		/// <seealso cref="XamOutlookBar.ExecuteCommand"/>
		public static readonly RoutedCommand SelectGroupCommand;

        #endregion //SelectGroupCommand

        #endregion //Commands
    }

    #endregion //OutlookBarCommands	class

    #region NavigationPaneOptionsControlCommands class

    /// <summary>
    /// Provides the list of RoutedCommands supported by the <see cref="NavigationPaneOptionsControl"/>. 
    /// </summary>
    public static class NavigationPaneOptionsControlCommands
    {
        #region Constructors

        static NavigationPaneOptionsControlCommands()
        {
            CommitChangesAndCloseCommand =
                new RoutedCommand("CommitChangesAndClose", typeof(NavigationPaneOptionsControlCommands));
            MoveSelectedDownCommand =
                new RoutedCommand("MoveSelectedDownCommand", typeof(NavigationPaneOptionsControlCommands));
            MoveSelectedUpCommand =
                new RoutedCommand("MoveSelectedUpCommand", typeof(NavigationPaneOptionsControlCommands));
            ResetGroupSequenceAndVisibilityCommand =
                new RoutedCommand("ResetGroupSequenceAndVisibilityCommand", typeof(NavigationPaneOptionsControlCommands));
        }

        #endregion //Constructors

        #region Commands

        #region MoveSelectedDownCommand
        /// <summary>
        /// Moves the selected <see cref="OutlookBarGroup"/> group down in the <see cref="NavigationPaneOptionsControl"/>.
        /// </summary>
		/// <seealso cref="XamOutlookBar.ExecutingCommandEvent"/>
		/// <seealso cref="XamOutlookBar.ExecutedCommandEvent"/>
		/// <seealso cref="XamOutlookBar.ExecuteCommand"/>
		public static readonly RoutedCommand MoveSelectedDownCommand;

        #endregion //MoveSelectedDownCommand

        #region MoveSelectedUpCommand

        /// <summary>
        /// Moves the selected <see cref="OutlookBarGroup"/> group up in the <see cref="NavigationPaneOptionsControl"/>.
        /// </summary>
		/// <seealso cref="XamOutlookBar.ExecutingCommandEvent"/>
		/// <seealso cref="XamOutlookBar.ExecutedCommandEvent"/>
		/// <seealso cref="XamOutlookBar.ExecuteCommand"/>
		public static readonly RoutedCommand MoveSelectedUpCommand;

        #endregion //MoveSelectedUpCommand

        #region CommitChangesAndCloseCommand

        /// <summary>
        /// Commits changes made in the <see cref="NavigationPaneOptionsControl"/> and closes parent.
        /// </summary>
		/// <seealso cref="XamOutlookBar.ExecutingCommandEvent"/>
		/// <seealso cref="XamOutlookBar.ExecutedCommandEvent"/>
		/// <seealso cref="XamOutlookBar.ExecuteCommand"/>
		public static readonly RoutedCommand CommitChangesAndCloseCommand;

        #endregion //CommitChangesAndCloseCommand

        #region ResetGroupSequenceAndVisibiltyCommand

        /// <summary>
        /// Resets groups oreder an visibilty to the initial state and raises GroupsReset event.
        /// </summary>
		/// <seealso cref="XamOutlookBar.ExecutingCommandEvent"/>
		/// <seealso cref="XamOutlookBar.ExecutedCommandEvent"/>
		/// <seealso cref="XamOutlookBar.ExecuteCommand"/>
		public static readonly RoutedCommand ResetGroupSequenceAndVisibilityCommand;

        #endregion //ResetGroupSequenceAndVisibiltyCommand

        #endregion //Commands

    }

    #endregion //NavigationPaneOptionsControlCommands class
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