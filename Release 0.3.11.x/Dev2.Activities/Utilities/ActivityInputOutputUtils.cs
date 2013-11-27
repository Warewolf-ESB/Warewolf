using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract;

namespace Unlimited.Applications.BusinessDesignStudio.Activities.Utilities
{
    public static class ActivityInputOutputUtils
    {
        #region Methods

        #region Get Inputs/Outputs

        /// <summary>
        /// Gets the inputs.
        /// </summary>
        /// <param name="obj">The obj to get inputs for.</param>
        /// <returns></returns>
        public static IBinaryDataList GetSimpleInputs(object obj)
        {
            string error = string.Empty;
            IBinaryDataList binaryDL = Dev2BinaryDataListFactory.CreateDataList();
            Type sourceType = obj.GetType();
            foreach (PropertyInfo pi in sourceType.GetProperties())
            {
                List<Inputs> listOfInputs = pi.GetCustomAttributes(typeof(Inputs), true).OfType<Inputs>().ToList();
                if (listOfInputs.Count > 0)
                {
                    if (binaryDL.TryCreateScalarTemplate(string.Empty, listOfInputs[0].UserVisibleName, "", true, out error))
                    {
                        binaryDL.TryCreateScalarValue(pi.GetValue(obj, null)!=null? pi.GetValue(obj, null).ToString() : string.Empty, listOfInputs[0].UserVisibleName, out error);
                    }
                }
            }
            return binaryDL;
        }

        /// <summary>
        /// Gets the outputs.
        /// </summary>
        /// <param name="obj">The obj to get outputs for.</param>
        /// <returns></returns>
        public static IBinaryDataList GetSimpleOutputs(object obj)
        {
            string error = string.Empty;
            IBinaryDataList binaryDL = Dev2BinaryDataListFactory.CreateDataList();
            Type sourceType = obj.GetType();
            foreach (PropertyInfo pi in sourceType.GetProperties())
            {
                List<Outputs> listOfInputs = pi.GetCustomAttributes(typeof(Outputs), true).OfType<Outputs>().ToList();
                if (listOfInputs.Count > 0)
                {
                    if (binaryDL.TryCreateScalarTemplate(string.Empty, listOfInputs[0].UserVisibleName, "", true, out error))
                    {
                        binaryDL.TryCreateScalarValue(pi.GetValue(obj, null).ToString(), listOfInputs[0].UserVisibleName, out error);
                    }
                }
            }
            return binaryDL;
        }

        public static IList<IDev2Definition> GetComplexInputs()
        {
            IList<IDev2Definition> result = new List<IDev2Definition>();

            return result;
        }

        #endregion Get Inputs/Outputs

        #region Set Values

        /// <summary>
        /// Sets the outputs.
        /// </summary>
        /// <param name="obj">The obj to set outputs for.</param>
        /// <param name="datalistID">The datalistID.</param>
        public static void SetValues<T>(ModelItem obj, Guid datalistID, IDataListCompiler compiler) where T : IActivityPropertyAttribute
        {
            string _dataListValue = string.Empty;
            Type sourceType = obj.ItemType;
            foreach (PropertyInfo pi in sourceType.GetProperties())
            {
                List<T> listOfInputs = pi.GetCustomAttributes(typeof(T), true).OfType<T>().ToList();
                if (listOfInputs.Count > 0)
                {
                    _dataListValue = GetValueFromDataList(listOfInputs[0].UserVisibleName, datalistID, compiler);

                    ModelProperty mp = obj.Properties.Find(pi.Name);
                    Type t = mp.PropertyType;
                    if (mp != null)
                    {
                        mp.SetValue(Convert.ChangeType(_dataListValue, t));
                    }
                }
            }
        }

        #endregion Set Values

        #region Get General Settings

        /// <summary>
        /// Gets the General Setting for an activity.
        /// </summary>
        /// <param name="obj">The obj to get settings for.</param>
        /// <returns></returns>
        public static IBinaryDataList GetGeneralSettings(object obj)
        {
            string error = string.Empty;
            IBinaryDataList binaryDL = Dev2BinaryDataListFactory.CreateDataList();
            Type sourceType = obj.GetType();
            foreach (PropertyInfo pi in sourceType.GetProperties())
            {
                List<GeneralSettings> listOfGeneralSettings = pi.GetCustomAttributes(typeof(GeneralSettings), true).OfType<GeneralSettings>().ToList();
                if (listOfGeneralSettings.Count > 0)
                {
                    if (binaryDL.TryCreateScalarTemplate(string.Empty, listOfGeneralSettings[0].UserVisibleName, "", true, out error))
                    {
                        binaryDL.TryCreateScalarValue(pi.GetValue(obj, null).ToString(), listOfGeneralSettings[0].UserVisibleName, out error);
                    }
                }
            }
            return binaryDL;
        }

        #endregion Get General Settings

        #endregion Methods

        #region Private Methods

        /// <summary>
        /// Gets a value from data list for a expression.
        /// </summary>
        /// <param name="expression">The expression to find the value for.</param>
        /// <param name="datalist">The ID of the datalist that the value must be extracted from.</param>       
        private static string GetValueFromDataList(string expression, Guid datalistID, IDataListCompiler compiler)
        {
            string result = string.Empty;
            expression = DataListUtil.AddBracketsToValueIfNotExist(expression);
            ErrorResultTO errors;          

            IBinaryDataListEntry returnedEntry = compiler.Evaluate(datalistID, enActionType.User, expression, false, out errors);
            if (returnedEntry == null)
            {
                throw new Exception(errors.MakeUserReady());
            }
            else
            {
                IBinaryDataListItem item = returnedEntry.FetchScalar();
                result = item.TheValue;
            }
            return result;
        }

        #endregion Private Methods
    }
}
