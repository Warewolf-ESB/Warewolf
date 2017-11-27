using System;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;

namespace Dev2.ViewModels.Merge
{
    public class ArmConnectorConflict : BindableBase, IArmConnectorConflict
    {
        bool _hasConflict;
        bool _isMergeExpanderEnabled;
        public IMergeArmConnectorConflict CurrentArmConnector { get; set; }
        public IMergeArmConnectorConflict DifferentArmConnector { get; set; }
        public bool HasConflict
        {
            get => _hasConflict;
            set
            {
                _hasConflict = value;
                OnPropertyChanged(() => HasConflict);
            }
        }
        public bool IsChecked { get; set; }
        public Guid UniqueId { get; set; }
        public string Key { get; set; }
        public bool IsMergeExpanderEnabled
        {
            get => _isMergeExpanderEnabled;
            set
            {
                _isMergeExpanderEnabled = value;
                OnPropertyChanged(() => IsMergeExpanderEnabled);
            }
        }

        public bool Equals(IArmConnectorConflict other)
        {
            if (other == null)
            {
                return false;
            }
            var equals = true;
            equals &= (other.CurrentArmConnector?.Equals(CurrentArmConnector)).GetValueOrDefault(false);
            equals &= (other.DifferentArmConnector?.Equals(DifferentArmConnector)).GetValueOrDefault(false);
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
            return Equals((IArmConnectorConflict)obj);
        }

        public override int GetHashCode()
        {
            var hashCode = (397) ^ UniqueId.GetHashCode();
            hashCode = (hashCode * 397) ^ (Key != null ? Key.GetHashCode() : 0);
            return hashCode;
        }
    }
}
