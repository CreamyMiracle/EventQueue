using Akavache;
using System;
using System.Reactive.Linq;

namespace PersistentQueue
{
    public class PersistentQueue<T> : IDisposable where T : QueueItem
    {
        public PersistentQueue(string name = "PersistentQueue")
        {
            Registrations.Start(name);

            _frontKey = name + "_frontPointer";
            _backKey = name + "_backPointer";
        }

        ~PersistentQueue()
        {
            Dispose(false);
        }

        #region Events
        public delegate void QueueEventHandler(object sender, QueueEventArgs e);
        public event QueueEventHandler ItemEnqueuedEvent;
        public event QueueEventHandler ItemDequeuedEvent;
        public event QueueEventHandler QueueClearedEvent;
        protected virtual void RaiseItemEnqueuedEvent()
        {
            ItemEnqueuedEvent?.Invoke(this, new QueueEventArgs());
        }
        protected virtual void RaiseItemDequeuedEvent()
        {
            ItemDequeuedEvent?.Invoke(this, new QueueEventArgs());
        }
        protected virtual void RaiseQueueClearedEvent()
        {
            QueueClearedEvent?.Invoke(this, new QueueEventArgs());
        }
        #endregion

        #region Async Methods
        public async Task EnqueueAsync(T item, DateTimeOffset? expirationTime = null)
        {
            int? Back = await BlobCache.LocalMachine.GetOrCreateObject<int?>(_backKey, () => null);
            if (Back == null)
            {
                Back = 0;
                await SetBack(0);
            }
            else
            {
                await SetBack(++Back);
            }
            await BlobCache.LocalMachine.InsertObject(Back.ToString(), item, expirationTime);
            RaiseItemEnqueuedEvent();
        }

        public async Task<T> DequeueAsync()
        {
            int? Front = await BlobCache.LocalMachine.GetOrCreateObject<int?>(_frontKey, () => null);
            int? Back = await BlobCache.LocalMachine.GetOrCreateObject<int?>(_backKey, () => null);

            if (Front == null)
            {
                Front = 0;
                await SetFront(0);
            }
            else
            {
                await SetFront(++Front);
            }

            try
            {
                T item = await BlobCache.LocalMachine.GetObject<T>(Front.ToString());
                await BlobCache.LocalMachine.InvalidateObject<T>(Front.ToString());

                if (Front.Equals(Back))
                {
                    await ClearAsync();
                }

                RaiseItemDequeuedEvent();
                return item;
            }
            catch (KeyNotFoundException)
            {
                if (Front == 0)
                {
                    await BlobCache.LocalMachine.InvalidateObject<T>(_frontKey);
                }
                return default(T);
            }
        }

        public async Task<T> PeekAsync()
        {
            try
            {
                int? Front = await BlobCache.LocalMachine.GetOrCreateObject<int?>(_frontKey, () => null);
                return await BlobCache.LocalMachine.GetObject<T>(Front.ToString());
            }
            catch (KeyNotFoundException)
            {
                return default(T);
            }
        }

        public async Task ClearAsync()
        {
            await BlobCache.LocalMachine.InvalidateAllObjects<T>();
            await BlobCache.LocalMachine.InvalidateObject<string>(_frontKey);
            await BlobCache.LocalMachine.InvalidateObject<string>(_backKey);
            RaiseQueueClearedEvent();
        }
        #endregion

        #region Sync Methods
        public void Enqueue(T item, DateTimeOffset? expirationTime = null)
        {
            Task task = EnqueueAsync(item, expirationTime);
            task.Wait();
        }

        public T Dequeue()
        {
            Task<T> task = DequeueAsync();
            task.Wait();
            return task.Result;
        }

        public T Peek()
        {
            Task<T> task = PeekAsync();
            task.Wait();
            return task.Result;
        }

        public void Clear()
        {
            Task task = ClearAsync();
            task.Wait();
        }
        #endregion

        #region Disposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Manual release of managed resources.    
                }
                // Release unmanaged resources.
                BlobCache.Shutdown().Wait();
                disposed = true;
            }
        }
        #endregion

        #region Private Methods
        private async Task SetFront(int? value)
        {
            await BlobCache.LocalMachine.InvalidateObject<T>(_frontKey);
            await BlobCache.LocalMachine.InsertObject(_frontKey, value);
        }
        private async Task SetBack(int? value)
        {
            await BlobCache.LocalMachine.InvalidateObject<T>(_backKey);
            await BlobCache.LocalMachine.InsertObject(_backKey, value);
        }
        #endregion

        #region Private Fields
        private bool disposed = false;
        private readonly string _frontKey;
        private readonly string _backKey;
        #endregion
    }
}