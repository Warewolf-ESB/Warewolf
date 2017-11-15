/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;

namespace Dev2
{
    public class ObservablePair<TKey, TValue> : ObservableObject
    {
        TKey _key;
        TValue _value;

        public ObservablePair()
        {
        }

        public ObservablePair(TKey key, TValue value)
        {
            _key = key;
            _value = value;
        }

        public TKey Key { get => _key; set => OnPropertyChanged(ref _key, value); }

        public TValue Value { get => _value; set => OnPropertyChanged(ref _value, value); }
    }
}
