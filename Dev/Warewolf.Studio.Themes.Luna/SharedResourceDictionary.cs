using System;
using System.Collections.Generic;
using System.Windows;

namespace Warewolf.Studio.Themes.Luna
{
    public class SharedResourceDictionary : ResourceDictionary
    {
        private Uri _sourceUri;

        public static Dictionary<Uri, ResourceDictionary> SharedDictionaries { get => sharedDictionaries; set => sharedDictionaries = value; }
        private static Dictionary<Uri, ResourceDictionary> sharedDictionaries = new Dictionary<Uri, ResourceDictionary>();

        public new Uri Source
        {
            get { return _sourceUri; }
            set
            {
                _sourceUri = value;

                if (!SharedDictionaries.ContainsKey(value))
                {
                    base.Source = value;
                    SharedDictionaries.Add(value, this);
                }
                else
                {
                    MergedDictionaries.Add(SharedDictionaries[value]);
                }
            }
        }
    }
}
