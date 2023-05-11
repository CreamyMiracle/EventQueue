using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersistentQueue
{
    [Table("QueueItem")]
    public class QueueItem<T>
    {
        public QueueItem()
        {

        }

        public QueueItem(T? value)
        {
            Value = value;
        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public T? Value { get; set; }
    }
}
