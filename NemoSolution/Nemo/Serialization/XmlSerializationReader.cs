﻿using Nemo.Collections;
using Nemo.Extensions;
using Nemo.Fn;
using Nemo.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Nemo.Serialization
{
    public static class XmlSerializationReader
    {
        private static object ReadValue(XmlReader reader, Type objectType, string name)
        {
            object value = null;
            var hasValue = false;
            if (name != null && reader.HasAttributes)
            {
                var attrValue = reader.GetAttribute(name);
                hasValue = !string.IsNullOrEmpty(attrValue);
                attrValue.TryParse(objectType, out value);
            }

            if (!hasValue)
            {
                value = reader.ReadElementContentAs(objectType, null);
            }
            return value;
        }

        public static object ReadObject(XmlReader reader, Type objectType, out bool isArray)
        {
            var states = new LinkedList<SerializationReaderState>();
            object result = null;
            isArray = false;

            IBusinessObject item = null;
            IList list = null;
            ITypeUnion union = null;
            Type elementType = null;
            IDictionary<string, ReflectedProperty> propertyMap = null;
            var isSimple = false;
            string name = null;

            if (reader.IsStartElement())
            {
                name = reader.Name;
                isArray = name.StartsWith("ArrayOf");
                var reflectedType = Reflector.GetReflectedType(objectType);
                if (isArray || reflectedType.IsList)
                {
                    elementType = reflectedType.IsList ? reflectedType.ElementType : objectType;
                    isSimple = reflectedType.IsList ? reflectedType.IsSimpleList : reflectedType.IsSimpleType;
                    propertyMap = Reflector.GetPropertyNameMap(elementType);
                    if (!objectType.IsInterface)
                    {
                        list = (IList)Nemo.Reflection.Activator.New(objectType);
                    }
                    else
                    {
                        list = List.Create(elementType);
                    }
                    result = list;
                }
                else if (reflectedType.IsBusinessObject)
                {
                    propertyMap = Reflector.GetPropertyNameMap(objectType);
                    item = (IBusinessObject)ObjectFactory.Create(objectType);
                    result = item;
                    // Handle attributes
                    if (reader.HasAttributes)
                    {
                        for (int i = 0; i < reader.AttributeCount; i++)
                        {
                            reader.MoveToAttribute(i);
                            ReflectedProperty property;
                            var propertyName = reader.Name;
                            if (propertyMap.TryGetValue(propertyName, out property))
                            {
                                object value = ReadValue(reader, property.PropertyType, propertyName);
                                if (value != null)
                                {
                                    item.Property(propertyName, value);
                                }
                            }
                        }
                    }
                }
                else if (reflectedType.IsSimpleType)
                {
                    result = ReadValue(reader, objectType, null);
                    isSimple = true;
                }
                reader.Read();
            }

            if (isSimple && list == null)
            {
                return result;
            }

            states.AddLast(new SerializationReaderState { Name = name, Item = item, List = list, Union = union, ElementType = elementType, PropertyMap = propertyMap, IsSimple = isSimple });

            while (reader.IsStartElement())
            {
                name = reader.Name;
                var lastState = states.Last;
                var currentValue = lastState.Value;
                var currentMap = currentValue.PropertyMap;
                ReflectedProperty property;
                if (currentMap != null && currentMap.TryGetValue(name, out property))
                {
                    if (property.IsBusinessObject)
                    {
                        propertyMap = Reflector.GetPropertyNameMap(property.PropertyType);
                        item = (IBusinessObject)ObjectFactory.Create(property.PropertyType);
                        states.AddLast(new SerializationReaderState { Name = name, Item = item, PropertyMap = propertyMap });

                        if (currentValue.Item != null)
                        {
                            currentValue.Item.Property(name, item);
                        }
                        else if (currentValue.Union != null)
                        {
                            currentValue.Union = TypeUnion.Create(currentValue.Union.AllTypes, item);
                        }
                        else
                        {
                            currentValue.List.Add(item);
                        }
                    }
                    else if (property.IsBusinessObjectList)
                    {
                        elementType = property.ElementType;
                        propertyMap = Reflector.GetPropertyNameMap(elementType);
                        if (!property.IsListInterface)
                        {
                            list = (IList)Nemo.Reflection.Activator.New(property.PropertyType);
                        }
                        else
                        {
                            list = List.Create(elementType, property.Distinct, property.Sorted);
                        }
                        states.AddLast(new SerializationReaderState { Name = name, List = list, ElementType = elementType, PropertyMap = propertyMap });

                        if (currentValue.Item != null)
                        {
                            currentValue.Item.Property(name, list);
                        }
                    }
                    else if (property.IsTypeUnion)
                    {
                        var unionTypeEmpty = reader.GetAttribute("__empty") == "true";
                        if (unionTypeEmpty) continue;
                        
                        var allTypes = property.PropertyType.GetGenericArguments();
                        var unionTypeIndex = int.Parse(reader.GetAttribute("__index")) - 1;
                        var unionType = allTypes.ElementAtOrDefault(unionTypeIndex);
                        if (unionType != null)
                        {
                            var unionTypeReflected = Reflector.GetReflectedType(unionType);
                            object unionValue = null;
                            if (unionType == typeof(string))
                            {
                                unionValue = string.Empty;
                            }
                            else if (unionTypeReflected.IsSimpleType)
                            {
                                unionValue = unionType.GetDefault();
                            }
                            else if (unionTypeReflected.IsBusinessObject)
                            {
                                unionValue = ObjectFactory.Create(unionType);
                            }
                            else if (unionTypeReflected.IsList)
                            {
                                unionValue = List.Create(unionTypeReflected.ElementType);
                            }
                            else
                            {
                                unionValue = Nemo.Reflection.Activator.New(unionType);
                            }
                            union = TypeUnion.Create(allTypes, unionValue);
                            states.AddLast(new SerializationReaderState { Name = name, Union = union, ElementType = unionType });
                            if (currentValue.Item != null)
                            {
                                currentValue.Item.Property(name, union);
                            }
                        }
                    }
                    else
                    {
                        object value = ReadValue(reader, currentValue.ElementType ?? property.PropertyType, name);
                        if (value != null)
                        {
                            if (!currentValue.IsSimple)
                            {
                                currentValue.Item.Property(property.PropertyName, value);
                            }
                            else if (currentValue.List != null)
                            {
                                currentValue.List.Add(value);
                            }
                            else
                            {
                                currentValue.Value = value;
                            }
                        }
                        if (reader.NodeType != XmlNodeType.EndElement)
                        {
                            continue;
                        }
                    }
                }
                else if (currentValue.Union != null)
                {
                    var previousState = lastState.Previous;
                    if (previousState.Value.Item != null)
                    {
                        var currentUnion = currentValue.Union;
                        var unionType = currentUnion.UnionType;
                        for (int i = 0; i < currentUnion.AllTypes.Length; i++)
                        {
                            reader.ReadStartElement();
                            if (unionType == currentUnion.AllTypes[i])
                            {
                                var reflectedType = Reflector.GetReflectedType(unionType);
                                var subtree = reader.ReadSubtree();
                                var isArraySybtree = false;
                                var value = ReadObject(subtree, unionType, out isArraySybtree);
                                currentUnion = TypeUnion.Create(currentUnion.AllTypes, value);
                                previousState.Value.Item.Property(currentValue.Name, currentUnion);
                                reader.Read();
                                reader.ReadEndElement();
                            }
                        }
                    }
                }
                else if (currentValue.ElementType != null)
                {
                    if (currentValue.IsSimple)
                    {
                        object value = ReadValue(reader, currentValue.ElementType, name);
                        if (currentValue.List != null)
                        {
                            currentValue.List.Add(value);
                        }
                        else
                        {
                            currentValue.Value = value;
                        }
                        if (reader.NodeType != XmlNodeType.EndElement)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        item = (IBusinessObject)ObjectFactory.Create(currentValue.ElementType);
                        if (currentValue.List != null)
                        {
                            currentValue.List.Add(item);
                        }
                        else
                        {
                            currentValue.Value = item;
                        }
                        states.AddLast(new SerializationReaderState { Name = name, Item = item, PropertyMap = currentValue.PropertyMap });
                    }
                }

                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    while (reader.NodeType == XmlNodeType.EndElement)
                    {
                        if (states.Count > 0)
                        {
                            states.RemoveLast();
                        }
                        reader.Read();
                    }
                }
                else
                {
                    reader.Read();
                }
            }
            return result;
        }
    }
}
