#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
using System.Collections.Generic;
using System.Text;

namespace Dev2.Data.Interfaces
{
    public interface IParserHelper
    {
        bool ProcessOpenRegion(string payload, bool openRegion, int i, ref IParseTO currentNode, ref StringBuilder region, ref char cur);
        IParseTO CurrentNode(IParseTO currentNode, StringBuilder region, int i);
        bool ShouldAddToRegion(string payload, char cur, char prev, int i, bool shouldAddToRegion, char charToCheck);
        bool IsValidIndex(IParseTO to);
        IIntellisenseResult AddErrorToResults(bool isRs, string part, IDev2DataLangaugeParseError dev2DataLanguageParseError, bool isOpen);
        bool CheckValidIndex(IParseTO to, string part, int start, int end);
        bool CheckCurrentIndex(IParseTO to, int start, string raw, int end);
        IIntellisenseResult ValidateName(string name, string displayString);
        bool ProcessFieldsForRecordSet(IParseTO payload, bool addCompleteParts, IList<IIntellisenseResult> result, string[] parts, out string search, out bool emptyOk, string display, IDev2DataLanguageIntellisensePart recordsetPart, string partName);
        void ProcessResults(IList<IIntellisenseResult> realResults, IIntellisenseResult intellisenseResult);
        bool ValidateName(string rawSearch, string displayString, IList<IIntellisenseResult> result, out IList<IIntellisenseResult> intellisenseResults);
        bool AddFieldResult(IParseTO payload, IList<IIntellisenseResult> result, string tmpString, string[] parts, bool isRs);
    }
}