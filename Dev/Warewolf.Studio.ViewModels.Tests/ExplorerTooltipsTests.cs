using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ExplorerTooltipsTests
    {
        private ExplorerTooltips _target;

        [TestInitialize]
        public void TestInitialize()
        {
            _target = new ExplorerTooltips();
        }

        [TestMethod]
        public void TestNewServiceTooltip()
        {
            _target.NewServiceTooltip = Resources.Languages.Tooltips.NoPermissionsToolTip;
            Assert.AreEqual(Resources.Languages.Tooltips.NoPermissionsToolTip, _target.NewServiceTooltip);
        }

        [TestMethod]
        public void TestNewServerSourceTooltip()
        {
            _target.NewServerSourceTooltip = Resources.Languages.Tooltips.NoPermissionsToolTip;
            Assert.AreEqual(Resources.Languages.Tooltips.NoPermissionsToolTip, _target.NewServerSourceTooltip);
        }

        [TestMethod]
        public void TestNewSqlServerSourceTooltip()
        {
            _target.NewSqlServerSourceTooltip = Resources.Languages.Tooltips.NoPermissionsToolTip;
            Assert.AreEqual(Resources.Languages.Tooltips.NoPermissionsToolTip, _target.NewSqlServerSourceTooltip);
        }

        [TestMethod]
        public void TestNewMySqlSourceTooltip()
        {
            _target.NewMySqlSourceTooltip = Resources.Languages.Tooltips.NoPermissionsToolTip;
            Assert.AreEqual(Resources.Languages.Tooltips.NoPermissionsToolTip, _target.NewMySqlSourceTooltip);
        }

        [TestMethod]
        public void TestNewPostgreSqlSourceTooltip()
        {
            _target.NewPostgreSqlSourceTooltip = Resources.Languages.Tooltips.NoPermissionsToolTip;
            Assert.AreEqual(Resources.Languages.Tooltips.NoPermissionsToolTip, _target.NewPostgreSqlSourceTooltip);
        }

        [TestMethod]
        public void TestNewOracleSourceTooltip()
        {
            _target.NewOracleSourceTooltip = Resources.Languages.Tooltips.NoPermissionsToolTip;
            Assert.AreEqual(Resources.Languages.Tooltips.NoPermissionsToolTip, _target.NewOracleSourceTooltip);
        }

        [TestMethod]
        public void TestNewOdbcSourceTooltip()
        {
            _target.NewOdbcSourceTooltip = Resources.Languages.Tooltips.NoPermissionsToolTip;
            Assert.AreEqual(Resources.Languages.Tooltips.NoPermissionsToolTip, _target.NewOdbcSourceTooltip);
        }

        [TestMethod]
        public void TestNewWebSourceTooltip()
        {
            _target.NewWebSourceTooltip = Resources.Languages.Tooltips.NoPermissionsToolTip;
            Assert.AreEqual(Resources.Languages.Tooltips.NoPermissionsToolTip, _target.NewWebSourceTooltip);
        }

        [TestMethod]
        public void TestNewPluginSourceTooltip()
        {
            _target.NewPluginSourceTooltip = Resources.Languages.Tooltips.NoPermissionsToolTip;
            Assert.AreEqual(Resources.Languages.Tooltips.NoPermissionsToolTip, _target.NewPluginSourceTooltip);
        }

        [TestMethod]
        public void TestNewComPluginSourceTooltip()
        {
            _target.NewComPluginSourceTooltip = Resources.Languages.Tooltips.NoPermissionsToolTip;
            Assert.AreEqual(Resources.Languages.Tooltips.NoPermissionsToolTip, _target.NewComPluginSourceTooltip);
        }

        [TestMethod]
        public void TestNewEmailSourceTooltip()
        {
            _target.NewEmailSourceTooltip = Resources.Languages.Tooltips.NoPermissionsToolTip;
            Assert.AreEqual(Resources.Languages.Tooltips.NoPermissionsToolTip, _target.NewEmailSourceTooltip);
        }

        [TestMethod]
        public void TestNewExchangeSourceTooltip()
        {
            _target.NewExchangeSourceTooltip = Resources.Languages.Tooltips.NoPermissionsToolTip;
            Assert.AreEqual(Resources.Languages.Tooltips.NoPermissionsToolTip, _target.NewExchangeSourceTooltip);
        }

        [TestMethod]
        public void TestNewRabbitMqSourceTooltip()
        {
            _target.NewRabbitMqSourceTooltip = Resources.Languages.Tooltips.NoPermissionsToolTip;
            Assert.AreEqual(Resources.Languages.Tooltips.NoPermissionsToolTip, _target.NewRabbitMqSourceTooltip);
        }

        [TestMethod]
        public void TestNewDropboxSourceTooltip()
        {
            _target.NewDropboxSourceTooltip = Resources.Languages.Tooltips.NoPermissionsToolTip;
            Assert.AreEqual(Resources.Languages.Tooltips.NoPermissionsToolTip, _target.NewDropboxSourceTooltip);
        }

        [TestMethod]
        public void TestNewSharepointSourceTooltip()
        {
            _target.NewSharepointSourceTooltip = Resources.Languages.Tooltips.NoPermissionsToolTip;
            Assert.AreEqual(Resources.Languages.Tooltips.NoPermissionsToolTip, _target.NewSharepointSourceTooltip);
        }

        [TestMethod]
        public void TestNewWcfSourceTooltip()
        {
            _target.NewWcfSourceTooltip = Resources.Languages.Tooltips.NoPermissionsToolTip;
            Assert.AreEqual(Resources.Languages.Tooltips.NoPermissionsToolTip, _target.NewWcfSourceTooltip);
        }

        [TestMethod]
        public void TestNewFolderTooltip()
        {
            _target.NewFolderTooltip = Resources.Languages.Tooltips.NoPermissionsToolTip;
            Assert.AreEqual(Resources.Languages.Tooltips.NoPermissionsToolTip, _target.NewFolderTooltip);
        }

        [TestMethod]
        public void TestViewApisJsonTooltip()
        {
            _target.ViewApisJsonTooltip = Resources.Languages.Tooltips.ViewApisJsonTooltip;
            Assert.AreEqual(Resources.Languages.Tooltips.ViewApisJsonTooltip, _target.ViewApisJsonTooltip);
        }

        [TestMethod]
        public void TestServerVersionTooltip()
        {
            _target.ServerVersionTooltip = Resources.Languages.Tooltips.ServerVersionTooltip;
            Assert.AreEqual(Resources.Languages.Tooltips.ServerVersionTooltip, _target.ServerVersionTooltip);
        }

    }
}
