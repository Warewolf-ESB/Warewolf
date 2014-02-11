using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using Dev2.Runtime.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.Services
{
    /// <summary>
    /// Summary description for HostSecureConfigTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class HostSecureConfigTests
    {
        public static Guid DefaultServerID = Guid.Parse("{D53BBCC5-4794-4DFA-B096-3AA815692E66}");
        public const string DefaultServerKey = "BwIAAACkAABSU0EyAAQAAAEAAQBBgKRIdzPGpaPt3hJ7Kxm8iVrKpfu4wfsJJf/3gBG5qhiS0rs5j5HqkLazdO5az9oPWnSTmNnww03WvCJhz8nhaJjXHoEK6xtcWL++IY+R3E27xaHaPQJSDvGg3j1Jvm0QKUGmzZX75tGDC4s17kQSCpsVW3vEuZ5gBdMLMi3UqaVW9EO7qOcEvVO9Cym7lxViqUhvq6c0nLzp6C6zrtZGjLtFqo9KDj7PMkq10Xc0JkzE1ptRz/YytMRacIDn8tptbHbxM8AtObeeiZ7V6Tznmi82jcAm2Jugr0D97Da2MXZuqEKLL5uPagL4RUHite3kT/puSNbTtqZLdqMtV5HGqVmn2a64JU3b8TIW8rKd5rKucG4KwoXRNQihJzX1it8vcqt6tjDnJZdJkuyDjdd6BKCYHWeX9mqDwKJ3EY+TRZmsl9RILyV/XviyrpTYBdDDmdQ9YLSLt0LtdpPpcRzciwMsBEfMH5NPFOtqSF/151Sl/DBdEJxOINXDl1qdO5MtgL7rXkfiGwu66n4hokRdVlj6TTcXTCn6YrUbzOts6IZJnQ9cwK693u9yMJ3Di0hp49L6LWnoWmW334ys+iOfob0i4eM+M3XNw7wGN/jd6t2KYemVZEnTcl5Lon5BpdoFlxa7Y1n+kXbaeSAwTJIe9HM6uoXIH61VCIk0ac69oZcG2/FhSeBO/DcGIQQqdFvuFqJY0g2qbt7+hmEZDZBehr3KpoRTgB5xPW/ThVhuaoZxlpEb4hFmKoj900knnQk=";
        public const string DefaultSystemKeyPublic = "BgIAAACkAABSU0ExAAQAAAEAAQBzb9y6JXoJj70+TVeUgRc7hPjb6tTJR7B/ZHZKFQsTLkhQLHo+93x/f30Lj/FToE2xXqnuZPk9IV94L4ekt+5jgEFcf1ReuJT/G1dVb1POiEC0upGdagwW10T3PcBK+UzfSXz5kD0SiGhXamPnT/zuHiTtVjv87W+5WuvU1vsrsQ==";
        public const string DefaultSystemKeyPrivate = "BwIAAACkAABSU0EyAAQAAAEAAQBzb9y6JXoJj70+TVeUgRc7hPjb6tTJR7B/ZHZKFQsTLkhQLHo+93x/f30Lj/FToE2xXqnuZPk9IV94L4ekt+5jgEFcf1ReuJT/G1dVb1POiEC0upGdagwW10T3PcBK+UzfSXz5kD0SiGhXamPnT/zuHiTtVjv87W+5WuvU1vsrsRV3gXwwGB0okX1ny1NBZLrWaMC/4AahE38jyNh2GVB7WRdqvhKbwUPb4O0KaOZkxxsQJadNsNc/xj5cQYbzkedn7tCxKTzYcz3G3eatwl6ZMuUZ6EdlVS1l2u3Bovyy/uKDTIaDEics7acXINtK1TQ/aYAUpCulQ4mfYHij49zD8Q/5GhYikM98C7v6z+88iGRGSef77nRm3RmTaAePqGyzywuupq17DyfJy1R8YQWmpcLb3pmVrtn/WeEyRkouSLMP32ck82NcWoi++udSfvkOg3i6gvdoSDPc1dS8Y9DXA5l8EOr0LQgLSgq/crwCJONeFZbgiBPGf2s3Sv16x6KCqySedTZewkXRFbb5tIp4oJJ0/kdVK7L9mwcEXujyfXn4FBbQLhctBIOSQ4U58v2YqIJuTD+GBdOVJuqn1eNokfwZi1iGnD6f6xXHkHEv1t9t6MEZhKyEZw5hTaolRsv3yFnoo5ajQOM4nPQLnXe4V68C1GMz6M4Ix/SNKNzbGbCaISyVvk0v5Z4SVhrQXcrXUjITSF5RxnFUR5ubxCg0ovlye9hWD+3wRrMOnw62JvH72rzUg2fn5K12ODXhdF8=";

        static NameValueCollection NewSettings;
        static NameValueCollection DefaultSettings;

        public static NameValueCollection CreateDefaultConfig()
        {
            return HostSecureConfig.CreateSettings(DefaultServerID.ToString(), DefaultServerKey, DefaultSystemKeyPublic);
        }

        #region Class Initialize/Cleanup

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            DefaultSettings = CreateDefaultConfig();
            NewSettings = HostSecureConfig.CreateSettings(string.Empty, string.Empty, DefaultSystemKeyPublic);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
        }

        #endregion

        #region Ctor

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void HostSecureConfig_WithoutConfig_Expected_ThrowsArgumentNullException()
        {
            var config = new HostSecureConfig(null);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // The tests run in parallel??? so there is no guarantee that the config saved is the one actually being used!!! 
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //[TestMethod]
        //public void HostSecureConfig_WithConfig_Expected_LoadsDefaultValues()
        //{
        //    HostSecureConfig.SaveConfig(_defaultSettings);
        //    TestConfig(DefaultServerID, DefaultServerKey, DefaultSystemKeyPublic, false, new HostSecureConfig());
        //}

        [TestMethod]
        public void HostSecureConfig_WithDefaultSettings_Expected_LoadsDefaultValues()
        {
            TestConfig(DefaultServerID, DefaultServerKey, DefaultSystemKeyPublic, false);
        }

        [TestMethod]
        public void HostSecureConfig_WithNewSettings_Expected_LoadsNewValues()
        {
            TestConfig(DefaultServerID, DefaultServerKey, DefaultSystemKeyPublic, true);
        }

        #endregion

        #region TestConfig

        static void TestConfig(Guid expectedServerID, string expectedServerKey, string expectedSystemKey, bool isNewConfig)
        {
            var config = new HostSecureConfigMock(isNewConfig ? NewSettings : DefaultSettings);
            TestConfig(expectedServerID, expectedServerKey, expectedSystemKey, isNewConfig, config);
        }

        static void TestConfig(Guid expectedServerID, string expectedServerKey, string expectedSystemKey, bool isNewConfig, HostSecureConfigMock config)
        {
            var actualServerID = config.ServerID;

            var actualServerKey = config.ServerKey;
            var actualServerKey64 = Convert.ToBase64String(actualServerKey.ExportCspBlob(true));

            var actualSystemKey = config.SystemKey;
            var actualSystemKey64 = Convert.ToBase64String(actualSystemKey.ExportCspBlob(false));

            Assert.IsFalse(actualServerKey.PublicOnly);
            Assert.IsTrue(actualSystemKey.PublicOnly);
            Assert.AreEqual(expectedSystemKey, actualSystemKey64);

            if(isNewConfig)
            {
                Assert.AreNotEqual(expectedServerID, actualServerID);
                Assert.AreNotEqual(expectedServerKey, actualServerKey64);

                Assert.IsNotNull(config.SaveConfigSettings);
                Assert.AreNotEqual(NewSettings, config.SaveConfigSettings);

                Assert.AreEqual(1, config.SaveConfigHitCount);
                Assert.AreEqual(1, config.ProtectConfigHitCount);
            }
            else
            {
                Assert.AreEqual(expectedServerID, actualServerID);
                Assert.AreEqual(expectedServerKey, actualServerKey64);

                Assert.IsNull(config.SaveConfigSettings);

                Assert.AreEqual(0, config.SaveConfigHitCount);
                Assert.AreEqual(0, config.ProtectConfigHitCount);
            }
        }

        #endregion

    }
}
