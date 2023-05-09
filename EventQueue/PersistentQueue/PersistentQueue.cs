using Akavache;
using System.Reactive.Linq;

namespace PersistentQueue
{
    public class PersistentQueue<T>
    {
        private int _currentItemValue = 0;
        private readonly string _currentItemKey = "currItemKey";


        public PersistentQueue()
        {
            Akavache.Registrations.Start("PersistentQueue");

        }

        public async Task Init()
        {
            _currentItemValue = await BlobCache.UserAccount.GetObject<int>(_currentItemKey);
        }

        public async Task Enqueue<T>(T item)
        {
            await BlobCache.LocalMachine.InvalidateObject<T>(_currentItemKey);
            _currentItemValue++;
            await BlobCache.LocalMachine.InsertObject(_currentItemKey, _currentItemValue);

            await BlobCache.LocalMachine.InsertObject(_currentItemValue.ToString(), item);
        }

        public async Task<T> Dequeue<T>()
        {
            T item = await BlobCache.LocalMachine.GetObject<T>(_currentItemValue.ToString());
            await BlobCache.LocalMachine.InvalidateObject<T>(_currentItemValue.ToString());

            await BlobCache.LocalMachine.InvalidateObject<T>(_currentItemKey);
            _currentItemValue--;
            await BlobCache.LocalMachine.InsertObject(_currentItemKey, _currentItemValue);
            return item;
        }

        public async Task<T> Peek<T>()
        {
            return await BlobCache.LocalMachine.GetObject<T>(_currentItemValue.ToString());
        }

        public async Task Clear()
        {
            await BlobCache.LocalMachine.InvalidateAll();
        }
    }
}