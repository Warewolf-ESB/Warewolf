using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace IronRuby.Builtins
{
	internal class RubyPipe : Stream
	{
		internal class PipeWriter : RubyPipe
		{
			internal PipeWriter(RubyPipe pipe)
				: base(pipe)
			{
			}

			public override void Close()
			{
				base.Close();
				CloseWriter();
			}
		}

		private const int WriterClosedEventIndex = 1;

		private readonly EventWaitHandle _dataAvailableEvent;

		private readonly EventWaitHandle _writerClosedEvent;

		private readonly WaitHandle[] _eventArray;

		private readonly Queue<byte> _queue;

		public override bool CanRead
		{
			get
			{
				return true;
			}
		}

		public override bool CanSeek
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override bool CanWrite
		{
			get
			{
				return true;
			}
		}

		public override long Length
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override long Position
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		private RubyPipe()
		{
			_dataAvailableEvent = new AutoResetEvent(false);
			_writerClosedEvent = new ManualResetEvent(false);
			_eventArray = new WaitHandle[2];
			_queue = new Queue<byte>();
			_eventArray[0] = _dataAvailableEvent;
			_eventArray[1] = _writerClosedEvent;
		}

		private RubyPipe(RubyPipe pipe)
		{
			_dataAvailableEvent = pipe._dataAvailableEvent;
			_writerClosedEvent = pipe._writerClosedEvent;
			_eventArray = pipe._eventArray;
			_queue = pipe._queue;
		}

		internal void CloseWriter()
		{
			_writerClosedEvent.Set();
		}

		public static void CreatePipe(out Stream reader, out Stream writer)
		{
			writer = new PipeWriter((RubyPipe)(reader = new RubyPipe()));
		}

		public override void Flush()
		{
			throw new NotImplementedException();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			WaitHandle.WaitAny(_eventArray);
			lock (((ICollection)_queue).SyncRoot)
			{
				if (_queue.Count <= count)
				{
					_queue.CopyTo(buffer, 0);
					_queue.Clear();
					return _queue.Count;
				}
				for (int i = 0; i < count; i++)
				{
					buffer[i] = _queue.Dequeue();
				}
				return count;
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			lock (((ICollection)_queue).SyncRoot)
			{
				for (int i = 0; i < count; i++)
				{
					_queue.Enqueue(buffer[offset + i]);
				}
				_dataAvailableEvent.Set();
			}
		}
	}
}
