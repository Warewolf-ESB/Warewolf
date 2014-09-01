using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Enums.Enums;
using Dev2.Common.Lookups;

namespace Dev2.Activities.Designers2.Zip
{
    public class ZipDesignerViewModel : FileActivityDesignerViewModel
    {
        public IList<string> CompressionRatioList { get; private set; }

        public ZipDesignerViewModel(ModelItem modelItem)
            : base(modelItem, "File or Folder", "Destination")
        {
            AddTitleBarLargeToggle();
            AddTitleBarHelpToggle();

            CompressionRatioList = Dev2EnumConverter.ConvertEnumsTypeToStringList<CompressionRatios>();

            var selectionRatio = string.IsNullOrEmpty(CompressionRatio)
                ? CompressionRatios.Default
                : (CompressionRatios)Enum.Parse(typeof(CompressionRatios), CompressionRatio);

            SelectedCompressionRatioDescription = selectionRatio.GetDescription();
        }

        public string SelectedCompressionRatioDescription
        {
            get { return (string)GetValue(SelectedCompressionRatioDescriptionProperty); }
            set { SetValue(SelectedCompressionRatioDescriptionProperty, value); }
        }

        public static readonly DependencyProperty SelectedCompressionRatioDescriptionProperty = DependencyProperty.Register("SelectedCompressionRatioDescription", typeof(string), typeof(ZipDesignerViewModel), new PropertyMetadata(default(string), OnSelectedCompressionRatioDescriptionPropertyChanged));

        static void OnSelectedCompressionRatioDescriptionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var viewModel = (ZipDesignerViewModel)d;
            var description = args.NewValue as string;

            var enumValue = Dev2EnumConverter.GetEnumFromStringDiscription(description, typeof(CompressionRatios));
            if(enumValue != null)
            {
                viewModel.CompressionRatio = enumValue.ToString();
            }
        }

        // This MUST be the enum.ToString()!!!
        string CompressionRatio
        {
            set { SetProperty(value); }
            get { return GetProperty<string>(); }
        }

        public override void Validate()
        {
            Errors = null;
            ValidateUserNameAndPassword();
            ValidateDestinationUsernameAndPassword();
            ValidateInputPath();
        }

    }
}

