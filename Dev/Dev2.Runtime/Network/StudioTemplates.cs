
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Network;

// ReSharper disable CheckNamespace
namespace Dev2.DynamicServices
{
    // ReSharper disable InconsistentNaming
    public static class StudioTemplates
    {
        public static readonly PacketTemplate Server_OnAuxiliaryConnectionRequested = new PacketTemplate(0, 0, true);

        public static readonly PacketTemplate Client_OnAuxiliaryConnectionReply = new PacketTemplate(0, 0, true);

        public static readonly PacketTemplate Server_OnDebugWriterAddition = new PacketTemplate(0, 1, true);
        public static readonly PacketTemplate Server_OnDebugWriterSubtraction = new PacketTemplate(0, 2, true);

        public static readonly PacketTemplate Client_OnDebugWriterWrite = new PacketTemplate(0, 1, true); 

    }
}
