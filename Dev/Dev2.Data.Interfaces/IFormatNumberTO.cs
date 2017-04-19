using Dev2.Data.Interfaces.Enums;

namespace Dev2.Data.Interfaces
{
    public interface IFormatNumberTO
    {
        string Number { get; set; }
        string RoundingType { get; set; }
        int RoundingDecimalPlaces { get; set; }
        bool AdjustDecimalPlaces { get; set; }
        int DecimalPlacesToShow { get; set; }

        enRoundingType GetRoundingTypeEnum();
    }
}