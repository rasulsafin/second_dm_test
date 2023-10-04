using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Brio.Docs.Integration.Utilities;

namespace Brio.Docs.Integration.Extensions
{
    public static class GettingPropertyExpressionExtensions
    {
        public static List<GettingPropertyExpression> AddProperty<T>(
            this List<GettingPropertyExpression> source,
            Expression<Func<T, object>> expression)
        {
            source.Add(GettingPropertyExpression.Create(expression));
            return source;
        }
    }
}
