/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Warewolf.Options;

namespace Warewolf.Data
{
    public class WorkflowWithInputs : BindableBase, IWorkflow, IEquatable<WorkflowWithInputs>, INotifyPropertyChanged
    {
        protected string _name;
        protected Guid _value;
        protected ICollection<IServiceInputBase> _inputs;

        public WorkflowWithInputs()
        {
            Name = "";
            Value = Guid.Empty;
            Inputs = null;
        }

        public WorkflowWithInputs(string name, Guid value, ICollection<IServiceInputBase> inputs)
        {
            Value = value;
            Name = name;
            Inputs = inputs;
        }

        public Guid Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public ICollection<IServiceInputBase> Inputs
        {
            get => _inputs;
            set => SetProperty(ref _inputs, value);
        }

        public override string ToString() => Name;

        public static bool operator ==(WorkflowWithInputs left, WorkflowWithInputs right) => Equals(left, right);
        public static bool operator !=(WorkflowWithInputs left, WorkflowWithInputs right) => !Equals(left, right);

        public bool Equals(WorkflowWithInputs other)
        {
            if (other is null)
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(_name, other._name) && string.Equals(_value, other._value);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((WorkflowWithInputs)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_name?.GetHashCode() ?? 0) * 397) ^ (_value.GetHashCode());
            }
        }
    }
}
