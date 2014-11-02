/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using Caliburn.Micro;
using Dev2.Util;
using Newtonsoft.Json;

namespace Dev2.Runtime.Configuration.ComponentModel
{
    public class ComputerDrive : PropertyChangedBase
    {
        #region fields

        private ObservableCollection<ComputerDrive> _children;
        private string _fullTitle = string.Empty;
        private bool _loaded;
        private ComputerDrive _parent;
        private string _title;

        #endregion fields

        #region properties

        public string Title
        {
            get { return _title; }
            set
            {
                if (_title == value)
                {
                    return;
                }

                _title = value;
                NotifyOfPropertyChange(() => Title);
                NotifyOfPropertyChange(() => FullTitle);
            }
        }

        public string FullTitle
        {
            get { return _fullTitle; }
            set
            {
                if (_fullTitle == value)
                {
                    return;
                }

                _fullTitle = value;
                NotifyOfPropertyChange(() => FullTitle);
            }
        }

        public int ChildrenCount
        {
            get { return Children.Count; }
        }

        public ObservableCollection<ComputerDrive> Children
        {
            get
            {
                if (_children == null)
                {
                    _children = new ObservableCollection<ComputerDrive>();
                    _children.CollectionChanged += (s, e) => NotifyOfPropertyChange(() => ChildrenCount);
                }
                return _children;
            }
        }

        public ComputerDrive Parent
        {
            get { return _parent; }
            set
            {
                if (_parent == value)
                {
                    return;
                }
                _parent = value;
                NotifyOfPropertyChange(() => Parent);
            }
        }

        public bool Loaded
        {
            get { return _loaded; }
            set
            {
                _loaded = value;
                NotifyOfPropertyChange(() => Loaded);
            }
        }

        #endregion properties

        #region public methods

        public static List<ComputerDrive> DeserializeJson(Stream stream)
        {
            using (TextReader textReader = new StreamReader(stream))
            {
                var computerDrives = JsonConvert.DeserializeObject<List<ComputerDrive>>(textReader.ReadToEnd());
                return computerDrives;
            }
        }

        public void LoadChildren()
        {
            if (Loaded)
            {
                return;
            }

            Loaded = true;
            string currentTitle = PrepareTitleForService();
            LoadDrivesOrDirectories(currentTitle);
        }

        private void LoadDrivesOrDirectories(string currentTitle)
        {
            using (var wc = new WebClient())
            {
                wc.OpenReadCompleted += ChildrenReadCompleted;
                Uri webUri;
                if (string.IsNullOrEmpty(currentTitle) || currentTitle.Equals("/"))
                {
                    webUri = GetDriveUri();
                }
                else
                {
                    webUri = GetDirectoryUri(currentTitle);
                }
                wc.OpenReadAsync(webUri);
            }
        }

        private static Uri GetDirectoryUri(string currentTitle)
        {
            string address = AppSettings.LocalHost + "/services/FindDirectoryService";
            return new Uri(address + "?DirectoryPath=" + currentTitle);
        }

        private static Uri GetDriveUri()
        {
            string address = AppSettings.LocalHost + "/services/FindDriveService";
            return new Uri(address);
        }

        private string PrepareTitleForService()
        {
            string currentTitle = FullTitle;
            if (!currentTitle.EndsWith("\\"))
            {
                currentTitle += "/";
            }
            currentTitle = currentTitle.Replace("\\", "/");
            return currentTitle;
        }

        #endregion public methods

        #region private methods

        private void ChildrenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            if (e.Error != null || e.Cancelled)
            {
                return;
            }

            Children.Clear();
            List<ComputerDrive> list = DeserializeJson(e.Result);
            foreach (ComputerDrive drive in list)
            {
                if (drive.Title.StartsWith("$"))
                {
                    continue;
                }

                drive.Parent = this;
                drive.FullTitle = CreateFullTitle(drive);
                Children.Add(drive);
            }
        }

        private string CreateFullTitle(ComputerDrive drive)
        {
            return GetDirectoryPath(drive).TrimEnd('\\', '/');
        }

        private string GetDirectoryPath(ComputerDrive drive)
        {
            if (drive.Parent == null || string.IsNullOrWhiteSpace(drive.Parent.Title))
                return drive.Title;

            return drive.Title.Insert(0, CreateFullTitle(drive.Parent) + "\\");
        }

        #endregion private methods
    }
}