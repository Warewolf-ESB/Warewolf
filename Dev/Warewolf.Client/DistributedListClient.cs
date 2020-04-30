using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Dev2.SignalR.Wrappers;
using Newtonsoft.Json.Linq;
using Warewolf.DistributedStore;

namespace Warewolf.Client
{
    public class DistributedListClient<T>
    {
        public event Action<T> OnAdd;
        public event Action<T> OnRemove;
        public event Action OnClear;
        public DistributedListClient(IHubConnectionWrapper conn, string listName)
        {
            Console.WriteLine("setup hub");
            var hub = conn.CreateHubProxy(listName);
            //hub.On("Notify", o => { Console.WriteLine("received notify: " + o); });
            var subscription = hub.Subscribe("Notify");
            subscription.Received += delegate(IList<JToken> list)
            {
                var token = ((JObject) list[0]).ToObject<ListNotification>();
                if (token.Type == typeof(Add<T>))
                {
                    var value = ((JObject) list[0]).ToObject<Add<T>>();
                    OnAdd?.Invoke(value.Value);
                }
                else if (token.Type == typeof(Remove<T>))
                {
                    var value = ((JObject) list[0]).ToObject<Remove<T>>();
                    OnRemove?.Invoke(value.Value);
                }
                else if (token.Type == typeof(Clear))
                {
                    OnClear?.Invoke();
                }
                else
                {
                    Console.WriteLine("woot" + list.ToString());
                }
            };

            async void PerformRegistration()
            {
                var result = await hub.Invoke<bool>("register", listName);
                if (result)
                {
                    Console.WriteLine($"registered to watch events on {listName}");
                }
            }

            conn.StateChanged += (stateChange) =>
            {
                Console.WriteLine("state changed: " + stateChange.NewState);
                if (stateChange.NewState == ConnectionStateWrapped.Connected)
                {
                    try
                    {
                        PerformRegistration();
                    }
                    catch (Exception e)
                    {
                    }
                }
            };
        }
    }

    public class ObservableDistributedListClient<T> : DistributedListClient<T>, IEnumerable
    {
        ObservableCollection<T> _observableCollection = new ObservableCollection<T>();
        public ObservableDistributedListClient(IHubConnectionWrapper conn, string listName)
            :base(conn, listName)
        {
            base.OnAdd += obj =>
            {
                Console.WriteLine("add" + obj);
                _observableCollection.Add(obj);
            };
            base.OnRemove += obj =>
            {
                Console.WriteLine("remove" + obj);
                _observableCollection.Remove(obj);
            };
            base.OnClear += () =>
            {
                Console.WriteLine("clear");
                _observableCollection.Clear();
            };
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add => _observableCollection.CollectionChanged += value;
            remove => _observableCollection.CollectionChanged -= value;
        }

        public IEnumerator GetEnumerator()
        {
            var list = _observableCollection.ToArray();
            return list.GetEnumerator();
        }
    }
}
