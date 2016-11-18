using System;
using System.Collections.Generic;

namespace Prelude
{
    public interface Identity<Key, Value>
    {
        Key ID { get; }
        Value Entity { get; }
    }

    public static class Utils
    {
        public static Dictionary<T, K> Add<T, K>(this Dictionary<T, K> memo, Identity<T, K> id)
        {
            memo.Add(id.ID, id.Entity);
            return memo;
        }

        public static Dictionary<T, K> MaybeAdd<T, K>(this Dictionary<T, K> memo, Identity<T, K> id)
        {
            if (!memo.ContainsKey(id.ID))
                memo.Add(id);

            return memo;
        }
    }
}