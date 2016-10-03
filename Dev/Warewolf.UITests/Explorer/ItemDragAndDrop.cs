using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class ItemDragAndDropTest
    {
        const string Dice = "Dice";
        [TestMethod]
        public void ItemDragAndDropUITest()
        {            
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests\Acceptance Testing Resources";
            Assert.IsFalse(Directory.Exists(resourcesFolder));
            Uimap.Move_AcceptanceTestd_To_AcceptanceTestingResopurces();
            Assert.IsTrue(Directory.Exists(resourcesFolder));
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            Uimap.SetPlaybackSettings();
#if !DEBUG
            Uimap.CloseHangingDialogs();
#endif
        }


        [TestCleanup()]
        public void MyTestCleanup()
        {
            var destinationDirectory = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources";
            var sourceDirectory = destinationDirectory + @"\Acceptance Tests\Acceptance Testing Resources";
            try
            {
                Directory.Move(sourceDirectory, destinationDirectory);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
UIMap Uimap
        {
            get
            {
                if (_uiMap == null)
                {
                    _uiMap = new UIMap();
                }

                return _uiMap;
            }
        }

        private UIMap _uiMap;

        #endregion
    }
}
