/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

namespace Warewolf.Streams
{
    public static class IConnectionExtensionMethods
    {
        public static IPublisher<T> NewPublisher<T>(this IConnection connection, IStreamConfig config, ISerializer serializer)
        {
            var publisher = connection.NewPublisher(config);

            return new SerializingPublisher<T>(publisher, serializer);
        }

        public static void StartConsuming<T>(this IConnection connection, IStreamConfig config, IConsumer<T> consumer, IDeserializer deserializer)
        {
            connection.StartConsuming(config, new DeserializingConsumer<T>(deserializer, consumer));
        }
    }
}
