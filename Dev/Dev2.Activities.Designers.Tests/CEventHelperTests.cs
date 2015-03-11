
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.Designers2.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Activities.Designers.Tests
{
    [TestClass]
    public class CEventHelperTests
    {
        int _i;



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CEventHelper_RemoveAll")]
// ReSharper disable InconsistentNaming
        public void CEventHelper_RemoveAll_RemoveAll_ExpectRemoved()
// ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var bob = new BobEvent();
            _i = 0;
            bob.Dobuilding += BobDobuilding;
            bob.Dobuilding += BobDomorebuilding;
            //------------Execute Test---------------------------
            bob.DoSomething();
            Assert.AreEqual(_i, 2);
            CEventHelper.RemoveAllEventHandlers(bob);
            bob.DoSomething();
            //------------Assert Results-------------------------
            Assert.AreEqual(_i,2);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CEventHelper_RemoveAll")]
        // ReSharper disable InconsistentNaming
        public void CEventHelper_RemoveAll_Null()
        // ReSharper restore InconsistentNaming
        {

            CEventHelper.RemoveAllEventHandlers(null);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CEventHelper_RemoveAll")]
// ReSharper disable InconsistentNaming
        public void CEventHelper_RemoveAll_Static_ExpectNothingRemoved()
// ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            BobEvent.GloabalBuild += BobDomorebuilding;
            BobEvent.GloabalBuild += BobDobuilding;
            _i = 0;
            //------------Execute Test---------------------------
            BobEvent.DoSomeThingElse();
            Assert.AreEqual(_i, 2);
            var bob = new BobEvent();
            CEventHelper.RemoveAllEventHandlers(bob);
            BobEvent.DoSomeThingElse();
            //------------Assert Results-------------------------
            Assert.AreEqual(_i, 4);
        }

        void BobDomorebuilding(object sender, BuildArgs args)
        {
            _i++;
        }

        void BobDobuilding(object sender, BuildArgs args)
        {
            _i++;
        }
        
    }

    public class BobEvent
    {
        public event Build Dobuilding;
        public static event GlobalBuild GloabalBuild;

        static void OnGloabalBuild(BuildArgs args)
        {
            var handler = GloabalBuild;
            if(handler != null)
            {
                handler(null, args);
            }
        }

        public void DoSomething()
        {
            OnDobuilding(new BuildArgs());
        }
        public static void DoSomeThingElse()
        {
            OnGloabalBuild(new BuildArgs());
        }
        
        protected virtual void OnDobuilding(BuildArgs args)
        {
            var handler = Dobuilding;
            if(handler != null)
            {
                handler(this, args);
            }
        }
    }

    public delegate void GlobalBuild(object sender, BuildArgs args);

    public delegate void Build(object sender, BuildArgs args);

    public class BuildArgs  
    {
    }
}
