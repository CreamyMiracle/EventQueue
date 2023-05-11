using PersistentQueue;

namespace EventQueue
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            PersistentQueue<string> stringQueue = new PersistentQueue<string>("mau");
            stringQueue.QueueClearedEvent += StringQueue_QueueClearedEvent;
            stringQueue.ItemEnqueuedEvent += StringQueue_ItemEnqueuedEvent;
            stringQueue.ItemDequeuedEvent += StringQueue_ItemDequeuedEvent;
            stringQueue.Clear();

            stringQueue.Enqueue("1");
            stringQueue.Enqueue("2");
            stringQueue.Enqueue("3");
            stringQueue.Enqueue("4");
            stringQueue.Enqueue("5");

            string str1 = stringQueue.Dequeue();

            PersistentQueue<string> stringQueue2 = new PersistentQueue<string>("mau2");
            stringQueue2.Clear();

            string str2 = stringQueue.Dequeue();
            
            stringQueue2.Enqueue("1");
            stringQueue2.Enqueue("2");
            stringQueue2.Enqueue("3");
            stringQueue2.Enqueue("4");
            stringQueue2.Enqueue("5");

            string str3 = stringQueue2.Dequeue();
            string str4 = stringQueue.Dequeue();

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