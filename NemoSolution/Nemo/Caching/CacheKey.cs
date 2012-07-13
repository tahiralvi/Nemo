﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Nemo.Extensions;
using Nemo.Reflection;
using Nemo.Utilities;

namespace Nemo.Caching
{
    public class CacheKey
    {
        private static HashSet<string> _algorithms = new HashSet<string>(new[] { "Default", "Native", "MD5", "SHA1", "SHA2", "Jenkins", "Tiger", "FNV", "SBox", "HMAC_SHA1", "None" }, StringComparer.OrdinalIgnoreCase);
        private HashAlgorithmName _hashAlgorithm = ObjectFactory.Configuration.DefaultHashAlgorithm;

        public CacheKey() { }

        public CacheKey(HashAlgorithmName hashAlgorithm, string value)
        {
            _hashAlgorithm = hashAlgorithm == HashAlgorithmName.Default ? ObjectFactory.Configuration.DefaultHashAlgorithm : hashAlgorithm;
            _value = value;
        }

        public CacheKey(HashAlgorithmName hashAlgorithm, byte[] data)
        {
            _hashAlgorithm = hashAlgorithm == HashAlgorithmName.Default ? ObjectFactory.Configuration.DefaultHashAlgorithm : hashAlgorithm;
            _data = data;
        }

        //private string _operation;
        //private string _typeName;
        //private string _keyValue;
        private string _value;
        private byte[] _data;
        //private OperationReturnType _returnType;
        private Tuple<string, byte[]> _hash;

        public CacheKey(IBusinessObject businessObject, string operation, OperationReturnType returnType, HashAlgorithmName hashAlgorithm = HashAlgorithmName.Default)
            : this(businessObject.GetPrimaryKey(true), businessObject.GetType(), operation, returnType, true, hashAlgorithm)
        { }

        public CacheKey(IBusinessObject businessObject, HashAlgorithmName hashAlgorithm = HashAlgorithmName.Default)
            : this(businessObject.GetPrimaryKey(true), businessObject.GetType(), hashAlgorithm)
        { }

        public CacheKey(IDictionary<string, object> key, HashAlgorithmName hashAlgorithm = HashAlgorithmName.Default)
            : this(key, typeof(IBusinessObject), null, OperationReturnType.Guess,  hashAlgorithm)
        { }

        public CacheKey(IDictionary<string, object> key, Type type, HashAlgorithmName hashAlgorithm = HashAlgorithmName.Default)
            : this(key, type, null, OperationReturnType.Guess, hashAlgorithm)
        { }

        internal CacheKey(IEnumerable<Param> parameters, Type type, string operation, OperationReturnType returnType, HashAlgorithmName hashAlgorithm = HashAlgorithmName.Default)
            : this(new SortedDictionary<string, object>(parameters.ToDictionary(p => p.Name, p => p.Value)), type, operation, returnType, hashAlgorithm)
        { }

        internal CacheKey(IEnumerable<Param> parameters, Type type, HashAlgorithmName hashAlgorithm = HashAlgorithmName.Default)
            : this(parameters, type, null, OperationReturnType.Guess, hashAlgorithm)
        { }

        internal CacheKey(IDictionary<string, object> key, Type type, string operation, OperationReturnType returnType, HashAlgorithmName hashAlgorithm = HashAlgorithmName.Default)
            : this(key, type, operation, returnType, key is SortedDictionary<string, object>, hashAlgorithm)
        { }

        protected CacheKey(IDictionary<string, object> key, Type type, string operation, OperationReturnType returnType, bool sorted, HashAlgorithmName hashAlgorithm = HashAlgorithmName.Default)
        {
            var reflectedType = Reflector.GetReflectedType(type);
            var typeName = reflectedType.IsBusinessObject && reflectedType.InterfaceTypeName != null ? reflectedType.InterfaceTypeName : reflectedType.FullTypeName;
            
            _hashAlgorithm = hashAlgorithm == HashAlgorithmName.Default ? ObjectFactory.Configuration.DefaultHashAlgorithm : hashAlgorithm;
            if (_hashAlgorithm == HashAlgorithmName.Native || _hashAlgorithm == HashAlgorithmName.None)
            {
                var keyValue = (sorted ? key.Select(k => string.Format("{0}={1}", k.Key, Uri.EscapeDataString(Convert.ToString(k.Value)))) : key.OrderBy(k => k.Key).Select(k => string.Format("{0}={1}", k.Key, Uri.EscapeDataString(Convert.ToString(k.Value))))).ToDelimitedString("&");
                if (!string.IsNullOrEmpty(operation))
                {
                    _value = string.Concat(typeName, "->", operation, "[", returnType, "]", "::", keyValue);
                }
                else
                {
                    _value = string.Concat(typeName, "::", keyValue);
                }
                _data = _value.ToByteArray();
            }
            else
            {
                Func<KeyValuePair<string, object>, IEnumerable<byte>> func = k => BitConverter.GetBytes(k.Key.GetHashCode()).Append((byte)'=').Concat(BitConverter.GetBytes(k.Value.GetHashCode())).Append((byte)'&');
                var keyValue = (sorted ? key.Select(func) : key.OrderBy(k => k.Key).Select(func)).Flatten();
                if (!string.IsNullOrEmpty(operation))
                {
                    _data = BitConverter.GetBytes(typeName.GetHashCode()).Concat(BitConverter.GetBytes(operation.GetHashCode())).Concat(BitConverter.GetBytes((int)returnType)).Concat(keyValue).ToArray();
                }
                else
                {
                    _data = BitConverter.GetBytes(typeName.GetHashCode()).Concat(keyValue).ToArray();
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is CacheKey)
            {
                return this.GetHashCode() == ((CacheKey)obj).GetHashCode();
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _value != null ? _value.GetHashCode() : (_hash != null ? _hash.Item1.GetHashCode() : base.GetHashCode());
        }

        public override string ToString()
        {
            return _value ?? Value;
        }

        public string Value
        {
            get
            {
                return this.Compute().Item1;
            }
        }

        public HashAlgorithmName Algorithm
        {
            get
            {
                return _hashAlgorithm;
            }
        }

        private byte[] ComputeHash(HashAlgorithm algorithm)
        {
            return algorithm.ComputeHash(_data);
        }
        
        public Tuple<string, byte[]> Compute(int maxSize = 250)
        {
            if (_hash == null)
            {
                byte[] data = null;
                string value = null;
                switch (Algorithm)
                {
                    case HashAlgorithmName.MD5:
                        data = this.ComputeHash(MD5.Create());
                        break;
                    case HashAlgorithmName.SHA1:
                        data = this.ComputeHash(SHA1.Create());
                        break;
                    case HashAlgorithmName.SHA2:
                        data = this.ComputeHash(SHA256.Create());
                        break;
                    case HashAlgorithmName.HMAC_SHA1:
                        data = this.ComputeHash(new HMACSHA1(ObjectFactory.Configuration.SecretKey.ToByteArray()));
                        break;
                    case HashAlgorithmName.Default:
                    case HashAlgorithmName.Jenkins:
                        var h1 = Hash.Jenkins96.Compute(_data);
                        data = BitConverter.GetBytes(h1);
                        break;
                    case HashAlgorithmName.SBox:
                        var h2 = Hash.SBox.Compute(_data);
                        data = BitConverter.GetBytes(h2);
                        break;
                    case HashAlgorithmName.Tiger:
                        data = this.ComputeHash(Enyim.TigerHash.Create());
                        break;
                    case HashAlgorithmName.FNV:
                        data = this.ComputeHash(Enyim.ModifiedFNV.Create());
                        break;
                    case HashAlgorithmName.None:
                        value = maxSize > _value.Length ? _value : _value.Substring(0, maxSize);
                        data = Encoding.Default.GetBytes(value);
                        break;
                    case HashAlgorithmName.Native:
                        var h3 = this.GetHashCode();
                        data = BitConverter.GetBytes(h3);
                        break;
                }
                _hash = Tuple.Create(value ?? Bytes.ToHex(data), data);
            }
            return _hash;
        }

        private static string GetTypeName(Type type)
        {
            if (!type.IsInterface)
            {
                return Reflector.ExtractInterface(type).FullName;
            }
            else
            {
                return type.FullName;
            }
        }
    }
}
