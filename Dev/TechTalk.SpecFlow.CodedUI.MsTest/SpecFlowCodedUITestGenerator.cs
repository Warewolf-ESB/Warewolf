using System.CodeDom;
using TechTalk.SpecFlow.Generator.UnitTestProvider;
using TechTalk.SpecFlow.Utils;

namespace TechTalk.SpecFlow.CodedUI.MsTest
{
    public class SpecFlowCodedUiTestGenerator : MsTestGeneratorProvider
    {
        public SpecFlowCodedUiTestGenerator(CodeDomHelper codeDomHelper) : base(codeDomHelper)
        {
        }

        public override void SetTestClass(TechTalk.SpecFlow.Generator.TestClassGenerationContext generationContext, string featureTitle, string featureDescription)
        {
            base.SetTestClass(generationContext, featureTitle, featureDescription);

            foreach (CodeAttributeDeclaration customAttribute in generationContext.TestClass.CustomAttributes)
            {
                if (customAttribute.Name == "Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute")
                {
                    generationContext.TestClass.CustomAttributes.Remove(customAttribute);
                    break;
                }
            }

            generationContext.TestClass.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference("Microsoft.VisualStudio.TestTools.UITesting.CodedUITestAttribute")));
        }
    }
}
