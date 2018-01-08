using System;

namespace SyncObjX.Collections.Generic
{
    public class FixedSizeQueueArgs<T> : EventArgs
    {
        public readonly T Item;

        public FixedSizeQueueArgs(T item)
        {
            Item = item;
        }
    }
}
