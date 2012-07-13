﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Nemo.Attributes;
using Nemo.Fn;

namespace Nemo.Reflection
{
    internal class ReflectedProperty
    {
        internal ReflectedProperty(PropertyInfo property, IEnumerable<PropertyAttribute> items)
        {
            PropertyName = property.Name;
            PropertyType = property.PropertyType;
            IsPersistent = Maybe<bool>.Empty;
            IsSelectable = Maybe<bool>.Empty;
            IsSimpleList = Reflector.IsSimpleList(property.PropertyType);
            IsBusinessObject = Reflector.IsBusinessObject(property.PropertyType);
            Type elementType;
            IsBusinessObjectList = Reflector.IsBusinessObjectList(property.PropertyType, out elementType);
            ElementType = elementType;
            if (IsBusinessObjectList)
            {
                IsList = true;
                IsListInterface = property.PropertyType.GetGenericTypeDefinition() == typeof(IList<>);
            }
            else
            {
                IsList = Reflector.IsList(property.PropertyType);
                if (IsList)
                {
                    ElementType = Reflector.ExtractCollectionElementType(property.PropertyType);
                    IsListInterface = property.PropertyType.GetGenericTypeDefinition() == typeof(IList<>);
                }
            }
            IsSimpleType = Reflector.IsSimpleType(property.PropertyType);
            IsTypeUnion = Reflector.IsTypeUnion(property.PropertyType);
            IsTuple = Reflector.IsTuple(property.PropertyType);
            IsNullableType = Reflector.IsNullableType(property.PropertyType);
            MappedColumnName = MapColumnAttribute.GetMappedColumnName(property);
            MappedPropertyName = MapPropertyAttribute.GetMappedPropertyName(property);
            CanWrite = property.CanWrite;
            CanRead = property.CanRead;

            foreach(var item in items)
            {
                if (!IsPrimaryKey && item is PrimaryKeyAttribute)
                {
                    IsPrimaryKey = true;
                    continue;
                }

                if (Generator == null && item is Generate.UsingAttribute)
                {
                    Generator = ((Generate.UsingAttribute)item).Type;
                    continue;
                }

                if (!IsAutoGenerated && item is Generate.NativeAttribute)
                {
                    IsAutoGenerated = true;
                    continue;
                }

                if (Parent == null && item is ReferencesAttribute)
                {
                    Parent = ((ReferencesAttribute)item).Parent;
                    continue;
                }

                if (!IsComparable && item is ComparableAttribute)
                {
                    IsComparable = true;
                    CustomComparer = ((ComparableAttribute)item).CustomComparer;
                    continue;
                }

                if (!IsCacheKey && item is CacheKeyAttribute)
                {
                    IsCacheKey = true;
                    continue;
                }

                if (ParameterName == null && item is ParameterAttribute)
                {
                    ParameterName = ((ParameterAttribute)item).Name;
                    Direction = ((ParameterAttribute)item).Direction;
                    continue;
                }

                if (!IsPersistent.HasValue && item is PersistentAttribute)
                {
                    IsPersistent = ((PersistentAttribute)item).Value;
                    continue;
                }

                if (!IsSelectable.HasValue && item is SelectableAttribute)
                {
                    IsSelectable = ((SelectableAttribute)item).Value;
                    continue;
                }
            }

            if (!IsPersistent.HasValue)
            {
                IsPersistent = true;
            }

            if (!IsSelectable.HasValue)
            {
                IsSelectable = true;
            }
        }

        public bool IsSimpleList
        {
            get;
            private set;
        }

        public bool IsBusinessObjectList
        {
            get;
            private set;
        }

        public bool IsListInterface
        {
            get;
            private set;
        }

        public bool IsBusinessObject
        {
            get;
            private set;
        }

        public bool IsSimpleType
        {
            get;
            private set;
        }

        public bool IsTypeUnion
        {
            get;
            private set;
        }

        public bool IsTuple
        {
            get;
            private set;
        }

        public Maybe<bool> IsPersistent
        {
            get;
            private set;
        }

        public bool IsPrimaryKey
        {
            get;
            private set;
        }

        public bool IsAutoGenerated
        {
            get;
            private set;
        }

        public Type Generator
        {
            get;
            private set;
        }

        public Type Parent
        {
            get;
            private set;
        }

        public Maybe<bool> IsSelectable
        {
            get;
            private set;
        }

        public bool IsComparable
        {
            get;
            private set;
        }

        public Type CustomComparer
        {
            get;
            private set;
        }

        public bool IsCacheKey
        {
            get;
            private set;
        }

        public string ParameterName
        {
            get;
            private set;
        }

        public ParameterDirection Direction
        {
            get;
            private set;
        }

        public string PropertyName
        {
            get;
            private set;
        }

        public Type PropertyType
        {
            get;
            private set;
        }

        public string MappedColumnName
        {
            get;
            private set;
        }

        public string MappedPropertyName
        {
            get;
            private set;
        }

        public bool CanWrite
        {
            get;
            private set;
        }

        public bool CanRead
        {
            get;
            private set;
        }

        public Type ElementType
        {
            get;
            private set;
        }

        public bool IsNullableType
        {
            get;
            private set;
        }

        public bool IsList
        {
            get;
            private set;
        }
    }
}
