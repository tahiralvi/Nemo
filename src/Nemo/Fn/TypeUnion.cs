﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Nemo.Collections;

namespace Nemo.Fn
{
    public interface ITypeUnion
    {
        Type UnionType { get; }
        bool Is<T>();
        T As<T>();
        Type[] AllTypes { get; }
        object GetObject();
    }

    [Serializable]
    public class TypeUnion<T1> : ITypeUnion
    {
        private readonly Tuple<T1> _state;

        public TypeUnion(T1 t1)
        {
            _state = Tuple.Create(t1);
            UnionType = typeof(T1);
        }

        public bool Is<T>()
        {
            return typeof(T).IsAssignableFrom(UnionType);
        }

        public T As<T>()
        {
            if (!Is<T>())
            {
                throw new Exception(string.Format("TypeUnion: Cannot cast from {0} to {1}", UnionType.Name, typeof(T).Name));
            }
            return (T)((ITypeUnion)this).GetObject();
        }

        object ITypeUnion.GetObject()
        {
            return _state.Item1;
        }

        public Type UnionType
        {
            get;
            private set;
        }

        public Type[] AllTypes
        {
            get
            {
                return new[] { typeof(T1) };
            }
        }
        
        #region Equals, GetHashCode, ==, !=, ToString

        public override bool Equals(object obj)
        {
            return _state.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _state.GetHashCode();
        }

        public static bool operator ==(TypeUnion<T1> t1, TypeUnion<T1> t2)
        {
            return ReferenceEquals(t1, t2) || (Equals(t1, null) && t1.Equals(t2));
        }

        public static bool operator !=(TypeUnion<T1> t1, TypeUnion<T1> t2)
        {
            return !(t1 == t2);
        }

        public override string ToString()
        {
            return string.Format("{0}->{1}:{2}", GetType().Name, UnionType.Name, ((ITypeUnion)this).GetObject());
        }

        #endregion
    }

    [Serializable]
    public class TypeUnion<T1, T2> : ITypeUnion
    {
        private readonly Tuple<Maybe<T1>, Maybe<T2>> _state;

        public TypeUnion(T1 t1)
        {
            _state = Tuple.Create(new Maybe<T1>(t1), Maybe<T2>.Empty);
            UnionType = typeof(T1);
        }

        public TypeUnion(T2 t2)
        {
            _state = Tuple.Create(Maybe<T1>.Empty, new Maybe<T2>(t2));
            UnionType = typeof(T2);
        }

        public bool Is<T>()
        {
            return typeof(T).IsAssignableFrom(UnionType);
        }

        public T As<T>()
        {
            if (!Is<T>())
            {
                throw new Exception(string.Format("TypeUnion: Cannot cast from {0} to {1}", UnionType.Name, typeof(T).Name));
            }
            return (T)((ITypeUnion)this).GetObject();
        }

        object ITypeUnion.GetObject()
        {
            if (_state.Item1.HasValue)
            {
                return _state.Item1.Value;
            }
            return _state.Item2.Value;
        }

        public Type UnionType
        {
            get;
            private set;
        }

        public Type[] AllTypes
        {
            get
            {
                return new[] { typeof(T1), typeof(T2) };
            }
        }

        #region Equals, GetHashCode, ==, !=, ToString

        public override bool Equals(object obj)
        {
            return _state.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _state.GetHashCode();
        }

        public static bool operator ==(TypeUnion<T1, T2> t1, TypeUnion<T1, T2> t2)
        {
            return ReferenceEquals(t1, t2) || (!Equals(t1, null) && t1.Equals(t2));
        }

        public static bool operator !=(TypeUnion<T1, T2> t1, TypeUnion<T1, T2> t2)
        {
            return !(t1 == t2);
        }

        public override string ToString()
        {
            return string.Format("{0}->{1}:{2}", GetType().Name, UnionType.Name, ((ITypeUnion)this).GetObject());
        }

        #endregion
    }

    [Serializable]
    public class TypeUnion<T1, T2, T3> : ITypeUnion
    {
        private readonly Tuple<Maybe<T1>, Maybe<T2>, Maybe<T3>> _state;

        public TypeUnion(T1 t1)
        {
            _state = Tuple.Create(new Maybe<T1>(t1), Maybe<T2>.Empty, Maybe<T3>.Empty);
            UnionType = typeof(T1);
        }

        public TypeUnion(T2 t2)
        {
            _state = Tuple.Create(Maybe<T1>.Empty, new Maybe<T2>(t2), Maybe<T3>.Empty);
            UnionType = typeof(T2);
        }

        public TypeUnion(T3 t3)
        {
            _state = Tuple.Create(Maybe<T1>.Empty, Maybe<T2>.Empty, new Maybe<T3>(t3));
            UnionType = typeof(T3);
        }

        public bool Is<T>()
        {
            return typeof(T).IsAssignableFrom(UnionType);
        }

        public T As<T>()
        {
            if (!Is<T>())
            {
                throw new Exception(string.Format("TypeUnion: Cannot cast from {0} to {1}", UnionType.Name, typeof(T).Name));
            }
            return (T)((ITypeUnion)this).GetObject();
        }

        object ITypeUnion.GetObject()
        {
            if (_state.Item1.HasValue)
            {
                return _state.Item1.Value;
            }
            if (_state.Item2.HasValue)
            {
                return _state.Item2.Value;
            }
            return _state.Item3.Value;
        }

        public Type UnionType
        {
            get;
            private set;
        }

        public Type[] AllTypes
        {
            get
            {
                return new[] { typeof(T1), typeof(T2), typeof(T3) };
            }
        }
        
        #region Equals, GetHashCode, ==, !=, ToString

        public override bool Equals(object obj)
        {
            return _state.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _state.GetHashCode();
        }

        public static bool operator ==(TypeUnion<T1, T2, T3> t1, TypeUnion<T1, T2, T3> t2)
        {
            return ReferenceEquals(t1, t2) || (Equals(t1, null) && t1.Equals(t2));
        }

        public static bool operator !=(TypeUnion<T1, T2, T3> t1, TypeUnion<T1, T2, T3> t2)
        {
            return !(t1 == t2);
        }

        public override string ToString()
        {
            return string.Format("{0}->{1}:{2}", GetType().Name, UnionType.Name, ((ITypeUnion)this).GetObject());
        }

        #endregion
    }

    [Serializable]
    public class TypeUnion<T1, T2, T3, T4> : ITypeUnion
    {
        private readonly Tuple<Maybe<T1>, Maybe<T2>, Maybe<T3>, Maybe<T4>> _state;

        public TypeUnion(T1 t1)
        {
            _state = Tuple.Create(new Maybe<T1>(t1), Maybe<T2>.Empty, Maybe<T3>.Empty, Maybe<T4>.Empty);
            UnionType = typeof(T1);
        }

        public TypeUnion(T2 t2)
        {
            _state = Tuple.Create(Maybe<T1>.Empty, new Maybe<T2>(t2), Maybe<T3>.Empty, Maybe<T4>.Empty);
            UnionType = typeof(T2);
        }

        public TypeUnion(T3 t3)
        {
            _state = Tuple.Create(Maybe<T1>.Empty, Maybe<T2>.Empty, new Maybe<T3>(t3), Maybe<T4>.Empty);
            UnionType = typeof(T3);
        }

        public TypeUnion(T4 t4)
        {
            _state = Tuple.Create(Maybe<T1>.Empty, Maybe<T2>.Empty, Maybe<T3>.Empty, new Maybe<T4>(t4));
            UnionType = typeof(T4);
        }

        public bool Is<T>()
        {
            return typeof(T).IsAssignableFrom(UnionType);
        }

        public T As<T>()
        {
            if (!Is<T>())
            {
                throw new Exception(string.Format("TypeUnion: Cannot cast from {0} to {1}", UnionType.Name, typeof(T).Name));
            }
            return (T)((ITypeUnion)this).GetObject();
        }

        object ITypeUnion.GetObject()
        {
            if (_state.Item1.HasValue)
            {
                return _state.Item1.Value;
            }
            if (_state.Item2.HasValue)
            {
                return _state.Item2.Value;
            }
            if (_state.Item3.HasValue)
            {
                return _state.Item3.Value;
            }
            return _state.Item4.Value;
        }

        public Type UnionType
        {
            get;
            private set;
        }

        public Type[] AllTypes
        {
            get
            {
                return new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) };
            }
        }

        #region Equals, GetHashCode, ==, !=, ToString

        public override bool Equals(object obj)
        {
            return _state.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _state.GetHashCode();
        }

        public static bool operator ==(TypeUnion<T1, T2, T3, T4> t1, TypeUnion<T1, T2, T3, T4> t2)
        {
            return ReferenceEquals(t1, t2) || (Equals(t1, null) && t1.Equals(t2));
        }
        public static bool operator !=(TypeUnion<T1, T2, T3, T4> t1, TypeUnion<T1, T2, T3, T4> t2)
        {
            return !(t1 == t2);
        }

        public override string ToString()
        {
            return string.Format("{0}->{1}:{2}", GetType().Name, UnionType.Name, ((ITypeUnion)this).GetObject());
        }

        #endregion
    }

    [Serializable]
    public class TypeUnion<T1, T2, T3, T4, T5> : ITypeUnion
    {
        private readonly Tuple<Maybe<T1>, Maybe<T2>, Maybe<T3>, Maybe<T4>, Maybe<T5>> _state;

        public TypeUnion(T1 t1)
        {
            _state = Tuple.Create(new Maybe<T1>(t1), Maybe<T2>.Empty, Maybe<T3>.Empty, Maybe<T4>.Empty, Maybe<T5>.Empty);
            UnionType = typeof(T1);
        }

        public TypeUnion(T2 t2)
        {
            _state = Tuple.Create(Maybe<T1>.Empty, new Maybe<T2>(t2), Maybe<T3>.Empty, Maybe<T4>.Empty, Maybe<T5>.Empty);
            UnionType = typeof(T2);
        }

        public TypeUnion(T3 t3)
        {
            _state = Tuple.Create(Maybe<T1>.Empty, Maybe<T2>.Empty, new Maybe<T3>(t3), Maybe<T4>.Empty, Maybe<T5>.Empty);
            UnionType = typeof(T3);
        }

        public TypeUnion(T4 t4)
        {
            _state = Tuple.Create(Maybe<T1>.Empty, Maybe<T2>.Empty, Maybe<T3>.Empty, new Maybe<T4>(t4), Maybe<T5>.Empty);
            UnionType = typeof(T4);
        }

        public TypeUnion(T5 t5)
        {
            _state = Tuple.Create(Maybe<T1>.Empty, Maybe<T2>.Empty, Maybe<T3>.Empty, Maybe<T4>.Empty, new Maybe<T5>(t5));
            UnionType = typeof(T5);
        }

        public bool Is<T>()
        {
            return typeof(T).IsAssignableFrom(UnionType);
        }

        public T As<T>()
        {
            if (!Is<T>())
            {
                throw new Exception(string.Format("TypeUnion: Cannot cast from {0} to {1}", UnionType.Name, typeof(T).Name));
            }
            return (T)((ITypeUnion)this).GetObject();
        }

        object ITypeUnion.GetObject()
        {
            if (_state.Item1.HasValue)
            {
                return _state.Item1.Value;
            }
            if (_state.Item2.HasValue)
            {
                return _state.Item2.Value;
            }
            if (_state.Item3.HasValue)
            {
                return _state.Item3.Value;
            }
            if (_state.Item4.HasValue)
            {
                return _state.Item4.Value;
            }
            return _state.Item5.Value;
        }

        public Type UnionType
        {
            get;
            private set;
        }

        public Type[] AllTypes
        {
            get
            {
                return new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) };
            }
        }

        #region Equals, GetHashCode, ==, !=, ToString

        public override bool Equals(object obj)
        {
            return _state.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _state.GetHashCode();
        }
       
        public static bool operator ==(TypeUnion<T1, T2, T3, T4, T5> t1, TypeUnion<T1, T2, T3, T4, T5> t2)
        {
            return ReferenceEquals(t1, t2) || (Equals(t1, null) && t1.Equals(t2));
        }
        public static bool operator !=(TypeUnion<T1, T2, T3, T4, T5> t1, TypeUnion<T1, T2, T3, T4, T5> t2)
        {
            return !(t1 == t2);
        }

        public override string ToString()
        {
            return string.Format("{0}->{1}:{2}", GetType().Name, UnionType.Name, ((ITypeUnion)this).GetObject());
        }

        #endregion
    }

    public static class TypeUnion
    {
        private static readonly ConcurrentDictionary<TypeArray, Type> Types = new ConcurrentDictionary<TypeArray, Type>();

        public static ITypeUnion Create(IList<Type> types, object value)
        {
            ITypeUnion union = null;
            if (types != null && value != null)
            {
                Type genericType = null;
                switch (types.Count)
                {
                    case 1:
                        genericType = typeof(TypeUnion<>);
                        break;
                    case 2:
                        genericType = typeof(TypeUnion<,>);
                        break;
                    case 3:
                        genericType = typeof(TypeUnion<,,>);
                        break;
                    case 4:
                        genericType = typeof(TypeUnion<,,,>);
                        break;
                    case 5:
                        genericType = typeof(TypeUnion<,,,,>);
                        break;
                }

                if (genericType != null)
                {
                    var key = new TypeArray(types);
                    var type = Types.GetOrAdd(key, t => genericType.MakeGenericType(t.Types is Type[] ? (Type[])t.Types : t.Types.ToArray()));
                    union = (ITypeUnion)Nemo.Reflection.Activator.New(type, value);
                }
            }
            return union;
        }
    }
}
