﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Nemo.Collections;
using System.Linq.Expressions;
using Nemo.Reflection;
using Nemo.Extensions;
using System.Collections.Concurrent;

namespace Nemo
{
    public class Param
    {
        public Param()
        {
            Direction = ParameterDirection.Input;
            Size = -1;
        }

        public Param(Param parameter) 
        {
            Name = parameter.Name;
            Direction = parameter.Direction;
            Value = parameter.Value;
            DbType = parameter.DbType;
            Size = parameter.Size;
        }

        public string Name { get; set; }
        public ParameterDirection Direction { get; set; }
        public object Value { get; set; }
        public DbType? DbType { get; set; }
        public Type Type
        {
            get
            {
                return Value != null ? Value.GetType() : null;
            }
        }
        public int Size { get; set; }
        public string Source { get; set; }
        public bool IsAutoGenerated { get; set; }
    }

    public class ParamList : List<Expression<Func<object, object>>> 
    {
        private static ConcurrentDictionary<Tuple<Type, string, string, Type>, Func<object, object>> _parameterCache = new ConcurrentDictionary<Tuple<Type, string, string, Type>, Func<object, object>>();

        internal Param[] ExtractParameters(Type objectType, string operation)
        {
            var result = new List<Param>();
            foreach (var expression in this.ToList())
            {
                var key = Tuple.Create(objectType, operation, expression.Parameters[0].Name, expression.Body.Type); 
                Func<Tuple<Type, string, string, Type>, Func<object, object>> valueFactory = t => expression.Compile();
                var func = _parameterCache.GetOrAdd(key, valueFactory);

                var parameter = new Param { Name = key.Item3};

                if (Reflector.IsAnonymousType(key.Item4))
                {
                    parameter = new Param(Adapter.Bind<Param>(func(null)));
                    parameter.Name = parameter.Name ?? key.Item3;
                }
                else if (key.Item4.InheritsFrom(typeof(Param)))
                {
                    parameter = (Param)func(null);
                    parameter.Name = parameter.Name ?? key.Item3;
                }
                else
                {
                    parameter.Value = func(null);
                }

                result.Add(parameter);
            }
            return result.ToArray();
        }
    }
}
