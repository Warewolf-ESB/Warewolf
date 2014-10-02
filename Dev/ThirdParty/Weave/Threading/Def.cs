
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace System.Threading
{
    public interface IMessage
    {
        void Execute();
    }      

    public interface IMessageContext
    {
        void Post(IMessage message);
    }

    internal sealed class Lock : IDisposable
    {
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private int _disposed;
      
        public void EnterReadLock()
        {
            _lock.EnterReadLock();
        }

        public void EnterWriteLock()
        {
            _lock.EnterWriteLock();
        }

        public void ExitReadLock()
        {
            _lock.ExitReadLock();
        }

        public void ExitWriteLock()
        {
            _lock.ExitWriteLock();
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
                _lock.Dispose();
        }
    }

    public sealed class TokenGeneratingLock : IDisposable
    {
        private int _disposed;
        private bool _isThreadSafe;
        private readonly Lock _lock;

        public bool IsThreadSafe { get { return _isThreadSafe; } }

        public TokenGeneratingLock(bool isThreadSafe)
        {
            if ((_isThreadSafe = isThreadSafe)) _lock = new Lock();
        }

        public IDisposable LockStateForRead()
        {
            if (_isThreadSafe) return new ReadLock(_lock);
            return WeaveUtility.EmptyReferenceDisposable;
        }

        public IDisposable LockStateForWrite()
        {
            if (_isThreadSafe) return new WriteLock(_lock);
            return WeaveUtility.EmptyReferenceDisposable;
        }

        public void Dispose()
        {
            if (_isThreadSafe && Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
                _lock.Dispose();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ReadLock : IDisposable
    {
        private readonly Lock _lock;
        private int _disposed;

        public ReadLock(Lock @lock)
        {
            _disposed = 0;
            _lock = @lock;
            _lock.EnterReadLock();
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
                _lock.ExitReadLock();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WriteLock : IDisposable
    {
        private readonly Lock _lock;
        private int _disposed;

        public WriteLock(Lock @lock)
        {
            _disposed = 0;
            _lock = @lock;
            _lock.EnterWriteLock();
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
                _lock.ExitWriteLock();
        }
    }

    public class AtomicComposition : IDisposable
    {
        #region Instance Fields
        private readonly AtomicComposition _outerComposition;
        private KeyValuePair<object, object>[] _values;
        private List<Action> _rollbackActions;
        private List<Action> _completeActions;

        private int _totalValues;

        private bool _containsInnerComposition;
        private bool _disposed;
        private bool _completed;
        #endregion

        #region Constructors
        public AtomicComposition()
            : this(null)
        {
        }

        public AtomicComposition(AtomicComposition outerComposition)
        {
            if (outerComposition != null)
            {
                _outerComposition = outerComposition;
                _outerComposition.SetContainsInnerComposition(true);
            }
        }
        #endregion

        #region [Get/Set] Handling
        public bool TryGetValue<T>(object key, out T value)
        {
            if (_disposed) throw new ObjectDisposedException(GetType().ToString());
            if (_completed) throw new InvalidOperationException("The atomicComposition can no longer be changed because the atomicComposition has already been completed.");
            if (key == null) throw new ArgumentNullException("key");
            object found;

            if (!TryGetValueInternal(key, false, out found))
            {
                value = default(T);
                return false;
            }
            else
            {
                value = (T)found;
                return true;
            }
        }

        public bool TryGetValue<T>(object key, bool localCompositionOnly, out T value)
        {
            if (_disposed) throw new ObjectDisposedException(GetType().ToString());
            if (_completed) throw new InvalidOperationException("The atomicComposition can no longer be changed because the atomicComposition has already been completed.");
            if (key == null) throw new ArgumentNullException("key");
            object found;

            if (!TryGetValueInternal(key, localCompositionOnly, out found))
            {
                value = default(T);
                return false;
            }
            else
            {
                value = (T)found;
                return true;
            }
        }

        private bool TryGetValueInternal(object key, bool localCompositionOnly, out object value)
        {
            for (int i = 0; i < _totalValues; i++)
                if (_values[i].Key == key)
                {
                    value = _values[i].Value;
                    return true;
                }

            if (!localCompositionOnly && _outerComposition != null) return _outerComposition.TryGetValueInternal(key, localCompositionOnly, out value);
            value = null;
            return false;
        }

        public void SetValue(object key, object value)
        {
            if (_disposed) throw new ObjectDisposedException(GetType().ToString());
            if (_completed) throw new InvalidOperationException("The atomicComposition can no longer be changed because the atomicComposition has already been completed.");
            if (_containsInnerComposition) throw new InvalidOperationException("The atomicComposition contains another inner atomicComposition and cannot be changed until the that inner atomicComposition has been completed.");
            if (key == null) throw new ArgumentNullException("key");
            SetValueInternal(key, value);
        }

        private void SetValueInternal(object key, object value)
        {
            for (int i = 0; i < _totalValues; i++)
                if (_values[i].Key == key)
                {
                    _values[i] = new KeyValuePair<object, object>(key, value);
                    return;
                }

            if (_values == null || _totalValues == _values.Length)
            {
                KeyValuePair<object, object>[] nValues = new KeyValuePair<object, object>[_totalValues == 0 ? 5 : (_totalValues * 2)];
                if (_values != null) Array.Copy(_values, nValues, _totalValues);
                _values = nValues;
            }

            _values[_totalValues++] = new KeyValuePair<object, object>(key, value);
        }

        private void SetContainsInnerComposition(bool value)
        {
            if (value && _containsInnerComposition) throw new InvalidOperationException("The atomicComposition already contains an inner atomicComposition and cannot contain more than one atomicComposition at a time.");
            _containsInnerComposition = value;
        }
        #endregion

        #region Addition Handling
        public void AddCompleteAction(Action action)
        {
            if (_disposed) throw new ObjectDisposedException(GetType().ToString());
            if (_completed) throw new InvalidOperationException("The atomicComposition can no longer be changed because the atomicComposition has already been completed.");
            if (_containsInnerComposition) throw new InvalidOperationException("The atomicComposition contains another inner atomicComposition and cannot be changed until the that inner atomicComposition has been completed.");
            if (action == null) throw new ArgumentNullException("action");
            if (_completeActions == null) _completeActions = new List<Action>();
            _completeActions.Add(action);
        }

        public void AddRollbackAction(Action action)
        {
            if (_disposed) throw new ObjectDisposedException(GetType().ToString());
            if (_completed) throw new InvalidOperationException("The atomicComposition can no longer be changed because the atomicComposition has already been completed.");
            if (_containsInnerComposition) throw new InvalidOperationException("The atomicComposition contains another inner atomicComposition and cannot be changed until the that inner atomicComposition has been completed.");
            if (action == null) throw new ArgumentNullException("action");
            if (_rollbackActions == null) _rollbackActions = new List<Action>();
            _rollbackActions.Add(action);
        }
        #endregion

        #region Completion Handling
        public void Complete()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().ToString());
            if (_completed) throw new InvalidOperationException("The atomicComposition can no longer be changed because the atomicComposition has already been completed.");

            if (_outerComposition == null)
            {
                if (_completeActions != null)
                {
                    foreach (Action action in _completeActions) action();
                    _completeActions = null;
                }
            }
            else
            {
                _outerComposition.SetContainsInnerComposition(false);

                if (_completeActions != null)
                    foreach (Action action in _completeActions)
                        _outerComposition.AddCompleteAction(action);

                if (_rollbackActions != null)
                    foreach (Action action in _rollbackActions)
                        _outerComposition.AddRollbackAction(action);

                for (int i = 0; i < _totalValues; i++) _outerComposition.SetValueInternal(_values[i].Key, _values[i].Value);
            }

            _completed = true;
        }
        #endregion

        #region Disposal Handling
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) throw new ObjectDisposedException(GetType().ToString());
            _disposed = true;
            if (_outerComposition != null) _outerComposition.SetContainsInnerComposition(false);

            if (!_completed && _rollbackActions != null)
            {
                for (int i = _rollbackActions.Count - 1; i >= 0; --i) _rollbackActions[i]();
                _rollbackActions = null;
            }
        }
        #endregion
    }
}
