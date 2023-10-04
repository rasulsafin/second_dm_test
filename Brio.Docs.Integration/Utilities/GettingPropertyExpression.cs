using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Brio.Docs.Integration.Utilities
{
    public class GettingPropertyExpression
    {
        private GettingPropertyExpression(Type type, Expression expression)
        {
            this.SourceType = type;
            this.Expression = expression;
        }

        public Type SourceType { get; }

        public Expression Expression { get; }

        public static GettingPropertyExpression Create<T>(Expression<Func<T, object>> property)
            => new GettingPropertyExpression(typeof(T), property);

        public static List<GettingPropertyExpression> CreateList()
            => new List<GettingPropertyExpression>();
    }
}
