/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;

namespace Dev2.Common.Interfaces.Infrastructure.Events
{
    public interface IEventPublisher
    {
        int Count { get; }

        bool RemoveEvent<TEvent>() where TEvent : class, new();

        IObservable<TEvent> GetEvent<TEvent>() where TEvent : class, new();

        void Publish<TEvent>(TEvent sampleEvent) where TEvent : class, new();

        void PublishObject(object sampleEvent);
    }
}