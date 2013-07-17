using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Caliburn.Micro;
using Newtonsoft.Json;

namespace Dev2.Runtime.Configuration.ComponentModel
{
    public class ComputerDrive : PropertyChangedBase
    {
        #region fields

        bool _loaded = false;
        private ObservableCollection<ComputerDrive> _children;
        private ComputerDrive _parent;
        private string _title;
        private string _fullTitle = string.Empty;

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
            get { return  _parent; }
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
            get
            {
                return _loaded;
            }
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
            TextReader textReader = new StreamReader(stream);
            var computerDrives = JsonConvert.DeserializeObject<List<ComputerDrive>>(textReader.ReadToEnd());
            return computerDrives;
        }

        public void LoadChildren()
        {
            if (Loaded)
            { return; }

            Loaded = true;
            var currentTitle = PrepareTitleForService();
            LoadDrivesOrDirectories(currentTitle);
        }

        private void LoadDrivesOrDirectories(string currentTitle)
        {
            var wc = new WebClient();
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

        private static Uri GetDirectoryUri(string currentTitle)
        {
            const string address = "http://localhost:1234/services/FindDirectoryService";
            return new Uri(address + "?DirectoryPath=" + currentTitle);
        }

        private static Uri GetDriveUri()
        {
            const string address = "http://localhost:1234/services/FindDriveService";
             return new Uri(address);
        }

        private string PrepareTitleForService()
        {
            var currentTitle = FullTitle;
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
                //TODO Handle errors
                return;
            }

            Children.Clear();
            var list = DeserializeJson(e.Result);
            foreach (var drive in list)
            {
                if (drive.Title.StartsWith("$"))
                {
                    continue;
                }

                drive.Parent = this;
                drive.FullTitle = CreateFullTitle(drive);
                Children.Add(drive);
            }
            //
            //            var wc = new WebClient();
            //            wc.OpenReadCompleted += ImageReadCompleted;
            //            wc.OpenReadAsync(new Uri(TreeViewWindow.QueryURl
            //              + "?QType=NextImgChildren&IDCat=" + IDCategory));
        }

        private string CreateFullTitle(ComputerDrive drive)
        {
            return GetDirectoryPath(drive).TrimEnd('\\','/');
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