using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Darwen.General
{
    public class DoubleDictionary<TYPE1, TYPE2> : IDictionary<TYPE1, TYPE2>
    {
        Dictionary<TYPE1, TYPE2> _mapType1ToType2;
        Dictionary<TYPE2, TYPE1> _mapType2ToType1;

        public DoubleDictionary()
        {
            _mapType1ToType2 = new Dictionary<TYPE1, TYPE2>();
            _mapType2ToType1 = new Dictionary<TYPE2, TYPE1>();
        }

        #region IDictionary<TYPE1,TYPE2> Members

        public void Add(TYPE1 key, TYPE2 value)
        {
            _mapType1ToType2.Add(key, value);
            _mapType2ToType1.Add(value, key);
        }

        public bool ContainsKey(TYPE1 key)
        {
            return _mapType1ToType2.ContainsKey(key);
        }

        public bool ContainsKey(TYPE2 key)
        {
            return _mapType2ToType1.ContainsKey(key);
        }

        public ICollection<TYPE1> Keys
        {
            get
            {
                return _mapType1ToType2.Keys;
            }
        }

        public bool Remove(TYPE1 key)
        {
            if (_mapType1ToType2.ContainsKey(key))
            {
                _mapType2ToType1.Remove(_mapType1ToType2[key]);
                _mapType1ToType2.Remove(key);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Remove(TYPE2 key)
        {
            if (_mapType2ToType1.ContainsKey(key))
            {
                _mapType1ToType2.Remove(_mapType2ToType1[key]);
                _mapType2ToType1.Remove(key);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryGetValue(TYPE1 key, out TYPE2 value)
        {
            if (_mapType1ToType2.ContainsKey(key))
            {
                value = _mapType1ToType2[key];
                return true;
            }
            else
            {
                value = default(TYPE2);
                return false;
            }
        }

        public ICollection<TYPE2> Values
        {
            get
            {
                return _mapType2ToType1.Keys;
            }
        }

        public TYPE2 this[TYPE1 key]
        {
            get
            {
                return _mapType1ToType2[key];
            }
            set
            {
                _mapType1ToType2[key] = value;
            }
        }

        public TYPE1 this[TYPE2 key]
        {
            get
            {
                return _mapType2ToType1[key];
            }
            set
            {
                _mapType2ToType1[key] = value;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<TYPE1,TYPE2>> Members

        public void Add(KeyValuePair<TYPE1, TYPE2> item)
        {
            Add(item.Key, item.Value);            
        }

        public void Clear()
        {
            _mapType1ToType2.Clear();
            _mapType2ToType1.Clear();
        }

        public bool Contains(KeyValuePair<TYPE1, TYPE2> item)
        {
            if (_mapType1ToType2.ContainsKey(item.Key))
            {
                if (_mapType2ToType1.ContainsKey(item.Value))
                {
                    return true;
                }
            }
            
            return false;            
        }

        public void CopyTo(KeyValuePair<TYPE1, TYPE2>[] array, int arrayIndex)
        {
            int index = 0;

            foreach (KeyValuePair<TYPE1, TYPE2> pair in _mapType1ToType2)
            {
                array[index++] = pair;
            }
        }

        public int Count
        {
            get
            {
                return _mapType1ToType2.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool Remove(KeyValuePair<TYPE1, TYPE2> item)
        {
            if (_mapType1ToType2.ContainsKey(item.Key))
            {
                if (_mapType2ToType1.ContainsKey(item.Value))
                {
                    Remove(item.Key);
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region IEnumerable<KeyValuePair<TYPE1,TYPE2>> Members

        public IEnumerator<KeyValuePair<TYPE1, TYPE2>> GetEnumerator()
        {
            return _mapType1ToType2.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            IEnumerable enumerator = _mapType1ToType2 as IEnumerable;
            return enumerator.GetEnumerator();
        }

        #endregion
    }
}
