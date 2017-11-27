using System;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;

namespace Dev2.ViewModels.Merge
{

    public class MergeArmConnectorConflict : BindableBase, IMergeArmConnectorConflict
    {
        public string ArmDescription { get; set; }
        public string SourceUniqueId { get; set; }
        public string DestinationUniqueId { get; set; }
        public IArmConnectorConflict Container { get; set; }
        public string Key { get; set; }
        bool _isChecked;
        bool _isArmSelectionAllowed;

        public event Action<IArmConnectorConflict,bool, string,string,string> OnChecked;

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                OnPropertyChanged(() => IsChecked);
                OnChecked?.Invoke(Container,_isChecked, SourceUniqueId,DestinationUniqueId,Key);
            }
        }

        public bool IsArmSelectionAllowed
        {
            get => _isArmSelectionAllowed;
            set
            {
                _isArmSelectionAllowed = value;
                OnPropertyChanged(() => IsArmSelectionAllowed);
            }
        }

        public MergeArmConnectorConflict(IArmConnectorConflict container)
        {
            Container = container;
        }
        public MergeArmConnectorConflict(string armDescription, string sourceUniqueId, string destinationUniqueId,string key, IArmConnectorConflict container)
        {
            ArmDescription = armDescription;
            SourceUniqueId = sourceUniqueId;
            DestinationUniqueId = destinationUniqueId;
            Key = key;
            Container = container;
        }

        public bool Equals(IMergeArmConnectorConflict other)
        {
            if (other == null)
            {
                return false;
            }
            var equals = true;
            equals &= other.SourceUniqueId == SourceUniqueId;
            equals &= other.DestinationUniqueId == DestinationUniqueId;
            equals &= other.Key == Key;
            return equals;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((IMergeArmConnectorConflict)obj);
        }

        public override int GetHashCode()
        {
            var hashCode = (397) ^ SourceUniqueId.GetHashCode();
            hashCode = (hashCode * 397) ^ (DestinationUniqueId != null ? DestinationUniqueId.GetHashCode() : 0);
            return hashCode;
        }
    }
}
