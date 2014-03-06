using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Infragistics.Windows.Design.SmartTagFramework
{
    /// <summary>
    /// The <see cref="LinkButton"/> class simulates some of the finctionalities of the well-known class with the same name in ASP.NET
    /// </summary>
    public partial class LinkButton : UserControl
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of <see cref="LinkButton"/>
        /// </summary>
        public LinkButton()
        {
            InitializeComponent();
        }

        #endregion //Constructor

        #region Properties

        #region Public Properties

        #region Command

        /// <summary>
        /// Identifies the <see cref="Command"/> dependency property
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command",
            typeof(ICommand), typeof(LinkButton));

        /// <summary>
        /// Description
        /// </summary>
        /// <seealso cref="CommandProperty"/>
        [Bindable(true)]
        [Category("Action")]
        [Localizability(LocalizationCategory.NeverLocalize)]
        public ICommand Command
        {
            get
            {
                return (RoutedCommand)this.GetValue(LinkButton.CommandProperty);
            }
            set
            {
                this.SetValue(LinkButton.CommandProperty, value);
            }
        }

        #endregion //Command

        #region CommandParameter

        /// <summary>
        /// Identifies the <see cref="Command"/> dependency property
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter",
            typeof(object), typeof(LinkButton));

        /// <summary>
        /// Description
        /// </summary>
        /// <seealso cref="CommandParameterProperty"/>
        [Bindable(true)]
        [Category("Action")]
        [Localizability(LocalizationCategory.NeverLocalize)]
        public object CommandParameter
        {
            get
            {
                return this.GetValue(LinkButton.CommandParameterProperty);
            }
            set
            {
                this.SetValue(LinkButton.CommandParameterProperty, value);

            }
        }

        #endregion //CommandParameter

        #region Text

        /// <summary>
        /// Identifies the <see cref="Text"/> dependency property
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text",
            typeof(string), typeof(LinkButton),
             new FrameworkPropertyMetadata("LinkButton1", new PropertyChangedCallback(OnTextChanged)));

        /// <summary>
        /// Description
        /// </summary>
        /// <seealso cref="TextProperty"/>
        [Description("Description")]
        [Category("Behavior")]
        [Bindable(true)]
        public string Text
        {
            get
            {
                return (string)this.GetValue(LinkButton.TextProperty);
            }
            set
            {
                this.SetValue(LinkButton.TextProperty, value);
            }
        }

        private static void OnTextChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            LinkButton linkButton = (LinkButton)o;
            linkButton.run.Text = (string)e.NewValue;
        }

        #endregion //Text

        #endregion //Public Properties

        #endregion //Properties

        #region Events

        #region Click

        /// <summary>
        /// Event ID for the <see cref="Click"/> routed event
        /// </summary>
        /// <seealso cref="Click"/>
        /// <seealso cref="OnClick"/>
        /// <seealso cref="RoutedEventArgs"/>
        /// <seealso cref="RoutedEventHandler"/>
        public static readonly RoutedEvent ClickEvent =
            EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LinkButton));

        /// <summary>
        /// Occurs when user click on the button
        /// </summary>
        /// <seealso cref="Click"/>
        /// <seealso cref="ClickEvent"/>
        /// <seealso cref="RoutedEventArgs"/>
        /// <seealso cref="RoutedEventHandler"/>
        protected virtual void OnClick(RoutedEventArgs args)
        {
            this.RaiseEvent(args);
        }

        internal void RaiseClick(RoutedEventArgs args)
        {
            args.RoutedEvent = LinkButton.ClickEvent;
            args.Source = this;
            this.OnClick(args);
        }

        /// <summary>
        /// Occurs when user click on the button
        /// </summary>
        /// <seealso cref="OnClick"/>
        /// <seealso cref="ClickEvent"/>
        /// <seealso cref="RoutedEventArgs"/>
        /// <seealso cref="RoutedEventHandler"/>
        [Description("Occurs when user click on the button")]
        [Category("Behavior")]
        public event RoutedEventHandler Click
        {
            add
            {
                base.AddHandler(LinkButton.ClickEvent, value);
            }
            remove
            {
                base.RemoveHandler(LinkButton.ClickEvent, value);
            }
        }

        #endregion //Click

        #endregion //Events

        #region Event Handlers

        #region hyperLink_Click

        /// <summary>
        /// Raise Click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hyperLink_Click(object sender, RoutedEventArgs e)
        {
            RaiseClick(e);
        }

        #endregion //hyperLink_Click

        #endregion //Event Handlers
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