using System;
using System.Text;
using System.Net;
using System.IO;
using System.Windows.Media.Imaging;
using Dev2.Studio.Core.AppResources.Converters;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.AppResources.ExtensionMethods;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Interfaces;
using System.ComponentModel.Composition;
using System.Xml;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.Core.ViewModels.Navigation;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Studio.Core {
    public static class ResourceHelper {
        public static string GetImageUri(string imageUri) {
            return string.Format("{0}{1}", StringResources.Uri_Application_Image_Website, imageUri);
        }

        public static string GetWebPageElementNames(string xmlConfig) {           
            XmlDocument xdoc = new XmlDocument();
            string xmlOutput = string.Empty;
            try {
                xdoc.LoadXml(xmlConfig);
                foreach (XmlNode xnode in xdoc.SelectNodes("//*[contains(name(),'Dev2elementName')]")) {
                    string tempString = xnode.InnerText;                   
                    tempString = "<Dev2ElementName>" + tempString + "</Dev2ElementName>";
                    xmlOutput = xmlOutput + tempString;
                }
            }
            catch (XmlException) {
            }
            return xmlOutput;
        }

        public static string MergeXmlConfig(string xmlConfig, string elementList) {
            string xmlOutputConfig = string.Empty;
            XmlDocument xdoc = new XmlDocument();
            try {
                xdoc.LoadXml(xmlConfig);
                XmlNode rootNode = xdoc.SelectSingleNode("//Dev2XMLResult");
                if (rootNode == null)
                {
                    rootNode = xdoc.SelectSingleNode("//Dev2WebpartConfig");
                }
                if (rootNode != null)
                {
                    XmlElement elem = xdoc.CreateElement("Dev2WebPageElementNames");
                    elem.InnerXml = elementList;
                    rootNode.AppendChild(elem);
                    xmlOutputConfig = rootNode.OuterXml;
                }
            }
            catch (XmlException) {
            }
            return xmlOutputConfig;
        }

        public static string PropertyEditorHtmlInject(string htmlToRender, string webServerUri) {
            if (htmlToRender.ToUpper().Contains("</HEAD>")) {
                int location = htmlToRender.ToUpper().IndexOf("</HEAD>");
                if (location > 0) {

                    htmlToRender = htmlToRender.Insert(location, PropertyEditorJavascriptHelper);

                    if (htmlToRender.ToUpper().IndexOf("JQUERY") <= 0) {
                        htmlToRender = htmlToRender.Insert(location, PropertyEditorJQueryInject(webServerUri));
                    }

                }
            }
            else {
                htmlToRender += ResourceHelper.PropertyEditorJavascriptHelper;
            }
            return htmlToRender;
        }

        public static string PropertyEditorJQueryInject(string webServerUri) {

            return @"<script src=""" + webServerUri + @"/scripts/jquery-1.7.js"" type=""text/javascript""></script>
                            <script src=""" + webServerUri + @"/scripts/jquery-ui.js"" type=""text/javascript""></script>";

        }

        public static BitmapImage GetImage(string uri) {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(uri, UriKind.RelativeOrAbsolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;
        }

        public static string PropertyEditorJavascriptHelper {
            get {
                return
                    @"
         
                <script type=""text/javascript"">
                           $(document).ready(function () {
                $('form').submit(function () {

                        var formPostData = $(this).serialize();
                        if(isValidForm) {
                            window.external.Dev2Set(formPostData, document.forms[0].action);
                        }
                        return false;
                    
                });

                $('input[type=button]').click(function () {
                    var item = $('input[id ^= ""dev2hidden""]').remove();
                    $('form').remove('input[id ^= ""dev2hidden""]');
                    $('form').append('<input type=""hidden"" name=""' + this.name + '"" id=""dev2hidden' + this.id + '""' + ' value=""' + this.value + '""/>');
                    $('form').submit();
                })

                $('input[type=submit]').click(function () {
                    var item = $('input[id ^= ""dev2hidden""]').remove();
                    $('form').remove('input[id ^= ""dev2hidden""]');
                    $('form').append('<input type=""hidden"" name=""' + this.name + '"" id=""dev2hidden' + this.id + '""' + ' value=""' + this.value + '""/>');
                })

});
                </script>

";

            }
        }

        public static bool OpenWindow(string _requestString, out string htmlToRender) {
            bool openWindow = false;
            var request = WebRequest.Create(_requestString) as HttpWebRequest;

            htmlToRender = string.Empty;

            using (var response = request.GetResponse() as HttpWebResponse) {

                switch (response.ContentType) {
                    case "text/html":
                    openWindow = true;
                    using (StreamReader reader = new StreamReader(response.GetResponseStream())) {
                        htmlToRender = reader.ReadToEnd();
                    }
                    break;

                    default:
                    openWindow = false;
                    break;
                }
            }

            return openWindow;
        }

        public static bool OpenWindow(string postUri, string postData, out string htmlToRender) {
            bool openWindow = false;
            htmlToRender = string.Empty;

            var request = WebRequest.Create(postUri) as HttpWebRequest;
            request.Method = "POST";

            byte[] byteData = UTF8Encoding.UTF8.GetBytes(postData);

            // Set the content length in the request headers  
            request.ContentLength = byteData.Length;

            using (Stream postStream = request.GetRequestStream()) {
                postStream.Write(byteData, 0, byteData.Length);
            }

            using (var response = request.GetResponse() as HttpWebResponse) {

                switch (response.ContentType) {
                    case "text/html":
                    openWindow = true;
                    using (StreamReader reader = new StreamReader(response.GetResponseStream())) {
                        htmlToRender = reader.ReadToEnd();
                    }
                    break;

                    default:
                    openWindow = false;
                    break;
                }
            }

            return openWindow;
        }

        public static string Get(string getUri) {
            string returnValue = string.Empty;

            var request = WebRequest.Create(getUri) as HttpWebRequest;
            request.Method = "GET";

            using (var response = request.GetResponse() as HttpWebResponse) {
                using (StreamReader reader = new StreamReader(response.GetResponseStream())) {
                    returnValue = reader.ReadToEnd();
                }
            }

            return returnValue;
        }

        public static string Post(string url, string postData) {
            string returnValue = string.Empty;

            var request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "POST";

            byte[] byteData = UTF8Encoding.UTF8.GetBytes(postData);

            // Set the content length in the request headers  
            request.ContentLength = byteData.Length;

            using (Stream postStream = request.GetRequestStream()) {
                postStream.Write(byteData, 0, byteData.Length);
            }

            using (var response = request.GetResponse() as HttpWebResponse) {
                using (StreamReader reader = new StreamReader(response.GetResponseStream())) {
                    returnValue = reader.ReadToEnd();
                }
            }

            return returnValue;
        }

        public static IDev2StudioView OpenPropertyEditor(string serviceName, string xmlConfiguration, IPropertyEditorWizard _editingViewModel) {

            IDev2StudioView win = null;
            //string requestString = string.Format("{0}/services/{1}.wiz", CurrentWebServer, serviceName);
            //string htmlToRender;

            //if (ResourceHelper.OpenWindow(requestString, xmlConfiguration, out htmlToRender)) {

            //    win = new WebPropertyEditorWindow(_editingViewModel);
            //    win.Title = string.Format(StringResources.Window_Title_Property_Editor, serviceName);

            //    htmlToRender = ResourceHelper.PropertyEditorHtmlInject(htmlToRender);

            //    win.propertyBrowser.NavigateToString(htmlToRender);
            //    //dropItem.Child = new Label { Content = (dragSource as NavigationItemViewModel).IResourceModelItem.ResourceName };
            //}
            //else {
            //    requestString = string.Format("{0}/services/{1}.wiz", ResourceHelper.CurrentWebServer, "LiteralControl");
            //    if (ResourceHelper.OpenWindow(requestString, xmlConfiguration, out htmlToRender)) {

            //        win = new WebPropertyEditorWindow(_editingViewModel);
            //        win.Title = string.Format(StringResources.Window_Title_Property_Editor, serviceName);

            //        htmlToRender = ResourceHelper.PropertyEditorHtmlInject(htmlToRender);

            //        win.propertyBrowser.NavigateToString(ConvertExtendedASCII(htmlToRender));
            //        //dropItem.Child = new Label { Content = (dragSource as NavigationItemViewModel).IResourceModelItem.ResourceName };
            //    }
            //}

            return win;
        }

        public static string ConvertExtendedASCII(string HTML) {
            string retVal = "";
            char[] s = HTML.ToCharArray();

            foreach (char c in s) {
                if (Convert.ToInt32(c) > 127)
                    retVal += "&#" + Convert.ToInt32(c) + ";";
                else
                    retVal += c;
            }

            return retVal;
        }


        public static IContextualResourceModel GetContextualResourceModel(object dataContext)
        {
            IContextualResourceModel resourceModel = null;

            TypeSwitch.Do(
                dataContext,
                TypeSwitch.Case<IContextualResourceModel>(x => resourceModel = x),
                TypeSwitch.Case<IWorkflowDesignerViewModel>(x => resourceModel = x.ResourceModel),
                TypeSwitch.Case<IServiceDebugInfoModel>(x => resourceModel = x.ResourceModel),
                TypeSwitch.Case<ITreeNode<IContextualResourceModel>>(x => resourceModel = x.DataContext),
                TypeSwitch.Case<ILayoutGridViewModel>(x => resourceModel = x.ResourceModel),
                TypeSwitch.Case<IWebActivity>(x => resourceModel = x.ResourceModel));

            return resourceModel;
        }

        public static string GetIconPath(IContextualResourceModel resource)
        {
            string iconPath = resource.IconPath;
            if (string.IsNullOrEmpty(resource.UnitTestTargetWorkflowService))
            {
                if (string.IsNullOrEmpty(resource.IconPath))
                {
                    iconPath = ResourceType.WorkflowService.GetIconLocation();
                }
                else if (!resource.IconPath.Contains(StringResources.Pack_Uri_Application_Image))
                {
                    var imageUriConverter = new ContextualResourceModelToImageConverter();
                    var iconUri = imageUriConverter.Convert(resource, null, null, null) as Uri;
                    if (iconUri != null) iconPath = iconUri.ToString();
                }
            }
            else
            {
                iconPath = string.IsNullOrEmpty(resource.IconPath)
                               ? StringResources.Navigation_UnitTest_Icon_Pack_Uri
                               : resource.IconPath;
            }
            return iconPath;
        }

        public static bool IsWebpage(IContextualResourceModel resource)
        {
            bool isWebpage = resource.Category.Equals("Webpage", StringComparison.InvariantCultureIgnoreCase)
                             ||
                             resource.Category.Equals("Human Interface Workflow",
                                                      StringComparison.InvariantCultureIgnoreCase);

            return isWebpage;
        }

        public static Type GetUserInterfaceType(IContextualResourceModel resource)
        {
            Type userInterfaceType = null;

            if (resource.Category.Equals("Webpage", StringComparison.InvariantCultureIgnoreCase)
                || resource.Category.Equals("Human Interface Workflow", StringComparison.InvariantCultureIgnoreCase))
            {
                userInterfaceType = typeof(DsfWebPageActivity);
            }

            if (resource.Category.Equals("Website", StringComparison.InvariantCultureIgnoreCase))
            {
                userInterfaceType = typeof(DsfWebSiteActivity);
            }

            return userInterfaceType;
        }

        public static string GetWizardName(IContextualResourceModel resource)
        {
            if (resource == null) return string.Empty;
            if (string.IsNullOrWhiteSpace(resource.ResourceName)) return string.Empty;

            return resource.ResourceName + ".wiz";
        }

    }
}
