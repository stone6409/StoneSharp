using System;
using System.Collections;
using System.Collections.Generic;

namespace StoneSharp.Core.Models
{
    public class ChatConversationCollection : IList<ChatConversation>, IList
    {
        private List<ChatConversation> _collection;
        private Dictionary<string, ChatConversation> _dictionary;

        public ChatConversationCollection()
        {
            _collection = new List<ChatConversation>();
            _dictionary = new Dictionary<string, ChatConversation>();
        }

        public ChatConversation this[int index]
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

        public ChatConversation this[string id]
        {
            get
            {
                ChatConversation value;
                _dictionary.TryGetValue(id, out value);

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

        public void Add(ChatConversation value)
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

        public bool Contains(ChatConversation value)
        {
            return _collection.Contains(value);
        }

        public void CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (index < 0 || (index + _collection.Count) > array.Length)
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

        public void CopyTo(ChatConversation[] array, int index)
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

        IEnumerator<ChatConversation> IEnumerable<ChatConversation>.GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        public int IndexOf(object value)
        {
            return _collection.IndexOf(value as ChatConversation);
        }

        public int IndexOf(ChatConversation value)
        {
            return _collection.IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            InsertHelper(index, Cast(value));
        }

        public void Insert(int index, ChatConversation value)
        {
            InsertHelper(index, value);
        }

        public void Remove(object value)
        {
            Remove(value as ChatConversation);
        }

        public bool Remove(ChatConversation value)
        {
            _collection.Remove(value);
            _dictionary.Remove(value.Id);

            return true;
        }

        public void RemoveAt(int index)
        {
            ChatConversation value = _collection[index];
            _collection.RemoveAt(index);
            _dictionary.Remove(value.Id);
        }

        private int AddHelper(ChatConversation value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            if (_dictionary.ContainsKey(value.Id))
            {
                throw new ArgumentException();
            }

            int index = _collection.Count;
            _collection.Add(value);
            _dictionary.Add(value.Id, value);

            return index;
        }

        private void InsertHelper(int index, ChatConversation value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            if (_dictionary.ContainsKey(value.Id))
            {
                throw new ArgumentException();
            }

            _collection.Insert(index, value);
            _dictionary.Add(value.Id, value);
        }

        private ChatConversation Cast(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            if (!(value is ChatConversation))
            {
                throw new ArgumentException();
            }

            return (ChatConversation)value;
        }

        public void RemoveById(string id)
        {
            int index = _collection.FindIndex(f => f.Id == id);
            RemoveAt(index);
        }

        public bool ContainsName(string id)
        {
            return _dictionary.ContainsKey(id);
        }
    }
}
