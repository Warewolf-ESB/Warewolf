/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Threading.Tasks;
using Warewolf.Data;

namespace Warewolf.Streams
{
    public interface IConsumer
    {
        Task<ConsumerResult> Consume(byte[] body,string customTransactionID);
    }
    public interface IConsumer<in T>
    {
        Task<ConsumerResult> Consume(T item);
    }
}
