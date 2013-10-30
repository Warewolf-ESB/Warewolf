using System;
using System.Windows;

namespace Dev2.Settings.Security
{
    public class WindowsGroupPermission : DependencyObject
    {
        public static readonly DependencyProperty IsServerProperty = DependencyProperty.Register("IsServer", typeof(bool), typeof(WindowsGroupPermission), new PropertyMetadata(default(bool)));
        public static readonly DependencyProperty ResourceIDProperty = DependencyProperty.Register("ResourceID", typeof(Guid), typeof(WindowsGroupPermission), new PropertyMetadata(default(Guid)));
        public static readonly DependencyProperty ResourceNameProperty = DependencyProperty.Register("ResourceName", typeof(string), typeof(WindowsGroupPermission), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty WindowsGroupProperty = DependencyProperty.Register("WindowsGroup", typeof(string), typeof(WindowsGroupPermission), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty ViewProperty = DependencyProperty.Register("View", typeof(bool), typeof(WindowsGroupPermission), new PropertyMetadata(default(bool)));
        public static readonly DependencyProperty ExecuteProperty = DependencyProperty.Register("Execute", typeof(bool), typeof(WindowsGroupPermission), new PropertyMetadata(default(bool)));
        public static readonly DependencyProperty ContributeProperty = DependencyProperty.Register("Contribute", typeof(bool), typeof(WindowsGroupPermission), new PropertyMetadata(default(bool)));
        public static readonly DependencyProperty DeployToProperty = DependencyProperty.Register("DeployTo", typeof(bool), typeof(WindowsGroupPermission), new PropertyMetadata(default(bool)));
        public static readonly DependencyProperty DeployFromProperty = DependencyProperty.Register("DeployFrom", typeof(bool), typeof(WindowsGroupPermission), new PropertyMetadata(default(bool)));
        public static readonly DependencyProperty AdministratorProperty = DependencyProperty.Register("Administrator", typeof(bool), typeof(WindowsGroupPermission), new PropertyMetadata(default(bool)));

        public bool IsServer { get { return (bool)GetValue(IsServerProperty); } set { SetValue(IsServerProperty, value); } }
        public Guid ResourceID { get { return (Guid)GetValue(ResourceIDProperty); } set { SetValue(ResourceIDProperty, value); } }
        public string ResourceName { get { return (string)GetValue(ResourceNameProperty); } set { SetValue(ResourceNameProperty, value); } }
        public string WindowsGroup { get { return (string)GetValue(WindowsGroupProperty); } set { SetValue(WindowsGroupProperty, value); } }
        public bool View { get { return (bool)GetValue(ViewProperty); } set { SetValue(ViewProperty, value); } }
        public bool Execute { get { return (bool)GetValue(ExecuteProperty); } set { SetValue(ExecuteProperty, value); } }
        public bool Contribute { get { return (bool)GetValue(ContributeProperty); } set { SetValue(ContributeProperty, value); } }
        public bool DeployTo { get { return (bool)GetValue(DeployToProperty); } set { SetValue(DeployToProperty, value); } }
        public bool DeployFrom { get { return (bool)GetValue(DeployFromProperty); } set { SetValue(DeployFromProperty, value); } }
        public bool Administrator { get { return (bool)GetValue(AdministratorProperty); } set { SetValue(AdministratorProperty, value); } }
    }
}