/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Threading;

namespace HangfireServer
{
    public interface IEventWaitHandle
    {
        EventWaitHandle New();
        void WaitOne();
    }
    public class EventWaitHandleFactory : IEventWaitHandle
    {
        public static EventWaitHandleFactory CreateInstance()
        {
            return new EventWaitHandleFactory();
        }

        private EventWaitHandle _eventWaitHandle;
        public EventWaitHandle New()
        {
            _eventWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            return _eventWaitHandle;
        }

        public void WaitOne()
        {
            _eventWaitHandle.WaitOne();
        }
    }
}