using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Unlimited.Applications.BusinessDesignStudio.Activities.AppResources.WebsiteBuilder
{
    [Serializable]
    public class TemplateTag : INotifyPropertyChanged
    {
        private string _label;

        public Guid TemplateTagID { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
        public string Label
        {
            get
            {
                return _label;
            }
            set
            {
                _label = value;
                RaisePropertyChanged("Label");
            }
        }
        public string ServiceName { get; set; }
        public string IDName { get; set; }
        public string ValidationServiceName { get; set; }
        public string WebFormServiceName { get; set; }

        public TemplateTag()
        {

        }

        public TemplateTag(Guid _templateTagID, string _type, string _content, string _label, string _serviceName, string _idName, string _validationServiceName, string _webFormServiceName)
        {
            this.TemplateTagID = _templateTagID;
            this.Type = _type;
            this.Content = _content;
            this.Label = _label;
            this.ServiceName = _serviceName;
            this.IDName = _idName;
            this.ValidationServiceName = _validationServiceName;
            this.WebFormServiceName = _webFormServiceName;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}
