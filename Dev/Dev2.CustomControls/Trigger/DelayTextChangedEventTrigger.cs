using System;
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
        private IDisposable _subscription;

        public long DelayInMilliSeconds
        {
            get { return (long)GetValue(DelayInMilliSecondsProperty); }
            set { SetValue(DelayInMilliSecondsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DelayInMilliSeconds.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DelayInMilliSecondsProperty =
            DependencyProperty.Register("DelayInMilliSeconds", typeof(long), typeof(DelayTextChangedEventTrigger), new PropertyMetadata(1000L));      

        protected override void OnAttached()
        {
            base.OnAttached();

            var observable = Observable.FromEventPattern(AssociatedObject, "TextChanged")
                              .Throttle(TimeSpan.FromMilliseconds(DelayInMilliSeconds),System.Reactive.Concurrency.Scheduler.ThreadPool)
                              .ObserveOn(SynchronizationContext.Current);

            _subscription = observable.Subscribe(ProcessKeyPress);

        }

// ReSharper disable RedundantNameQualifier
        private void ProcessKeyPress(System.Reactive.EventPattern<System.EventArgs> obj)
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
