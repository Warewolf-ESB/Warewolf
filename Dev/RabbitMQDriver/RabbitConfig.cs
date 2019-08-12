/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;
using RabbitMQ.Client;
using IConnection = RabbitMQ.Client.IConnection;

namespace Warewolf.Driver.RabbitMQ
{
    public class RabbitConfig : IQueueConfig
    {
        public RabbitConfig()
        {
        }

        //options
        internal IModel CreateChannel(IConnection connection)
        {
            var channel = connection.CreateModel();

              //configure channel using options here             
            return channel;
        }
    }
}