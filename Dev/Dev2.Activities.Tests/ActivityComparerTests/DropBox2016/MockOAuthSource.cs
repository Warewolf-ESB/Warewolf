using Dev2.Common.Interfaces;
using Dev2.Data.ServiceModel;
using System;

namespace Dev2.Tests.Activities.ActivityComparerTests.DropBox2016
{
    class MockOAuthSource : OauthSource
    {
        public MockOAuthSource(Guid id)
        {
            ResourceID = id;
        }
        public override string AppKey { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override string AccessToken { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override bool Equals(IOAuthSource other)
        {
            throw new NotImplementedException();
        }

        protected override string GetConnectionString()
        {
            throw new NotImplementedException();
        }
    }
}
