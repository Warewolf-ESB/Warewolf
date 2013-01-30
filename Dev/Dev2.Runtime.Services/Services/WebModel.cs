using System;
using System.Linq;
using System.Reflection;
using Dev2.Common;
using Dev2.Data.SystemTemplates.Models;
using Dev2.DataList.Contract;
using Dev2.Runtime.Diagnostics;

namespace Dev2.Runtime.Services
{
    /// <summary>
    /// A model class for pushing and pulling JSON serialized objects to and from Web into the Server
    /// </summary>
    public class WebModel : ExceptionManager
    {

        #region Model Save

        /// <summary>
        /// Saves the JSON model.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <param name="dataListID">The data list ID.</param>
        /// <returns></returns>
        public string SaveModel(string args, Guid workspaceID, Guid dataListID)
        {
            string result = "{ \"message\" : \"Error Saving Model\"} ";

            if (dataListID != GlobalConstants.NullDataListID)
            {
                IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

                ErrorResultTO errors = new ErrorResultTO();

                compiler.UpsertSystemTag(dataListID, enSystemTag.SystemModel, args, out errors);

                result = "{  \"message\" : \"Saved Model\"} ";
            }

            return result;
        }

        #endregion 


        #region Model Fetch
        /// <summary>
        /// Fetches the decision model.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <param name="dataListID">The data list ID.</param>
        /// <returns></returns>
        public string FetchDecisionModel(string args, Guid workspaceID, Guid dataListID)
        {

            if (dataListID != GlobalConstants.NullDataListID)
            {
                IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

                ErrorResultTO errors = new ErrorResultTO();

                // System.Activator.CreateInstance(Type.GetType(className))

                //object obj = System.Activator.CreateInstance(Type.GetType("abc"));


                /*Type typeFromString = Type.GetType(typeof(Dev2DecisionStack).FullName);

                object[] paremeters = new object[]
                {
                    dataListID, 
                    null,
                };
                MethodInfo mi = compiler.GetType().GetMethods().Where(m => m.IsGenericMethod && m.Name == "FetchSystemModelAsWebModel" && m.GetParameters().Length == 2).First().MakeGenericMethod(typeFromString);
                string result = mi.Invoke(compiler, paremeters) as string;

                errors = paremeters[1] as ErrorResultTO;*/

                string result = compiler.FetchSystemModelAsWebModel<Dev2DecisionStack>(dataListID, out errors);

                if (errors.HasErrors())
                {
                    compiler.UpsertSystemTag(dataListID, enSystemTag.Error, errors.MakeDataListReady(), out errors);
                }

                return result;
            }

            return "{}"; // empty model
        }

        /// <summary>
        /// Fetches the switch expression.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <param name="dataListID">The data list ID.</param>
        /// <returns></returns>
        public string FetchSwitchExpression(string args, Guid workspaceID, Guid dataListID)
        {
            if (dataListID != GlobalConstants.NullDataListID)
            {
                IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

                ErrorResultTO errors = new ErrorResultTO();

                string result = compiler.FetchSystemModelAsWebModel<Dev2Switch>(dataListID, out errors);

                if (errors.HasErrors())
                {
                    compiler.UpsertSystemTag(dataListID, enSystemTag.Error, errors.MakeDataListReady(), out errors);
                }

                return result;
            }

            return "{}"; // empty model
        }

        /// <summary>
        /// Fetches the switch case.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <param name="dataListID">The data list ID.</param>
        /// <returns></returns>
        public string FetchSwitchCase(string args, Guid workspaceID, Guid dataListID)
        {

            if (dataListID != GlobalConstants.NullDataListID)
            {
                IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

                ErrorResultTO errors = new ErrorResultTO();

                string result = compiler.FetchSystemModelAsWebModel<Dev2SwitchCase>(dataListID, out errors);

                if (errors.HasErrors())
                {
                    compiler.UpsertSystemTag(dataListID, enSystemTag.Error, errors.MakeDataListReady(), out errors);
                }

                return result;
            }

            return "{}"; // empty model
        }

        #endregion
    }
}
