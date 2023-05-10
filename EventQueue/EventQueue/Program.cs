using PersistentQueue;

namespace EventQueue
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            PersistentQueue<MyOwnQueueItem> stringQueue = new PersistentQueue<MyOwnQueueItem>("mau");
            stringQueue.QueueClearedEvent += StringQueue_QueueClearedEvent;
            stringQueue.ItemEnqueuedEvent += StringQueue_ItemEnqueuedEvent;
            stringQueue.ItemDequeuedEvent += StringQueue_ItemDequeuedEvent;
            stringQueue.Clear();

            stringQueue.Enqueue(new MyOwnQueueItem() { Name = "1" });
            stringQueue.Enqueue(new MyOwnQueueItem() { Name = "2" });
            stringQueue.Enqueue(new MyOwnQueueItem() { Name = "3" });
            stringQueue.Enqueue(new MyOwnQueueItem() { Name = "4" });
            stringQueue.Enqueue(new MyOwnQueueItem() { Name = "5" });

            MyOwnQueueItem str1 = stringQueue.Dequeue();

            PersistentQueue<MyOwnQueueItem2> stringQueue2 = new PersistentQueue<MyOwnQueueItem2>("mau2");
            stringQueue2.Clear();

            MyOwnQueueItem str2 = stringQueue.Dequeue();
            
            stringQueue2.Enqueue(new MyOwnQueueItem2() { Name = "1" });
            stringQueue2.Enqueue(new MyOwnQueueItem2() { Name = "2" });
            stringQueue2.Enqueue(new MyOwnQueueItem2() { Name = "3" });
            stringQueue2.Enqueue(new MyOwnQueueItem2() { Name = "4" });
            stringQueue2.Enqueue(new MyOwnQueueItem2() { Name = "5" });

            MyOwnQueueItem2 str3 = stringQueue2.Dequeue();
            MyOwnQueueItem str4 = stringQueue.Dequeue();

        }

        private static void StringQueue_ItemDequeuedEvent(object sender, QueueEventArgs e)
        {
            
        }

        private static void StringQueue_ItemEnqueuedEvent(object sender, QueueEventArgs e)
        {
            
        }

        private static void StringQueue_QueueClearedEvent(object sender, QueueEventArgs e)
        {
            
        }
    }
}