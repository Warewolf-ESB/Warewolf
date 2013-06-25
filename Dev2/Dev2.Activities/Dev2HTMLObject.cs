using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unlimited.Applications.BusinessDesignStudio.Activities {
    public class Dev2HTMLObject {

        public string Name { get; set; }
        public string Value { get; set; }
        public enDev2HTMLType Type {get; set;}
        public string Alt { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Dev2HTMLObject() { }

        public override string ToString() {
            string returnString = base.ToString();

            switch (Type) {
                case enDev2HTMLType.IMAGE:
                    returnString = "<img src=\"{0}\" alt=\"{1}\" width=\"{2}\" src=\"{3}\" /> ";
                    break;

                default:
                    returnString = Value.ToString();
                    break;
            }


            return returnString;
        }


    }
}
