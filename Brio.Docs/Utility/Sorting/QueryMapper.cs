using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Brio.Docs.Utility.Sorting
{
    /// <summary>
    /// Stores expressions needed for query building during request parsing.
    /// </summary>
    /// <typeparam name="T">Database model type.</typeparam>
    public class QueryMapper<T>
    {
        private readonly Dictionary<string, QueryMap> mappings = new Dictionary<string, QueryMap>();

        public QueryMapper(QueryMapperConfiguration config = null)
        {
            Config = config ?? new QueryMapperConfiguration();
        }

        public QueryMapperConfiguration Config { get; }

        /// <summary>
        /// Register new property mapping.
        /// </summary>
        /// <param name="name">Request field name.</param>
        /// <param name="propertyExpression">Mapping expression.</param>
        /// <param name="overwrite">Overwrite existing mapping, if provided field name is already registered.
        /// Otherwise, throws an exception.</param>
        public void AddMap(string name, Expression<Func<T, object>> propertyExpression, bool overwrite = false)
        {
            AddMapImpl(name, propertyExpression, overwrite);
        }

        /// <summary>
        /// Check, if mapping for provided key is registered.
        /// </summary>
        /// <param name="name">Key.</param>
        /// <returns>True, if mapping does exist.</returns>
        public bool HasMap(string name) => TryFindMap(name) != null;

        /// <summary>
        /// Get mapping expression for provided key.
        /// </summary>
        /// <param name="name">Key.</param>
        /// <returns>Mapping expression.</returns>
        public Expression<Func<T, object>> GetExpression(string name)
        {
            var key = FormatKey(name);
            var map = TryFindMap(key);
            if (map == null && !Config.IgnoreNotMappedFields)
                throw new InvalidOperationException($"Mapping for property {name} is not found");

            return map?.Expression;
        }

        private string FormatKey(string key) => Config.IsCaseSensitive ? key : key.ToUpperInvariant();

        private QueryMap TryFindMap(string key) => mappings.TryGetValue(FormatKey(key), out var map) ? map : null;

        private void AddMapImpl(string name, Expression<Func<T, object>> propertyExpression, bool overwrite)
        {
            var key = FormatKey(name);
            if (mappings.ContainsKey(key) && !overwrite)
                throw new ArgumentException($"Mapping for key {key} is already registered", nameof(name));

            mappings[key] = new QueryMap(name, propertyExpression);
        }

        private class QueryMap
        {
            public QueryMap(string name, Expression<Func<T, object>> expr)
            {
                Name = name;
                Expression = expr;
            }

            public string Name { get; }

            public Expression<Func<T, object>> Expression { get; }
        }
    }
}
