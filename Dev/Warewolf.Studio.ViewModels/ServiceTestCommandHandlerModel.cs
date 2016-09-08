using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.Data;
using Dev2.Studio.Core.Interfaces;

namespace Warewolf.Studio.ViewModels
{
    public sealed class ServiceTestCommandHandlerModel : IServiceTestCommandHandler
    {
        private readonly DataListConversionUtils _dataListConversionUtils;

        public ServiceTestCommandHandlerModel()
        {
            DataList = new DataListModel();
            _dataListConversionUtils = new DataListConversionUtils();
        }

        public DataListModel DataList { get; set; }

        public IServiceTestModel CreateTest(IResourceModel resourceModel)
        {
            var testModel = new ServiceTestModel
            {
                TestName = "Test 1",
                IsDirty = true,
                TestPending =  true,
                Enabled = true
            };
            if (!string.IsNullOrEmpty(resourceModel.DataList))
            {

                DataList.Create(resourceModel.DataList,resourceModel.DataList);
                var inputList = _dataListConversionUtils.GetInputs(DataList);
                var outputList = _dataListConversionUtils.GetOutputs(DataList);
                testModel.Inputs = inputList
                                    .Select(sca =>
                    {
                        var serviceTestInput = new ServiceTestInput(sca.DisplayValue, sca.Value);
                        serviceTestInput.AddNewAction = () => testModel.AddRow(serviceTestInput, DataList);
                        return serviceTestInput as IServiceTestInput;
                    }).ToList();

                testModel.Outputs =  outputList
                                    .Select(sca => new ServiceTestOutput(sca.DisplayValue, sca.Value) as IServiceTestOutput).ToList();
            }
            return testModel;
       }


        public void StopTest()
        {
        }

      

        public void RunAllTestsInBrowser()
        {
        }

        

        public void RunAllTestsCommand()
        {
        }
        

        public void RunSelectedTestInBrowser()
        {
        }

        

        public void RunSelectedTest()
        {
        }

        

        public void DuplicateTest()
        {
        }

       

        public void DeleteTest()
        {
        }        

    }
}