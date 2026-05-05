using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneSharp.Core.Models.ContextItems
{
    public class ContextItemUsageCollection : IList<ContextItemUsage>, IList
    {
        private List<ContextItemUsage> _collection;

        public ContextItemUsageCollection()
        {
            _collection = new List<ContextItemUsage>();
        }

        public ContextItemUsage this[int index]
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

        public void Add(ContextItemUsage value)
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
        }

        public bool Contains(object value)
        {
            return _collection.Contains(Cast(value));
        }

        public bool Contains(ContextItemUsage value)
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

        public void CopyTo(ContextItemUsage[] array, int index)
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

        IEnumerator<ContextItemUsage> IEnumerable<ContextItemUsage>.GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        public int IndexOf(object value)
        {
            return _collection.IndexOf(value as ContextItemUsage);
        }

        public int IndexOf(ContextItemUsage value)
        {
            return _collection.IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            InsertHelper(index, Cast(value));
        }

        public void Insert(int index, ContextItemUsage value)
        {
            InsertHelper(index, value);
        }

        public void Remove(object value)
        {
            Remove(value as ContextItemUsage);
        }

        public bool Remove(ContextItemUsage value)
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
            ContextItemUsage value = _collection[index];
            _collection.RemoveAt(index);
        }

        private int AddHelper(ContextItemUsage value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            int index = _collection.Count;
            _collection.Add(value);

            return index;
        }

        private void InsertHelper(int index, ContextItemUsage value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            _collection.Insert(index, value);
        }

        private ContextItemUsage Cast(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            if (!(value is ContextItemUsage))
            {
                throw new ArgumentException();
            }

            return (ContextItemUsage)value;
        }
    }
}
