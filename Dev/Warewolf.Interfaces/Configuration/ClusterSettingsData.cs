/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Newtonsoft.Json;
using Warewolf.Data;

namespace Warewolf.Configuration
{
    public class ClusterSettingsData : BindableBase, IEquatable<ClusterSettingsData>, IHasChanged
    {
        private NamedGuid _leaderServerResource = new NamedGuid();
        private string _leaderServerKey;
        private string _key;

        public ClusterSettingsData()
        {
            PropertyChanged += (sender, args) => HasChanged = true;
        }

        public string Key
        {
            get => _key;
            set => SetProperty(ref _key, value);
        }

        public NamedGuid LeaderServerResource
        {
            get => _leaderServerResource;
            set => SetProperty(ref _leaderServerResource, value);
        }

        public string LeaderServerKey
        {
            get => _leaderServerKey;
            set => SetProperty(ref _leaderServerKey, value);
        }

        [JsonIgnore]
        public bool HasChanged { get; set; }

        public bool Equals(ClusterSettingsData other)
        {
            var equals = true;
            equals &= string.Equals(Key, other.Key);

            return equals;
        }

        public ClusterSettingsData Clone()
        {
            var result = (ClusterSettingsData)MemberwiseClone();
            result.LeaderServerResource = LeaderServerResource.Clone();
            return result;
        }
    }
}
