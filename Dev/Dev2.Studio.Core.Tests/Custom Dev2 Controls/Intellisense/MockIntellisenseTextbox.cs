using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dev2.UI;

namespace Dev2.Core.Tests.Custom_Dev2_Controls.Intellisense
{
    class MockIntellisenseTextbox : IntellisenseTextBox
    {
        public int TextChangedCounter { get; set; }

        
        public void InitTestClass()
        {
            TextChangedCounter = 0;           
        }

        protected override void TheTextHasChanged()
        {            
            TextChangedCounter++;            
        }
    }
}
