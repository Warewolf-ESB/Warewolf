
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2.Common;
using Dev2.DataList.Contract;

namespace Dev2.DataList
{
    /// <summary>
    /// Class for the "=" recordset search option 
    /// </summary>
    public class RsOpEqual : AbstractRecsetSearchValidation
    {

        public override Func<IList<string>> BuildSearchExpression(IList<RecordSetSearchPayload> operationRange, IRecsetSearch to)
        {
            Func<IList<string>> result = () =>
                {
                    
                     
                    IList<string> fnResult = new List<string>();

                    string toFind = to.SearchCriteria.Trim();
                    string toFindLower = toFind.ToLower();

                    foreach(RecordSetSearchPayload p in operationRange)
                    {
                        string toMatch = p.Payload.Trim();

                        if(to.MatchCase)
                        {
                            if(toMatch.Equals(toFind, StringComparison.CurrentCulture))
                            {
                                fnResult.Add(p.Index.ToString(CultureInfo.InvariantCulture));
                            }
                            else
                            {
                                if(to.RequireAllFieldsToMatch)
                                {
                                    return new List<string>();
                                }
                            }
                        }
                        else
                        {
                            if(toMatch.ToLower().Equals(toFindLower, StringComparison.CurrentCulture))
                            {
                                fnResult.Add(p.Index.ToString(CultureInfo.InvariantCulture));
                            }
                            else
                            {
                                if(to.RequireAllFieldsToMatch)
                                {
                                    return new List<string>();
                                }
                            }
                        }

                    }

                    return fnResult.Distinct().ToList();
                };

            return result;
        }

        #region Overrides of AbstractRecsetSearchValidation

        public override Func<DataASTMutable.WarewolfAtom, bool> CreateFunc(IEnumerable<DataASTMutable.WarewolfAtom> values, IEnumerable<DataASTMutable.WarewolfAtom> warewolfAtoms, IEnumerable<DataASTMutable.WarewolfAtom> to, bool all)
        {
            if (all)
                return (a) => values.All(x => DataASTMutable.CompareAtoms(a,x)==0);
            return (a) => values.Any(x =>  DataASTMutable.CompareAtoms(a, x) == 0);

        }

        #endregion




        public override string HandlesType()
        {
            return "=";
        }
    }
}
