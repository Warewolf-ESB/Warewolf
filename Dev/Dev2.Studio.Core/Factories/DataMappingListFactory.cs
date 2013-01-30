using System.Collections.Generic;
using Dev2.Studio.Core.Interfaces;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Xml;
using Dev2.Studio.Core.ViewModels;
using System.ComponentModel;
using System;
using Dev2.DataList.Contract;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Models.DataList;

namespace Dev2.Studio.Core.Factories {    
    public static class DataMappingListFactory
    {

        public static IList<IDev2Definition> CreateListOutputMapping(string xmlServiceDefintion)
        {
            IList<IDev2Definition> outputDef = DataListFactory.CreateOutputParser().ParseAndAllowBlanks(xmlServiceDefintion);
            return outputDef;
        }

        public static IList<IDev2Definition> CreateListInputMapping(string xmlServiceDefintion)
        {
            IList<IDev2Definition> inputDef = DataListFactory.CreateInputParser().ParseAndAllowBlanks(xmlServiceDefintion);
            return inputDef;
        }

        public static IRecordSetCollection CreateListOutputRecordSetMapping(string xmlServiceDefintion)
        {
            IList<IDev2Definition> outputDef = DataListFactory.CreateOutputParser().Parse(xmlServiceDefintion);
            IRecordSetCollection outputRecSet = DataListFactory.CreateRecordSetCollection(outputDef);
            return outputRecSet;
        }

        public static IRecordSetCollection CreateListInputRecordSetMapping(string xmlServiceDefintion)
        {
            IList<IDev2Definition> inputDef = DataListFactory.CreateInputParser().ParseAndAllowBlanks(xmlServiceDefintion);
            IRecordSetCollection inputRecSet = DataListFactory.CreateRecordSetCollection(inputDef);
            return inputRecSet;
        }       
       
        public static IList<IInputOutputViewModel> CreateListToDisplayOutputs(IList<IDev2Definition> outputList)
        {
            IList<IInputOutputViewModel> _displayOutputData = new List<IInputOutputViewModel>();            
            foreach (IDev2Definition otp in outputList) {
                IInputOutputViewModel inputOutputViewModel = InputOutputViewModelFactory.CreateInputOutputViewModel(otp.Name, otp.RawValue, otp.MapsTo, otp.DefaultValue, otp.IsRequired, otp.RecordSetName);               
                _displayOutputData.Add(inputOutputViewModel);
            }            
            return _displayOutputData;
        }

        public static IList<IInputOutputViewModel> CreateListToDisplayInputs(IList<IDev2Definition> inputList)
        {
            IList<IInputOutputViewModel> _displayInputData = new List<IInputOutputViewModel>();            
            foreach (IDev2Definition itp in inputList) {
                IInputOutputViewModel inputOutputViewModel = InputOutputViewModelFactory.CreateInputOutputViewModel(itp.Name, itp.RawValue, itp.RawValue, itp.DefaultValue, itp.IsRequired, itp.RecordSetName, itp.EmptyToNull);                                
                _displayInputData.Add(inputOutputViewModel);
            }
            return _displayInputData;
        }

        public static string GenerateMapping(IList<IDev2Definition> defs, enDev2ArgumentType typeOf)
        {
            return DataListFactory.GenerateMapping(defs, typeOf);
        }

    }
}
