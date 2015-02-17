using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Infragistics.Documents;
using Infragistics.Documents.Parsing;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for DbSyntaxEditor.xaml
    /// </summary>
    public partial class DbSyntaxEditor 
    {
        Grid _blackoutGrid;
        Window _window;
        public DbSyntaxEditor()
        {
            InitializeComponent();
            SyntaxEditor.Document = new TextDocument { IsReadOnly = true, Language = TSqlLanguage.Instance };

            var textBlock = new TextBlock { FontSize = 16.0, Margin = new Thickness(0, 0, 0, 0), Text = "Action Inspector" };
            Header = textBlock; 


            SyntaxEditor.Document.InitializeText(@"USE [Dev2TestingDB]
GO

/****** Object:  StoredProcedure [dbo].[proc_SmallFetch]    Script Date: 02/16/2015 06:40:55 *************8888888888888888888*********************************************************/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[proc_SmallFetch] 
AS
BEGIN
	select * from dbo.SmallInsert;
END


GO");
            DataContext = new HelpTextValue(@"USE [Dev2TestingDB]
GO

/****** Object:  StoredProcedure [dbo].[proc_SmallFetch]    Script Date: 02/16/2015 06:40:55 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[proc_SmallFetch] 
AS
BEGIN
	select * from dbo.SmallInsert;
END


GO");
        }


        public void ShowView()
        {
            IsModal = true;
            var effect = new BlurEffect { Radius = 10, KernelType = KernelType.Gaussian, RenderingBias = RenderingBias.Quality };
            var content = Application.Current.MainWindow.Content as Grid;
            _blackoutGrid = new Grid { Background = new SolidColorBrush(Colors.Black), Opacity = 0.75 };
            if (content != null)
            {
                content.Children.Add(_blackoutGrid);
            }
            Application.Current.MainWindow.Effect = effect;
      
            _window = new Window { WindowStyle = WindowStyle.None, AllowsTransparency = true, Background = Brushes.Transparent, SizeToContent = SizeToContent.Manual, MinWidth = 640 , MinHeight = 480,ResizeMode = ResizeMode.CanResize, WindowStartupLocation = WindowStartupLocation.CenterScreen, Content = this };
            _window.ShowDialog();
        }

        void RemoveBlackOutEffect()
        {
            Application.Current.MainWindow.Effect = null;
            var content = Application.Current.MainWindow.Content as Grid;
            if (content != null)
            {
                content.Children.Remove(_blackoutGrid);
            }
        }

        public void RequestClose()
        {
            RemoveBlackOutEffect();
            _window.Close();
        }

        // ReSharper disable InconsistentNaming
        private void Button_Click(object sender, RoutedEventArgs e)
            // ReSharper restore InconsistentNaming
        {
            RequestClose();
        }

    }
}
