using SQLite;
using System;

namespace PersistentQueue
{
    public class PersistentQueue<T> : IDisposable
    {
        public PersistentQueue(string name = "PersistentQueue")
        {
            File = new FileInfo(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PersistentQueues", name + ".db"));
            DirectoryInfo di = Directory.CreateDirectory(File.Directory.FullName);

            _dbCon = new SQLiteConnection(File.FullName, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);
            _dbCon.CreateTable<QueueItem<T>>();
        }

        ~PersistentQueue()
        {
            Dispose(false);
        }

        #region Events
        public delegate void QueueEventHandler(object sender, QueueEventArgs e);
        public event QueueEventHandler? ItemEnqueuedEvent;
        public event QueueEventHandler? ItemDequeuedEvent;
        public event QueueEventHandler? QueueClearedEvent;
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

        #region Public Methods
        public void Enqueue(T item, DateTimeOffset? expirationTime = null)
        {
            QueueItem<T> queueItem = new QueueItem<T>(item);
            _dbCon.Insert(queueItem);
            RaiseItemEnqueuedEvent();
        }

        public T? Dequeue()
        {
            T? item = default;
            _dbCon.RunInTransaction(() =>
            {
                int front = _dbCon.ExecuteScalar<int>("SELECT Min(Id) FROM QueueItem");
                item = _dbCon.Get<QueueItem<T>>(front).Value;
                _dbCon.Delete<QueueItem<T>>(front);
            });
            return item;
        }

        public T? Peek()
        {
            T? item = default;
            _dbCon.RunInTransaction(() =>
            {
                int front = _dbCon.ExecuteScalar<int>("SELECT Min(Id) FROM QueueItem", null);
                item = _dbCon.Get<QueueItem<T>>(front).Value;
            });
            return item;
        }

        public void Clear()
        {
            _dbCon.DeleteAll<QueueItem<T>>();
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
                disposed = true;
            }
        }
        #endregion

        #region Properties
        public FileInfo File { get; private set; }
        #endregion

        #region Private Fields
        private bool disposed = false;
        private readonly SQLiteConnection _dbCon;
        #endregion
    }
}