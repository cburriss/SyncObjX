using System;
using System.Collections.Generic;

namespace SyncObjX.Collections.Generic
{
    public class FixedSizeQueue<T>
    {
        private object _lock = new Object();

        private readonly Queue<T> queue = new Queue<T>();

        private int _maxNumberOfItems;

        public event EventHandler<FixedSizeQueueArgs<T>> ItemEnqueued;

        public event EventHandler<FixedSizeQueueArgs<T>> ItemDequeued;

        public int Count
        {
            get { return queue.Count; }
        }

        public int MaxNumberOfItems
        {
            get { return _maxNumberOfItems; }

            set
            {
                if (value < 1)
                    throw new Exception("Fized size queue must allow at least one item.");

                _maxNumberOfItems = value;
            }
        }

        public FixedSizeQueue(int maxNumberOfItems)
        {
            MaxNumberOfItems = maxNumberOfItems;
        }

        public virtual void Enqueue(T item)
        {
            queue.Enqueue(item);

            lock (_lock)
            {
                while (Count > MaxNumberOfItems)
                    Dequeue();
            }

            if (ItemEnqueued != null)
                ItemEnqueued(this, new FixedSizeQueueArgs<T>(item));
        }

        public virtual T Dequeue()
        {
            T item = queue.Dequeue();

            if (ItemDequeued != null)
                ItemDequeued(this, new FixedSizeQueueArgs<T>(item));

            return item;
        }
    }
}
