﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Nemo.Attributes;
using Nemo.Collections;
using Nemo.Collections.Extensions;
using Nemo.Configuration;
using Nemo.Configuration.Mapping;
using Nemo.Data;
using Nemo.Extensions;
using Nemo.Fn;
using Nemo.Fn.Extensions;
using Nemo.Reflection;
using Nemo.Utilities;

namespace Nemo
{
    public static partial class ObjectFactory
    {
        #region Count Methods

        public static async Task<int> CountAsync<T>(Expression<Func<T, bool>> predicate = null, string connectionName = null, DbConnection connection = null)
            where T : class
        {
            string providerName = null;
            if (connection == null)
            {
                providerName = DbFactory.GetProviderInvariantName(connectionName, typeof(T));
                connection = DbFactory.CreateConnection(connectionName, typeof(T));
            }
            var sql = SqlBuilder.GetSelectCountStatement(predicate, DialectFactory.GetProvider(connection, providerName));
            return await RetrieveScalarAsync<int>(sql, connection: connection);
        }

        #endregion

        #region Select Methods

        public static async Task<IEnumerable<T>> ToEnumerableAsync<T>(this IAsyncEnumerable<T> source)
            where T : class
        {
            if (!(source is EagerLoadEnumerableAsync<T> loader)) return source.ToEnumerable();

            var iterator = await loader.GetEnumeratorAsync().ConfigureAwait(false);
            return iterator.AsEnumerable();
        }

        public static IAsyncEnumerable<T> SelectAsync<T>(Expression<Func<T, bool>> predicate = null, string connectionName = null, DbConnection connection = null, int page = 0, int pageSize = 0, bool? cached = null,
            SelectOption selectOption = SelectOption.All, params Sorting<T>[] orderBy)
            where T : class
        {
            string providerName = null;
            if (connection == null)
            {
                providerName = DbFactory.GetProviderInvariantName(connectionName, typeof(T));
                connection = DbFactory.CreateConnection(connectionName, typeof(T));
            }

            var provider = DialectFactory.GetProvider(connection, providerName);

            var sql = SqlBuilder.GetSelectStatement(predicate, page, pageSize, selectOption != SelectOption.All, provider, orderBy);

            var result = new EagerLoadEnumerableAsync<T>(new[] { sql }, new[] { typeof(T) },
                (s, t) => RetrieveImplemenationAsync<T>(s, OperationType.Sql, null, OperationReturnType.SingleResult, connectionName, connection, types: t, cached: cached), predicate, provider, selectOption);

            return result;
        }

        private static IAsyncEnumerable<T> SelectAsync<T, T1>(Expression<Func<T, T1, bool>> join, Expression<Func<T, bool>> predicate = null, string connectionName = null, DbConnection connection = null, DialectProvider provider = null, int page = 0, int pageSize = 0,
            bool? cached = null, SelectOption selectOption = SelectOption.All, params Sorting<T>[] orderBy)
            where T : class
            where T1 : class
        {
            if (provider == null)
            {
                string providerName = null;
                if (connection == null)
                {
                    providerName = DbFactory.GetProviderInvariantName(connectionName, typeof(T));
                    connection = DbFactory.CreateConnection(connectionName, typeof(T));
                }

                provider = DialectFactory.GetProvider(connection, providerName);
            }

            var sqlRoot = SqlBuilder.GetSelectStatement(predicate, page, pageSize, selectOption != SelectOption.All, provider, orderBy);
            var sqlJoin = SqlBuilder.GetSelectStatement(predicate, join, 0, 0, false, provider, orderBy);

            var result = new EagerLoadEnumerableAsync<T>(new[] { sqlRoot, sqlJoin }, new[] { typeof(T), typeof(T1) },
                (s, t) => RetrieveImplemenationAsync<T>(s, OperationType.Sql, null, OperationReturnType.MultiResult, connectionName, connection, types: t, cached: cached), predicate, provider, selectOption);

            return result;
        }

        private static IAsyncEnumerable<T> SelectAsync<T, T1, T2>(Expression<Func<T, T1, bool>> join1, Expression<Func<T1, T2, bool>> join2,
            Expression<Func<T, bool>> predicate = null, string connectionName = null, DbConnection connection = null, DialectProvider provider = null, int page = 0, int pageSize = 0, bool? cached = null,
            SelectOption selectOption = SelectOption.All, params Sorting<T>[] orderBy)
            where T : class
            where T1 : class
            where T2 : class
        {
            if (provider == null)
            {
                string providerName = null;
                if (connection == null)
                {
                    providerName = DbFactory.GetProviderInvariantName(connectionName, typeof(T));
                    connection = DbFactory.CreateConnection(connectionName, typeof(T));
                }

                provider = DialectFactory.GetProvider(connection, providerName);
            }

            var sqlRoot = SqlBuilder.GetSelectStatement(predicate, page, pageSize, selectOption != SelectOption.All, provider, orderBy);
            var sqlJoin1 = SqlBuilder.GetSelectStatement(predicate, join1, 0, 0, false, provider, orderBy);
            var sqlJoin2 = SqlBuilder.GetSelectStatement(predicate, join1, join2, 0, 0, false, provider, orderBy);

            var result = new EagerLoadEnumerableAsync<T>(new[] { sqlRoot, sqlJoin1, sqlJoin2 }, new[] { typeof(T), typeof(T1), typeof(T2) },
                (s, t) => RetrieveImplemenationAsync<T>(s, OperationType.Sql, null, OperationReturnType.MultiResult, connectionName, connection, types: t, cached: cached), predicate, provider, selectOption);

            return result;
        }

        internal static IAsyncEnumerable<T> SelectAsync<T, T1, T2, T3>(Expression<Func<T, T1, bool>> join1, Expression<Func<T1, T2, bool>> join2, Expression<Func<T2, T3, bool>> join3,
            Expression<Func<T, bool>> predicate = null, string connectionName = null, DbConnection connection = null, DialectProvider provider = null, int page = 0, int pageSize = 0, bool? cached = null,
            SelectOption selectOption = SelectOption.All, params Sorting<T>[] orderBy)
            where T : class
            where T1 : class
            where T2 : class
            where T3 : class
        {
            if (provider == null)
            {
                string providerName = null;
                if (connection == null)
                {
                    providerName = DbFactory.GetProviderInvariantName(connectionName, typeof(T));
                    connection = DbFactory.CreateConnection(connectionName, typeof(T));
                }

                provider = DialectFactory.GetProvider(connection, providerName);
            }

            var sqlRoot = SqlBuilder.GetSelectStatement(predicate, page, pageSize, selectOption != SelectOption.All, provider, orderBy);
            var sqlJoin1 = SqlBuilder.GetSelectStatement(predicate, join1, 0, 0, false, provider, orderBy);
            var sqlJoin2 = SqlBuilder.GetSelectStatement(predicate, join1, join2, 0, 0, false, provider, orderBy);
            var sqlJoin3 = SqlBuilder.GetSelectStatement(predicate, join1, join2, join3, 0, 0, false, provider, orderBy);

            var result = new EagerLoadEnumerableAsync<T>(new[] { sqlRoot, sqlJoin1, sqlJoin2, sqlJoin3 }, new[] { typeof(T), typeof(T1), typeof(T2), typeof(T3) },
                (s, t) => RetrieveImplemenationAsync<T>(s, OperationType.Sql, null, OperationReturnType.MultiResult, connectionName, connection, types: t, cached: cached), predicate, provider, selectOption);

            return result;
        }

        private static IAsyncEnumerable<T> SelectAsync<T, T1, T2, T3, T4>(Expression<Func<T, T1, bool>> join1, Expression<Func<T1, T2, bool>> join2, Expression<Func<T2, T3, bool>> join3, Expression<Func<T3, T4, bool>> join4,
            Expression<Func<T, bool>> predicate = null, string connectionName = null, DbConnection connection = null, DialectProvider provider = null, int page = 0, int pageSize = 0, bool? cached = null,
            SelectOption selectOption = SelectOption.All, params Sorting<T>[] orderBy)
            where T : class
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
        {
            if (provider == null)
            {
                string providerName = null;
                if (connection == null)
                {
                    providerName = DbFactory.GetProviderInvariantName(connectionName, typeof(T));
                    connection = DbFactory.CreateConnection(connectionName, typeof(T));
                }

                provider = DialectFactory.GetProvider(connection, providerName);
            }

            var sqlRoot = SqlBuilder.GetSelectStatement(predicate, page, pageSize, selectOption != SelectOption.All, provider, orderBy);
            var sqlJoin1 = SqlBuilder.GetSelectStatement(predicate, join1, 0, 0, false, provider, orderBy);
            var sqlJoin2 = SqlBuilder.GetSelectStatement(predicate, join1, join2, 0, 0, false, provider, orderBy);
            var sqlJoin3 = SqlBuilder.GetSelectStatement(predicate, join1, join2, join3, 0, 0, false, provider, orderBy);
            var sqlJoin4 = SqlBuilder.GetSelectStatement(predicate, join1, join2, join3, join4, 0, 0, false, provider, orderBy);

            var result = new EagerLoadEnumerableAsync<T>(new[] { sqlRoot, sqlJoin1, sqlJoin2, sqlJoin3, sqlJoin4 }, new[] { typeof(T), typeof(T1), typeof(T2), typeof(T3), typeof(T4) },
                (s, t) => RetrieveImplemenationAsync<T>(s, OperationType.Sql, null, OperationReturnType.MultiResult, connectionName, connection, types: t, cached: cached), predicate, provider, selectOption);

            return result;
        }

        private static IAsyncEnumerable<T> UnionAsync<T>(params IAsyncEnumerable<T>[] sources)
            where T : class
        {
            IAsyncEnumerable<T> first = null;
            foreach (var source in sources)
            {
                if (first == null)
                {
                    first = source;
                }
                else if (first is EagerLoadEnumerableAsync<T>)
                {
                    first = ((EagerLoadEnumerableAsync<T>)first).Union(source);
                }
                else
                {
                    first = first.Union(source);
                }
            }
            return first ?? AsyncEnumerable.Empty<T>();
        }

        public static IAsyncEnumerable<TSource> IncludeAsync<TSource, TInclude>(this IAsyncEnumerable<TSource> source, Expression<Func<TSource, TInclude, bool>> join)
            where TSource : class
            where TInclude : class
        {
            if (source is EagerLoadEnumerableAsync<TSource> eagerSource)
            {
                return UnionAsync(source, SelectAsync(join, eagerSource.Predicate, provider: eagerSource.Provider, selectOption: eagerSource.SelectOption));
            }
            return source;
        }

        public static IAsyncEnumerable<TSource> IncludeAsync<TSource, TInclude1, TInclude2>(this IAsyncEnumerable<TSource> source, Expression<Func<TSource, TInclude1, bool>> join1, Expression<Func<TInclude1, TInclude2, bool>> join2)
            where TSource : class
            where TInclude1 : class
            where TInclude2 : class
        {
            if (source is EagerLoadEnumerableAsync<TSource> eagerSource)
            {
                return UnionAsync(source, SelectAsync(join1, join2, eagerSource.Predicate, provider: eagerSource.Provider, selectOption: eagerSource.SelectOption));
            }
            return source;
        }

        public static IAsyncEnumerable<TSource> IncludeAsync<TSource, TInclude1, TInclude2, TInclude3>(this IAsyncEnumerable<TSource> source, Expression<Func<TSource, TInclude1, bool>> join1, Expression<Func<TInclude1, TInclude2, bool>> join2, Expression<Func<TInclude2, TInclude3, bool>> join3)
            where TSource : class
            where TInclude1 : class
            where TInclude2 : class
            where TInclude3 : class
        {
            if (source is EagerLoadEnumerableAsync<TSource> eagerSource)
            {
                return UnionAsync(source, SelectAsync(join1, join2, join3, eagerSource.Predicate, provider: eagerSource.Provider, selectOption: eagerSource.SelectOption));
            }
            return source;
        }

        public static IAsyncEnumerable<TSource> IncludeAsync<TSource, TInclude1, TInclude2, TInclude3, TInclude4>(this IAsyncEnumerable<TSource> source, Expression<Func<TSource, TInclude1, bool>> join1, Expression<Func<TInclude1, TInclude2, bool>> join2, Expression<Func<TInclude2, TInclude3, bool>> join3, Expression<Func<TInclude3, TInclude4, bool>> join4)
            where TSource : class
            where TInclude1 : class
            where TInclude2 : class
            where TInclude3 : class
            where TInclude4 : class
        {
            if (source is EagerLoadEnumerableAsync<TSource> eagerSource)
            {
                return UnionAsync(source, SelectAsync(join1, join2, join3, join4, eagerSource.Predicate, provider: eagerSource.Provider, selectOption: eagerSource.SelectOption));
            }
            return source;
        }

        #endregion

        #region Retrieve Methods

        public static async Task<T> RetrieveScalarAsync<T>(string sql, Param[] parameters = null, string connectionName = null, DbConnection connection = null, string schema = null)
            where T : struct
        {
            var response = connection != null
                ? await ExecuteAsync(sql, parameters, OperationReturnType.Scalar, OperationType.Sql, connection: connection, schema: schema)
                : await ExecuteAsync(sql, parameters, OperationReturnType.Scalar, OperationType.Sql, connectionName: connectionName, schema: schema);

            var value = response.Value;
            if (value == null)
            {
                return default(T);
            }

            return (T)Reflector.ChangeType(value, typeof(T));
        }

        private static async Task<IEnumerable<TResult>> RetrieveImplemenationAsync<TResult>(string operation, OperationType operationType, IList<Param> parameters, OperationReturnType returnType, string connectionName, DbConnection connection, Func<object[], TResult> map = null, IList<Type> types = null, MaterializationMode mode = MaterializationMode.Default, string schema = null, bool? cached = null, IConfiguration config = null)
            where TResult : class
        {
            Log.CaptureBegin(() => $"RetrieveImplemenation: {typeof(TResult).FullName}::{operation}");
            IEnumerable<TResult> result;

            string queryKey = null;
            IdentityMap<TResult> identityMap = null;

            if (!cached.HasValue)
            {
                if (config == null)
                {
                    config = ConfigurationFactory.Get<TResult>();
                }

                cached = config.DefaultCacheRepresentation != CacheRepresentation.None;
            }

            if (cached.Value)
            {
                if (config == null)
                {
                    config = ConfigurationFactory.Get<TResult>();
                }

                queryKey = GetQueryKey<TResult>(operation, parameters ?? new Param[] { }, returnType);

                Log.CaptureBegin(() => $"Retrieving from L1 cache: {queryKey}");

                if (returnType == OperationReturnType.MultiResult)
                {
                    result = config.ExecutionContext.Get(queryKey) as IEnumerable<TResult>;
                }
                else
                {
                    identityMap = Identity.Get<TResult>();
                    result = identityMap.GetIndex(queryKey);
                }

                Log.CaptureEnd();

                if (result != null)
                {
                    Log.Capture(() => $"Found in L1 cache: {queryKey}");

                    if (returnType == OperationReturnType.MultiResult)
                    {
                        ((IMultiResult)result).Reset();
                    }

                    Log.CaptureEnd();
                    return result;
                }
                Log.Capture(() => $"Not found in L1 cache: {queryKey}");
            }

            result = await RetrieveItemsAsync(operation, parameters, operationType, returnType, connectionName, connection, types, map, cached.Value, mode, schema, config, identityMap);

            if (queryKey != null)
            {
                Log.CaptureBegin(() => $"Saving to L1 cache: {queryKey}");

                if (!(result is IList<TResult>) && !(result is IMultiResult))
                {
                    if (config.DefaultCacheRepresentation == CacheRepresentation.List)
                    {
                        result = result.ToList();
                    }
                    else
                    {
                        result = result.AsStream();
                    }
                }

                if (identityMap != null)
                {
                    result = identityMap.AddIndex(queryKey, result);
                }
                else if (result is IMultiResult)
                {
                    config.ExecutionContext.Set(queryKey, result);
                }

                Log.CaptureEnd();
            }

            Log.CaptureEnd();
            return result;
        }

        private static async Task<IEnumerable<T>> RetrieveItemsAsync<T>(string operation, IList<Param> parameters, OperationType operationType, OperationReturnType returnType, string connectionName, DbConnection connection, IList<Type> types, Func<object[], T> map, bool cached, MaterializationMode mode, string schema, IConfiguration config, IdentityMap<T> identityMap)
            where T : class
        {
            if (operationType == OperationType.Guess)
            {
                operationType = operation.Any(char.IsWhiteSpace) ? OperationType.Sql : OperationType.StoredProcedure;
            }

            var operationText = GetOperationText(typeof(T), operation, operationType, schema, config);

            var response = connection != null
                ? await ExecuteAsync(operationText, parameters, returnType, connection: connection, operationType: operationType, types: types, schema: schema)
                : await ExecuteAsync(operationText, parameters, returnType, connectionName: connectionName, operationType: operationType, types: types, schema: schema);

            var result = Translate(response, map, types, cached, mode, identityMap);
            return result;
        }

        /// <summary>
        /// Retrieves an enumerable of type T using provided rule parameters.
        /// </summary>
        /// <returns></returns>
        public static async Task<IEnumerable<TResult>> RetrieveAsync<TResult, T1, T2, T3, T4>(string operation = OperationRetrieve, string sql = null, object parameters = null, Func<TResult, T1, T2, T3, T4, TResult> map = null, string connectionName = null, DbConnection connection = null, MaterializationMode materialization = MaterializationMode.Default, string schema = null, bool? cached = null)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
            where TResult : class
        {
            var fakeType = typeof(Fake);
            var realTypes = new List<Type> { typeof(TResult) };
            if (fakeType != typeof(T1))
            {
                realTypes.Add(typeof(T1));
            }
            if (fakeType != typeof(T2))
            {
                realTypes.Add(typeof(T2));
            }
            if (fakeType != typeof(T3))
            {
                realTypes.Add(typeof(T3));
            }
            if (fakeType != typeof(T4))
            {
                realTypes.Add(typeof(T4));
            }

            IConfiguration config = null;
            if (materialization == MaterializationMode.Default)
            {
                config = ConfigurationFactory.Get<TResult>();
                materialization = config.DefaultMaterializationMode;
            }

            var returnType = OperationReturnType.SingleResult;

            Func<object[], TResult> func = null;
            if (map == null && realTypes.Count > 1)
            {
                returnType = OperationReturnType.MultiResult;
            }
            else if (map != null && realTypes.Count > 1)
            {
                switch (realTypes.Count)
                {
                    case 5:
                        func = args => map((TResult)args[0], (T1)args[1], (T2)args[2], (T3)args[3], (T4)args[4]);
                        break;
                    case 4:
                        func = args => map.Curry((TResult)args[0], (T1)args[1], (T2)args[2], (T3)args[3])(null);
                        break;
                    case 3:
                        func = args => map.Curry((TResult)args[0], (T1)args[1], (T2)args[2])(null, null);
                        break;
                    case 2:
                        func = args => map.Curry((TResult)args[0], (T1)args[1])(null, null, null);
                        break;
                }
            }
            
            var command = sql ?? operation;
            var commandType = sql == null ? OperationType.StoredProcedure : OperationType.Sql;
            IList<Param> parameterList = null;
            if (parameters != null)
            {
                switch (parameters)
                {
                    case ParamList list:
                        parameterList = list.GetParameters(typeof(TResult), operation);
                        break;
                    case Param[] array:
                        parameterList = array;
                        break;
                }
            }
            return await RetrieveImplemenationAsync(command, commandType, parameterList, returnType, connectionName, connection, func, realTypes, materialization, schema, cached, config);
        }

        public static async Task<IEnumerable<TResult>> RetrieveAsync<TResult, T1, T2, T3>(string operation = OperationRetrieve, string sql = null, object parameters = null, Func<TResult, T1, T2, T3, TResult> map = null, string connectionName = null, DbConnection connection = null, MaterializationMode materialization = MaterializationMode.Default, string schema = null, bool? cached = null)
            where T1 : class
            where T2 : class
            where T3 : class
            where TResult : class
        {
            var newMap = map != null ? (t, t1, t2, t3, f4) => map(t, t1, t2, t3) : (Func<TResult, T1, T2, T3, Fake, TResult>)null;
            return await RetrieveAsync(operation, sql, parameters, newMap, connectionName, connection, materialization, schema, cached);
        }

        public static async Task<IEnumerable<TResult>> RetrieveAsync<TResult, T1, T2>(string operation = OperationRetrieve, string sql = null, object parameters = null, Func<TResult, T1, T2, TResult> map = null, string connectionName = null, DbConnection connection = null, MaterializationMode materialization = MaterializationMode.Default, string schema = null, bool? cached = null)
            where T1 : class
            where T2 : class
            where TResult : class
        {
            var newMap = map != null ? (t, t1, t2, f3, f4) => map(t, t1, t2) : (Func<TResult, T1, T2, Fake, Fake, TResult>)null;
            return await RetrieveAsync(operation, sql, parameters, newMap, connectionName, connection, materialization, schema, cached);
        }

        public static async Task<IEnumerable<TResult>> RetrieveAsync<TResult, T1>(string operation = OperationRetrieve, string sql = null, object parameters = null, Func<TResult, T1, TResult> map = null, string connectionName = null, DbConnection connection = null, MaterializationMode materialization = MaterializationMode.Default, string schema = null, bool? cached = null)
            where T1 : class
            where TResult : class
        {
            var newMap = map != null ? (t, t1, f1, f2, f3) => map(t, t1) : (Func<TResult, T1, Fake, Fake, Fake, TResult>)null;
            return await RetrieveAsync(operation, sql, parameters, newMap, connectionName, connection, materialization, schema, cached);
        }

        public static async Task<IEnumerable<T>> RetrieveAsync<T>(string operation = OperationRetrieve, string sql = null, object parameters = null, string connectionName = null, DbConnection connection = null, MaterializationMode materialization = MaterializationMode.Default, string schema = null, bool? cached = null)
            where T : class
        {
            IConfiguration config = null;
            if (materialization == MaterializationMode.Default)
            {
                config = ConfigurationFactory.Get<T>();
                materialization = config.DefaultMaterializationMode;
            }
            
            var command = sql ?? operation;
            var commandType = sql == null ? OperationType.StoredProcedure : OperationType.Sql;
            IList<Param> parameterList = null;
            if (parameters != null)
            {
                switch (parameters)
                {
                    case ParamList list:
                        parameterList = list.GetParameters(typeof(T), operation);
                        break;
                    case Param[] array:
                        parameterList = array;
                        break;
                }
            }
            return await RetrieveImplemenationAsync<T>(command, commandType, parameterList, OperationReturnType.SingleResult, connectionName, connection, null, new[] { typeof(T) }, materialization, schema, cached, config);
        }

        #endregion

        #region Insert/Update/Delete/Execute Methods

        public static async Task<long> InsertAsync<T>(IEnumerable<T> items, string connectionName = null, DbConnection connection = null, DbTransaction transaction = null, bool captureException = false)
           where T : class
        {
            var count = 0L;
            var connectionOpenedHere = false;
            var externalTransaction = transaction != null;
            var externalConnection = externalTransaction || connection != null;
            var config = ConfigurationFactory.Get<T>();
            if (externalTransaction)
            {
                connection = transaction.Connection;
            }
            if (!externalConnection)
            {
                connection = DbFactory.CreateConnection(connectionName ?? config.DefaultConnectionName);
            }

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                    connectionOpenedHere = true;
                }
                if (transaction == null)
                {
                    transaction = connection.BeginTransaction();
                }

                var propertyMap = Reflector.GetPropertyMap<T>();
                var provider = DialectFactory.GetProvider(transaction.Connection);

                var requests = BuildBatchInsert(items, transaction, captureException, propertyMap, provider);
                foreach (var request in requests)
                {
                    var response = await ExecuteAsync<T>(request);
                    if (!response.HasErrors)
                    {
                        count += response.RecordsAffected;
                    }
                }
                transaction.Commit();

                return count;
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                throw;
            }
            finally
            {
                if (connectionOpenedHere)
                {
                    connection.Clone();
                }

                if (!externalConnection)
                {
                    connection.Dispose();
                }
            }
        }

        public static async Task<OperationResponse> InsertAsync<T>(ParamList parameters, string connectionName = null, bool captureException = false, string schema = null, DbConnection connection = null)
            where T : class
        {
            return await InsertAsync<T>(parameters.GetParameters(typeof(T), OperationInsert), connectionName, captureException, schema, connection);
        }

        public static async Task<OperationResponse> InsertAsync<T>(Param[] parameters, string connectionName = null, bool captureException = false, string schema = null, DbConnection connection = null)
            where T : class
        {
            var request = new OperationRequest { Parameters = parameters, ReturnType = OperationReturnType.NonQuery, ConnectionName = connectionName, Connection = null, CaptureException = captureException };
            var config = ConfigurationFactory.Get<T>();
            if (config.GenerateInsertSql)
            {
                request.Operation = SqlBuilder.GetInsertStatement(typeof(T), parameters, request.Connection != null ? DialectFactory.GetProvider(request.Connection) : DialectFactory.GetProvider(request.ConnectionName ?? config.DefaultConnectionName));
                request.OperationType = OperationType.Sql;
            }
            else
            {
                request.Operation = OperationInsert;
                request.OperationType = OperationType.StoredProcedure;
                request.SchemaName = schema;
            }
            var response = await ExecuteAsync<T>(request);
            return response;
        }

        public static async Task<OperationResponse> UpdateAsync<T>(ParamList parameters, string connectionName = null, bool captureException = false, string schema = null, DbConnection connection = null)
            where T : class
        {
            return await UpdateAsync<T>(parameters.GetParameters(typeof(T), OperationUpdate), connectionName, captureException, schema, connection);
        }

        public static async Task<OperationResponse> UpdateAsync<T>(Param[] parameters, string connectionName = null, bool captureException = false, string schema = null, DbConnection connection = null)
            where T : class
        {
            var request = new OperationRequest { Parameters = parameters, ReturnType = OperationReturnType.NonQuery, ConnectionName = connectionName, Connection = connection, CaptureException = captureException };
            var config = ConfigurationFactory.Get<T>();
            if (config.GenerateUpdateSql)
            {
                var partition = parameters.Partition(p => p.IsPrimaryKey);
                // if p.IsPrimaryKey is not set then
                // we need to infer it from reflected property 
                if (partition.Item1.Count == 0)
                {
                    var propertyMap = Reflector.GetPropertyMap<T>();
                    var pimaryKeySet = propertyMap.Values.Where(p => p.IsPrimaryKey).ToDictionary(p => p.ParameterName ?? p.PropertyName, p => p.MappedColumnName);
                    partition = parameters.Partition(p =>
                    {
                        if (pimaryKeySet.TryGetValue(p.Name, out var column))
                        {
                            p.Source = column;
                            p.IsPrimaryKey = true;
                            return true;
                        }
                        return false;
                    });
                }

                request.Operation = SqlBuilder.GetUpdateStatement(typeof(T), partition.Item2, partition.Item1, request.Connection != null ? DialectFactory.GetProvider(request.Connection) : DialectFactory.GetProvider(request.ConnectionName ?? config.DefaultConnectionName));
                request.OperationType = OperationType.Sql;
            }
            else
            {
                request.Operation = OperationUpdate;
                request.OperationType = OperationType.StoredProcedure;
                request.SchemaName = schema;
            }
            var response = await ExecuteAsync<T>(request);
            return response;
        }

        public static async Task<OperationResponse> DeleteAsync<T>(ParamList parameters, string connectionName = null, bool captureException = false, string schema = null, DbConnection connection = null)
            where T : class
        {
            return await DeleteAsync<T>(parameters.GetParameters(typeof(T), OperationDelete), connectionName, captureException, schema, connection);
        }

        public static async Task<OperationResponse> DeleteAsync<T>(Param[] parameters, string connectionName = null, bool captureException = false, string schema = null, DbConnection connection = null)
            where T : class
        {
            var request = new OperationRequest { Parameters = parameters, ReturnType = OperationReturnType.NonQuery, ConnectionName = connectionName, Connection = connection, CaptureException = captureException };
            var config = ConfigurationFactory.Get<T>();
            if (config.GenerateDeleteSql)
            {
                string softDeleteColumn = null;
                var map = MappingFactory.GetEntityMap<T>();
                if (map != null)
                {
                    softDeleteColumn = map.SoftDeleteColumnName;
                }

                if (softDeleteColumn == null)
                {
                    var attr = Reflector.GetAttribute<T, TableAttribute>();
                    if (attr != null)
                    {
                        softDeleteColumn = attr.SoftDeleteColumn;
                    }
                }

                var partition = parameters.Partition(p => p.IsPrimaryKey);
                // if p.IsPrimaryKey is not set then
                // we need to infer it from reflected property 
                if (partition.Item1.Count == 0)
                {
                    var propertyMap = Reflector.GetPropertyMap<T>();
                    var pimaryKeySet = propertyMap.Values.Where(p => p.IsPrimaryKey).ToDictionary(p => p.ParameterName ?? p.PropertyName, p => p.MappedColumnName);
                    partition = parameters.Partition(p =>
                    {
                        if (pimaryKeySet.TryGetValue(p.Name, out var column))
                        {
                            p.Source = column;
                            p.IsPrimaryKey = true;
                            return true;
                        }
                        return false;
                    });
                }

                request.Operation = SqlBuilder.GetDeleteStatement(typeof(T), partition.Item1, request.Connection != null ? DialectFactory.GetProvider(request.Connection) : DialectFactory.GetProvider(request.ConnectionName ?? config.DefaultConnectionName), softDeleteColumn);
                request.OperationType = OperationType.Sql;
            }
            else
            {
                request.Operation = OperationDelete;
                request.OperationType = OperationType.StoredProcedure;
                request.SchemaName = schema;
            }
            var response = await ExecuteAsync<T>(request);
            return response;
        }

        public static async Task<OperationResponse> DestroyAsync<T>(ParamList parameters, string connectionName = null, bool captureException = false, string schema = null, DbConnection connection = null)
            where T : class
        {
            return await DestroyAsync<T>(parameters.GetParameters(typeof(T), OperationDestroy), connectionName, captureException, schema, connection);
        }

        public static async Task<OperationResponse> DestroyAsync<T>(Param[] parameters, string connectionName = null, bool captureException = false, string schema = null, DbConnection connection = null)
            where T : class
        {
            var request = new OperationRequest { Parameters = parameters, ReturnType = OperationReturnType.NonQuery, ConnectionName = connectionName, Connection = connection, CaptureException = captureException };
            var config = ConfigurationFactory.Get<T>();
            if (config.GenerateDeleteSql)
            {
                request.Operation = SqlBuilder.GetDeleteStatement(typeof(T), parameters, request.Connection != null ? DialectFactory.GetProvider(request.Connection) : DialectFactory.GetProvider(request.ConnectionName ?? config.DefaultConnectionName));
                request.OperationType = OperationType.Sql;
            }
            else
            {
                request.Operation = OperationDestroy;
                request.OperationType = OperationType.StoredProcedure;
                request.SchemaName = schema;
            }
            var response = await ExecuteAsync<T>(request);
            return response;
        }

        internal static async Task<OperationResponse> ExecuteAsync(string operationText, IList<Param> parameters, OperationReturnType returnType, OperationType operationType, IList<Type> types = null, string connectionName = null, DbConnection connection = null, DbTransaction transaction = null, bool captureException = false, string schema = null)
        {
            var rootType = types?[0];

            DbConnection dbConnection;
            var closeConnection = false;

            if (transaction != null)
            {
                dbConnection = transaction.Connection;
            }
            else if (connection != null)
            {
                dbConnection = connection;
            }
            else
            {
                dbConnection = DbFactory.CreateConnection(connectionName, rootType);
                closeConnection = true;
            }

            if (returnType == OperationReturnType.Guess)
            {
                if (operationText.IndexOf("insert", StringComparison.OrdinalIgnoreCase) > -1
                         || operationText.IndexOf("update", StringComparison.OrdinalIgnoreCase) > -1
                         || operationText.IndexOf("delete", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    returnType = OperationReturnType.NonQuery;
                }
                else
                {
                    returnType = OperationReturnType.SingleResult;
                }
            }

            Dictionary<DbParameter, Param> outputParameters = null;

            var command = dbConnection.CreateCommand();
            command.CommandText = operationText;
            command.CommandType = operationType == OperationType.StoredProcedure ? CommandType.StoredProcedure : CommandType.Text;
            command.CommandTimeout = 0;
            if (parameters != null)
            {
                for (var i = 0; i < parameters.Count; ++i)
                {
                    var parameter = parameters[i];
                    var dbParam = command.CreateParameter();
                    dbParam.ParameterName = parameter.Name.TrimStart('@', '?', ':');
                    dbParam.Direction = parameter.Direction;
                    dbParam.Value = parameter.Value ?? DBNull.Value;

                    if (parameter.Value != null)
                    {
                        if (parameter.Size > -1)
                        {
                            dbParam.Size = parameter.Size;
                        }

                        dbParam.DbType = parameter.DbType ?? Reflector.ClrToDbType(parameter.Type);
                    }

                    if (dbParam.Direction == ParameterDirection.Output)
                    {
                        if (outputParameters == null)
                        {
                            outputParameters = new Dictionary<DbParameter, Param>();
                        }
                        outputParameters.Add(dbParam, parameter);
                    }

                    command.Parameters.Add(dbParam);
                }
            }

            if (dbConnection.State != ConnectionState.Open)
            {
                await dbConnection.OpenAsync().ConfigureAwait(false);
            }

            var response = new OperationResponse { ReturnType = returnType };
            try
            {
                switch (returnType)
                {
                    case OperationReturnType.NonQuery:
                        response.RecordsAffected = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                        break;
                    case OperationReturnType.MultiResult:
                    case OperationReturnType.SingleResult:
                    case OperationReturnType.SingleRow:
                        var behavior = CommandBehavior.Default;
                        switch (returnType)
                        {
                            case OperationReturnType.SingleResult:
                                behavior = CommandBehavior.SingleResult;
                                break;
                            case OperationReturnType.SingleRow:
                                behavior = CommandBehavior.SingleRow;
                                break;
                            default:
                                closeConnection = false;
                                break;
                        }

                        if (closeConnection)
                        {
                            behavior |= CommandBehavior.CloseConnection;
                        }

                        closeConnection = false;
                        response.Value = await command.ExecuteReaderAsync(behavior).ConfigureAwait(false);
                        break;
                    case OperationReturnType.Scalar:
                        response.Value = await command.ExecuteScalarAsync().ConfigureAwait(false);
                        break;
                }

                // Handle output parameters
                if (outputParameters != null)
                {
                    foreach (var entry in outputParameters)
                    {
                        entry.Value.Value = Convert.IsDBNull(entry.Key.Value) ? null : entry.Key.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                if (captureException)
                {
                    response.Exception = ex;
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                command.Dispose();
                if (dbConnection != null && (closeConnection || response.HasErrors))
                {
                    dbConnection.Close();
                }
            }

            return response;
        }

        public static async Task<OperationResponse> ExecuteAsync<T>(OperationRequest request)
            where T : class
        {
            if (request.Types == null)
            {
                request.Types = new[] { typeof(T) };
            }

            var operationType = request.OperationType;
            if (operationType == OperationType.Guess)
            {
                operationType = request.Operation.Any(char.IsWhiteSpace) ? OperationType.Sql : OperationType.StoredProcedure;
            }

            var operationText = GetOperationText(typeof(T), request.Operation, request.OperationType, request.SchemaName, ConfigurationFactory.Get<T>());

            var response = request.Connection != null
                ? await ExecuteAsync(operationText, request.Parameters, request.ReturnType, operationType, request.Types, connection: request.Connection, transaction: request.Transaction, captureException: request.CaptureException, schema: request.SchemaName)
                : await ExecuteAsync(operationText, request.Parameters, request.ReturnType, operationType, request.Types, request.ConnectionName, transaction: request.Transaction, captureException: request.CaptureException, schema: request.SchemaName);
            return response;
        }

        #endregion
    }
}
