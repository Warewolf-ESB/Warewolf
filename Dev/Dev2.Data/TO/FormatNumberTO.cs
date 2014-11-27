
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Enums.Enums;
using Dev2.DataList.Contract;

namespace Dev2.Data.TO
{
    // ReSharper disable InconsistentNaming
    public class FormatNumberTO
    // ReSharper restore InconsistentNaming
    {
        #region Properties

        public string Number { get; set; }
        public string RoundingType { get; set; }
        public int RoundingDecimalPlaces { get; set; }
        public bool AdjustDecimalPlaces { get; set; }
        public int DecimalPlacesToShow { get; set; }

        #endregion Properties

        #region Constructor

        public FormatNumberTO()
        {
        }

        public FormatNumberTO(string number, enRoundingType roundingType, int roundingDecimalPlaces,
                              bool adjustDecimalPlaces, int decimalPlacesToShow)
        {
            Number = number;
            RoundingType = Dev2EnumConverter.ConvertEnumValueToString(roundingType);
            RoundingDecimalPlaces = roundingDecimalPlaces;
            AdjustDecimalPlaces = adjustDecimalPlaces;
            DecimalPlacesToShow = decimalPlacesToShow;
        }

        public FormatNumberTO(string number, string roundingType, int roundingDecimalPlaces, bool adjustDecimalPlaces,
                              int decimalPlacesToShow)
        {
            Number = number;
            RoundingType = roundingType;
            RoundingDecimalPlaces = roundingDecimalPlaces;
            AdjustDecimalPlaces = adjustDecimalPlaces;
            DecimalPlacesToShow = decimalPlacesToShow;
        }

        #endregion Constructor

        #region Methods

        public enRoundingType GetRoundingTypeEnum()
        {
            return (enRoundingType)Dev2EnumConverter.GetEnumFromStringDiscription(RoundingType, typeof(enRoundingType));
        }

        #endregion Methods
    }
}
