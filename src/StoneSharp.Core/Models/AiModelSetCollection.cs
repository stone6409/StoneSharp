using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneSharp.Core.Models
{
    public class AiModelSetCollection : IList<AiModelSet>, IList
    {
        private List<AiModelSet> _collection;
        private Dictionary<string, AiModelSet> _dictionary;

        public AiModelSetCollection()
        {
            _collection = new List<AiModelSet>();
            _dictionary = new Dictionary<string, AiModelSet>();
        }

        public AiModelSet this[int index]
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

        public AiModelSet this[string name]
        {
            get
            {
                AiModelSet value;
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

        public void Add(AiModelSet value)
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

        public bool Contains(AiModelSet value)
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

        public void CopyTo(AiModelSet[] array, int index)
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

        IEnumerator<AiModelSet> IEnumerable<AiModelSet>.GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        public int IndexOf(object value)
        {
            return _collection.IndexOf(value as AiModelSet);
        }

        public int IndexOf(AiModelSet value)
        {
            return _collection.IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            InsertHelper(index, Cast(value));
        }

        public void Insert(int index, AiModelSet value)
        {
            InsertHelper(index, value);
        }

        public void Remove(object value)
        {
            Remove(value as AiModelSet);
        }

        public bool Remove(AiModelSet value)
        {
            for (int i = 0; i < _collection.Count; i++)
            {
                if (_collection[i] == value)
                {
                    RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public void RemoveAt(int index)
        {
            AiModelSet value = _collection[index];
            _collection.RemoveAt(index);
            _dictionary.Remove(value.Name);
        }

        private int AddHelper(AiModelSet value)
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

        private void InsertHelper(int index, AiModelSet value)
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

        private AiModelSet Cast(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            if (!(value is AiModelSet))
            {
                throw new ArgumentException();
            }

            return (AiModelSet)value;
        }

        public AiModelSet FindByName(string name)
        {
            AiModelSet value;
            _dictionary.TryGetValue(name, out value);

            return value;
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
