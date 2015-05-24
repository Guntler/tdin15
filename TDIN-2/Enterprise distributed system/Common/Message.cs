using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Common
{
    [Serializable]
    public class AjaxDictionary<TKey, TValue> : ISerializable
    {
        private Dictionary<TKey, TValue> _dictionary;
        public AjaxDictionary()
        {
            _dictionary = new Dictionary<TKey, TValue>();
        }
        public AjaxDictionary(SerializationInfo info, StreamingContext context)
        {
            _dictionary = new Dictionary<TKey, TValue>();
        }
        public TValue this[TKey key]
        {
            get
            {
                if (key == null) throw new ArgumentNullException("key");
                return _dictionary[key];
            }
            set
            {
                if (key == null) throw new ArgumentNullException("key");
                _dictionary[key] = value;
            }
        }
        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            foreach (TKey key in _dictionary.Keys)
                info.AddValue(key.ToString(), _dictionary[key]);
        }
    }
}
