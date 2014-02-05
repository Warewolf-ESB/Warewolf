using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Xml.Linq;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Email;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Activities.Designers.Tests.Email
{
    public class TestEmailDesignerViewModel : EmailDesignerViewModel
    {
        public TestEmailDesignerViewModel(ModelItem modelItem, IEventAggregator eventPublisher)
            : base(modelItem, eventPublisher)
        {
        }

        public override List<EmailSource> GetSources(IEnvironmentModel environmentModel)
        {

            var list = new List<EmailSource>();
            var emailSource1 = new EmailSource(XElement.Parse(@"<Source ID=""6f6fda2f-4060-4c06-bfbb-7a38a1088604"" Version=""1.0"" Name=""MoEmailTest1"" ResourceType=""EmailSource"" ConnectionString=""Host=smtp.mail.yahoo.com;UserName=dev2developer@yahoo.com;Password=Q38qrDmsi36ei1R;Port=25;EnableSsl=False;Timeout=100000"" Type=""EmailSource"" ServerID=""51a58300-7e9d-4927-a57b-e5d700b11b55"">
  <DisplayName>MoEmailTest1</DisplayName>
  <Category>MoEmailTestSources</Category>
  <AuthorRoles></AuthorRoles>
  <TypeOf>EmailSource</TypeOf>
  <Signature xmlns=""http://www.w3.org/2000/09/xmldsig#"">
    <SignedInfo>
      <CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315"" />
      <SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1"" />
      <Reference URI="""">
        <Transforms>
          <Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" />
        </Transforms>
        <DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1"" />
        <DigestValue>mnY5GQkRM6RQpvYYx6p40rmDrWw=</DigestValue>
      </Reference>
    </SignedInfo>
    <SignatureValue>pMuNyT8uHzb3C++wGl058QdOdRNSxKcTKm7MeqwtRkuiLM9we0EItMyzAEJ/8ZyxZCDUi3GoosCTsND8X+HJh1EmHj6AZJ59PIX+ypZK4d+IGlqSrS29oeVZ1tIKOxIrVxc1HaGfRRibh33mxW/OWFxl7JeQQnLVlQ17SGsTl2s=</SignatureValue>
  </Signature>
</Source>"));
            var emailSource2 = new EmailSource(XElement.Parse(@"<Source ID=""8f803242-a0cf-45d1-8449-d4dab2662718"" Version=""1.0"" Name=""MoEmailTest2"" ResourceType=""EmailSource"" ConnectionString=""Host=smtp.mail.yahoo.com;UserName=dev2developer@yahoo.com;Password=Q38qrDmsi36ei1R;Port=25;EnableSsl=False;Timeout=100000"" Type=""EmailSource"" ServerID=""51a58300-7e9d-4927-a57b-e5d700b11b55"">
  <DisplayName>MoEmailTest2</DisplayName>
  <Category>MoEmailTestSources</Category>
  <AuthorRoles></AuthorRoles>
  <TypeOf>EmailSource</TypeOf>
  <Signature xmlns=""http://www.w3.org/2000/09/xmldsig#"">
    <SignedInfo>
      <CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315"" />
      <SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1"" />
      <Reference URI="""">
        <Transforms>
          <Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" />
        </Transforms>
        <DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1"" />
        <DigestValue>epBzm31clckPtICoVs0f5PL36qs=</DigestValue>
      </Reference>
    </SignedInfo>
    <SignatureValue>Vn6taCw3fFFL/NyuERfgHw0X9+JgeRAkmVWbtDmrhB6LsLB07o/rQXtf/FUJrKa9jDiiyS6G6WVhVYJ/zo3cBXZq6sBzOdYxPQHVtjf322p22rvCRaY3zw2hhZArvyc26YxpX8vjdIYmxPCJ6tbn5Hpg4ftB+ciHTo+MYS7zHN0=</SignatureValue>
  </Signature>
</Source>"));
            list.Add(emailSource1);
            list.Add(emailSource2);
            return list;
        }

        public EmailSource SelectedEmailSource { get { return GetProperty<EmailSource>(); } set { SetProperty(value); } }
        public string FromAccount { set { SetProperty(value); } get { return GetProperty<string>(); } }
    }
}