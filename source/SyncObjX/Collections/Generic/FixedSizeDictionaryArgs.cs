using System;

namespace SyncObjX.Collections.Generic
{
    public class FixedSizeDictionaryArgs<T> : EventArgs
    {
        public readonly T Key;

        public FixedSizeDictionaryArgs(T key)
        {
            Key = key;
        }
    }
}
