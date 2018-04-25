/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Dev2.Activities;
using Dev2.Activities.Designers2.Decision;
using Dev2.Activities.Designers2.Switch;
using Dev2.Studio.ActivityDesigners;

namespace Dev2
{
    public static class DesignerAttributeMap
    {
        public static Dictionary<Type, Type> DesignerAttributes
        {
            get; private set;
        }

        static DesignerAttributeMap()
        {
            DesignerAttributes = ActivityDesignerHelper.DesignerAttributes;
            DesignerAttributes.Add(typeof(DsfDecision), typeof(DecisionDesignerViewModel));
            DesignerAttributes.Add(typeof(DsfSwitch), typeof(SwitchDesignerViewModel));
        }        
    }
}
