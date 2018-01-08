using System;
using System.Collections.Generic;

namespace SyncObjX.Collections.Generic
{
    [Serializable]
    public class FixedSizeDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        private int _maxNumberOfItems;

        private FixedSizeQueue<TKey> _queue;

        public int MaxNumberOfItems
        {
            get { return _maxNumberOfItems; }

            set
            {
                if (value < 1)
                    throw new Exception("Fized size dictionary must allow at least one item.");

                _maxNumberOfItems = value;
            }
        }

        public FixedSizeDictionary(int maxNumberOfItems)
            : base()
        {
            MaxNumberOfItems = maxNumberOfItems;

            _queue = new FixedSizeQueue<TKey>(maxNumberOfItems);

            _queue.ItemDequeued += new EventHandler<FixedSizeQueueArgs<TKey>>((s, e) =>
            {
                if (this.ContainsKey(e.Item))
                    base.Remove(e.Item);
            });
        }

        public new void Add(TKey key, TValue value)
        {
            base.Add(key, value);

            _queue.Enqueue(key);
        }

        public new void Remove(TKey key)
        {
            throw new Exception("Explicit removal of an item is not allowed.");
        }
    }
}
