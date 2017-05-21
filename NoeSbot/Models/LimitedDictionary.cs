using System;
using System.Collections.Generic;
using System.Text;

namespace NoeSbot.Models
{
    public class LimitedDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TValue> _dictionary;
        private Queue<TKey> _keys;
        private int _capacity;

        public LimitedDictionary(int capacity = 1000)
        {
            _keys = new Queue<TKey>(capacity);
            _capacity = capacity;
            _dictionary = new Dictionary<TKey, TValue>(capacity);
        }

        public void Add(TKey key, TValue value)
        {
            if (_dictionary.Count == _capacity)
            {
                var oldestKey = _keys.Dequeue();
                _dictionary.Remove(oldestKey);
            }

            _dictionary.Add(key, value);
            _keys.Enqueue(key);
        }

        public TValue this[TKey key]
        {
            get { return _dictionary[key]; }
        }

        public bool TryGetValue(TKey id, out TValue urbanMain)
        {
            return _dictionary.TryGetValue(id, out urbanMain);
        }
    }
}
