
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.ComponentModel;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.TO
{
    /// <summary>
    /// Provides information on deploy statistics in a bindable 
    /// </summary>
    public class DeployStatsTO : INotifyPropertyChanged
    {
        #region Events

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion PropertyChanged

        #endregion Events

        #region Class Members

        private string _name;
        private string _description;

        #endregion Class Members

        #region Constructors

        public DeployStatsTO()
        {
        }

        public DeployStatsTO(string name, string description)
        {
            Name = name;
            Description = description;
        }

        #endregion Constructors

        #region Properties

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
                OnPropertyChanged("Description");
            }
        }

        #endregion Properties
    }
}
