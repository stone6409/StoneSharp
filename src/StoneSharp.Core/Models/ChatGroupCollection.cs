using System;
using System.Collections;
using System.Collections.Generic;

namespace StoneSharp.Core.Models
{
    public class ChatGroupCollection : IList<ChatGroup>, IList
    {
        private List<ChatGroup> _collection;
        private Dictionary<string, ChatGroup> _dictionary;

        public ChatGroupCollection()
        {
            _collection = new List<ChatGroup>();
            _dictionary = new Dictionary<string, ChatGroup>();
        }

        public ChatGroup this[int index]
        {
            get
            {
                return _collection[index];
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        object IList.this[int index]
        {
            get
            {
                return _collection[index];
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public ChatGroup this[string name]
        {
            get
            {
                ChatGroup value;
                _dictionary.TryGetValue(name, out value);

                return value;
            }
        }

        public bool IsReadOnly => throw new NotImplementedException();

        public bool IsFixedSize => throw new NotImplementedException();

        public int Count
        {
            get
            {
                return _collection.Count;
            }
        }

        public object SyncRoot => throw new NotImplementedException();

        public bool IsSynchronized => throw new NotImplementedException();

        public void Add(ChatGroup value)
        {
            AddHelper(value);
        }

        public int Add(object value)
        {
            return AddHelper(Cast(value));
        }

        public void Clear()
        {
            _collection.Clear();
            _dictionary.Clear();
        }

        public bool Contains(object value)
        {
            return _collection.Contains(Cast(value));
        }

        public bool Contains(ChatGroup value)
        {
            return _collection.Contains(value);
        }

        public void CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (index < 0 || index + _collection.Count > array.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (array.Rank != 1)
            {
                throw new ArgumentException();
            }

            try
            {
                int count = _collection.Count;
                for (int i = 0; i < count; i++)
                {
                    array.SetValue(_collection[i], index + i);
                }
            }
            catch
            {
                throw new ArgumentException();
            }
        }

        public void CopyTo(ChatGroup[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            _collection.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        IEnumerator<ChatGroup> IEnumerable<ChatGroup>.GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        public int IndexOf(object value)
        {
            return _collection.IndexOf(value as ChatGroup);
        }

        public int IndexOf(ChatGroup value)
        {
            return _collection.IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            InsertHelper(index, Cast(value));
        }

        public void Insert(int index, ChatGroup value)
        {
            InsertHelper(index, value);
        }

        public void Remove(object value)
        {
            Remove(value as ChatGroup);
        }

        public bool Remove(ChatGroup value)
        {
            _collection.Remove(value);
            _dictionary.Remove(value.Name);

            return true;
        }

        public void RemoveAt(int index)
        {
            ChatGroup value = _collection[index];
            _collection.RemoveAt(index);
            _dictionary.Remove(value.Name);
        }

        private int AddHelper(ChatGroup value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            if (string.IsNullOrEmpty(value.Name))
            {
                throw new ArgumentException();
            }

            if (_dictionary.ContainsKey(value.Name))
            {
                throw new ArgumentException();
            }

            int index = _collection.Count;
            _collection.Add(value);
            _dictionary.Add(value.Name, value);

            return index;
        }

        private void InsertHelper(int index, ChatGroup value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            if (string.IsNullOrEmpty(value.Name))
            {
                throw new ArgumentException();
            }

            if (_dictionary.ContainsKey(value.Name))
            {
                throw new ArgumentException();
            }

            _collection.Insert(index, value);
            _dictionary.Add(value.Name, value);
        }

        private ChatGroup Cast(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            if (!(value is ChatGroup))
            {
                throw new ArgumentException();
            }

            return (ChatGroup)value;
        }

        public void RemoveByName(string name)
        {
            int index = _collection.FindIndex(f => f.Name == name);
            RemoveAt(index);
        }

        public bool ContainsName(string name)
        {
            return _dictionary.ContainsKey(name);
        }
    }
}
