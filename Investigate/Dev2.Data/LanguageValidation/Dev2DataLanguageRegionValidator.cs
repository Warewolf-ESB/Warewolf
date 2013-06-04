using System.Collections.Generic;
using System.Text.RegularExpressions;
using Dev2.DataList.Contract;

namespace Dev2.Data.LanguageValidation
{
    /// <summary>
    /// Used to scrub results in a standardized manor ;)
    /// </summary>
    public class Dev2DataLanguageRegionValidator
    {

        private static Regex _alphaPattern = new Regex(@"^[a-zA-Z0-9_-]*$");

        /// <summary>
        /// Scrubs the intellisense results.
        /// </summary>
        /// <param name="toProcess">To process.</param>
        /// <returns></returns>
        public static IList<IIntellisenseResult> ScrubIntellisenseResults(IList<IIntellisenseResult> toProcess, string payload)
        {
            // Check for empty region
            IList<IIntellisenseResult> result = new List<IIntellisenseResult>();

            IIntellisenseResult empty = CheckEmptyRegion(payload);
            if(empty != null)
            {
                result.Add(empty);
            }
            else
            {
                result = CheckForInvalidChars(toProcess);    
            }
            
            
            

            

            return result;
        }

        #region Private Methods

        /// <summary>
        /// Checks for invalid chars.
        /// </summary>
        /// <param name="toProcess">To process.</param>
        /// <returns></returns>
        private static IList<IIntellisenseResult> CheckForInvalidChars(IEnumerable<IIntellisenseResult> toProcess)
        {
            IList<IIntellisenseResult> result = new List<IIntellisenseResult>();

            foreach(IIntellisenseResult opt in toProcess)
            {
                string tmp = opt.Option.DisplayValue.Replace("[", "").Replace("]", "").Replace("().", ""); // Remove [[ ]] and (). for correct detection
                if(!_alphaPattern.IsMatch(tmp))
                {

                    result.Add(BuildError(opt, opt.Option.DisplayValue + " contains invalid characters"));
                }
                else
                {
                    result.Add(opt); // all good ;)
                }
            }

            return result;

        }

        /// <summary>
        /// Checks for an empty region.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        private static IIntellisenseResult CheckEmptyRegion(string payload)
        {
            IIntellisenseResult result = null;

            if(payload.Replace(" ", "").Length == 4)
            {

                IDataListVerifyPart p = IntellisenseFactory.CreateDataListValidationScalarPart("Empty Region");
                result = IntellisenseFactory.CreateErrorResult(1, 1, p, "Empty DataList region", enIntellisenseErrorCode.SyntaxError, true);
            }


            return result;
        }


        /// <summary>
        /// Builds the error.
        /// </summary>
        /// <param name="opt">The opt.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        private static IIntellisenseResult BuildError(IIntellisenseResult opt, string message)
        {
            // naughty match
            IDataListVerifyPart p;
            if (!opt.Option.IsScalar)
            {
                p = IntellisenseFactory.CreateDataListValidationRecordsetPart(opt.Option.Recordset, opt.Option.Field, opt.Option.Description, opt.Option.RecordsetIndex);
            }
            else
            {
                p = IntellisenseFactory.CreateDataListValidationScalarPart(opt.Option.Field, opt.Option.Description);
            }

           return ( IntellisenseFactory.CreateErrorResult(opt.StartIndex, opt.EndIndex, p, message, enIntellisenseErrorCode.SyntaxError, opt.IsClosedRegion) );
        }

        #endregion

    }
}
