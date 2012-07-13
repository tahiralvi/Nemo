﻿using System;
using System.Collections.Generic;
using System.Linq;
using Nemo.Extensions;
using Nemo.Validation;
using System.Collections;

namespace Nemo.Collections
{
    public class MapList<TSource, TResult> : IList<TResult>, IList
        where TSource : class
        where TResult : class
    {
        private IList<TSource> _items;
        private Func<TSource, TResult> _mapper;
        private List<Tuple<bool, TResult>> _map;

        public MapList(IEnumerable<TSource> items, Func<TSource, TResult> mapper)
        {
            _mapper = mapper;
            InitializeItems(items.ToList());
        }

        private Tuple<bool, TResult> NewMapEntry(TResult item)
        {
            return Tuple.Create(item != null, item);
        }

        private void InitializeMap()
        {
            _map = new List<Tuple<bool, TResult>>(_items.Count);
            _map.AddRange(LinqExtensions.Repeat(NewMapEntry(null), _items.Count));
        }

        protected void InitializeItems(IList<TSource> items)
        {
            _items = items;
            InitializeMap();
        }

        private class MapListEnumerator : IEnumerator<TResult>
        {
            private MapList<TSource, TResult> _items;
            private int _index;
            private TResult _item;

            public MapListEnumerator(MapList<TSource, TResult> items)
            {
                _items = items;
                _index = -1;
                _item = default(TResult);
            }

            #region IEnumerator<TResult> Members

            public TResult Current
            {
                get { return _item; }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            { }

            #endregion

            #region IEnumerator Members

            object System.Collections.IEnumerator.Current
            {
                get { return this.Current; }
            }

            public bool MoveNext()
            {
                //Avoids going beyond the end of the collection.
                if (++_index >= _items.Count)
                {
                    return false;
                }
                else
                {
                    // Set current box to next item in collection.
                    _item = _items[_index];
                }
                return true;

            }

            public void Reset()
            {
                _index = -1;
            }

            #endregion
        }

        #region IList<TResult> Members

        public void Insert(int index, TResult item)
        {
            _items.Insert(index, default(TSource));
            _map.Insert(index, NewMapEntry(item));
        }

        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
            _map.RemoveAt(index);
        }

        public TResult this[int index]
        {
            get
            {
                if (!_map[index].Item1)
                {
                    _map[index] = Tuple.Create(true, _mapper(_items[index]));
                }
                return _map[index].Item2;
            }
            set
            {
                _map[index] = NewMapEntry(value);
            }
        }

        public int IndexOf(TResult item)
        {
            var index = 0;
            var iter = this.GetEnumerator();
            while (iter.MoveNext())
            {
                if (iter.Current == item)
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        #endregion

        #region ICollection<TResult> Members

        public void Add(TResult item)
        {
            this.Insert(_items.Count, item);
        }

        public void Clear()
        {
            _items.Clear();
            _map.Clear();
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(TResult item)
        {
            var index = this.IndexOf(item);
            if (index > -1)
            {
                this.RemoveAt(index);
                return true;
            }
            return false;
        }

        public int Count
        {
            get
            {
                return _items.Count;
            }
        }

        public bool Contains(TResult item)
        {
            return this.IndexOf(item) > -1;
        }

        public void CopyTo(TResult[] array, int index)
        {
            this.GetEnumerator().AsEnumerable().CopyTo(array, index);
        }

        #endregion

        #region IEnumerable<TResult> Members

        public IEnumerator<TResult> GetEnumerator()
        {
            return new MapListEnumerator(this);
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region IList Members

        int IList.Add(object value)
        {
            this.Add((TResult)value);
            return this.Count - 1;
        }

        bool IList.Contains(object value)
        {
            return this.Contains((TResult)value);
        }

        int IList.IndexOf(object value)
        {
            return this.IndexOf((TResult)value);
        }

        void IList.Insert(int index, object value)
        {
            this.Insert(index, (TResult)value);
        }

        bool IList.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        void IList.Remove(object value)
        {
            this.Remove((TResult)value);
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this[index] = (TResult)value;
            }
        }

        #endregion

        #region ICollection Members

        void ICollection.CopyTo(Array array, int index)
        {
            this.CopyTo((TResult[])array, index);
        }

        bool ICollection.IsSynchronized
        {
            get { throw new NotImplementedException(); }
        }

        object ICollection.SyncRoot
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
