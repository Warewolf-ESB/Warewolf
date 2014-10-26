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
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Threading;

namespace Dev2.CustomControls.Trigger
{
    public class DelayTextChangedEventTrigger : TriggerBase<TextBox>
    {
        public static readonly DependencyProperty DelayInMilliSecondsProperty =
            DependencyProperty.Register("DelayInMilliSeconds", typeof (long), typeof (DelayTextChangedEventTrigger),
                new PropertyMetadata(1000L));

        private IDisposable _subscription;

        public long DelayInMilliSeconds
        {
            get { return (long) GetValue(DelayInMilliSecondsProperty); }
            set { SetValue(DelayInMilliSecondsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DelayInMilliSeconds.  This enables animation, styling, binding, etc...

        protected override void OnAttached()
        {
            base.OnAttached();

            IObservable<EventPattern<EventArgs>> observable = Observable.FromEventPattern(AssociatedObject,
                "TextChanged")
                .Throttle(TimeSpan.FromMilliseconds(DelayInMilliSeconds), Scheduler.ThreadPool)
                .ObserveOn(SynchronizationContext.Current);

            _subscription = observable.Subscribe(ProcessKeyPress);
        }

// ReSharper disable RedundantNameQualifier
        private void ProcessKeyPress(EventPattern<EventArgs> obj)
// ReSharper restore RedundantNameQualifier
        {
            Dispatcher.BeginInvoke(() => InvokeActions(null));
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            _subscription.Dispose();
        }
    }
}