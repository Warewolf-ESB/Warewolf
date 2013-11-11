using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Dev2.Activities.Designers2.Core;

namespace Dev2.Activities.Designers2.Zip
{
    public class ZipDesignerViewModel : FileActivityDesignerViewModel
    {
        public IList<CompressionType> CompressionTypes { get; set; }

        public ZipDesignerViewModel(ModelItem modelItem)
            : base(modelItem, "File or Folder", "Destination")
        {
            AddTitleBarLargeToggle();
            AddTitleBarHelpToggle();
            CompressionTypes = new List<CompressionType>
                {
                    new CompressionType("None", "No Compression"),
                    new CompressionType("Partial", "Best Speed"),
                    new CompressionType("Normal", "Default"),
                    new CompressionType("Max", "Best Compression")
                };

            SelectedCompressionRatio = string.IsNullOrEmpty(CompressionRatio) ? CompressionTypes[0] : CompressionTypes.SingleOrDefault(c => c.CompressionRatio.Replace(" ","").Equals(CompressionRatio));
        }

        public override void Validate()
        {
            Errors = null;
            ValidateUserNameAndPassword();
            ValidateInputAndOutputPaths(true);
        }

        public CompressionType SelectedCompressionRatio
        {
            get { return (CompressionType)GetValue(SelectedCompressionRatioProperty); }
            set { SetValue(SelectedCompressionRatioProperty, value); }
        }

        public static readonly DependencyProperty SelectedCompressionRatioProperty =
            DependencyProperty.Register("SelectedCompressionRatio", typeof(CompressionType), typeof(ZipDesignerViewModel), new PropertyMetadata(null, OnSelectedCompressionRatioChanged));

        static void OnSelectedCompressionRatioChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (ZipDesignerViewModel)d;
            var value = e.NewValue as CompressionType;

            if (value != null)
            {
                viewModel.CompressionRatio = value.CompressionRatio;
            }
        }



        string CompressionRatio { set { SetProperty(value); } get { return GetProperty<string>(); } }
    }

    public class CompressionType
    {
        public string DisplayName { get; private set; }
        public string CompressionRatio { get; private set; }

        public CompressionType(string name, string compressionRatio)
        {
            CompressionRatio = compressionRatio;
            DisplayName = string.Format("{0} ({1})", name , compressionRatio);
        }
    }
}