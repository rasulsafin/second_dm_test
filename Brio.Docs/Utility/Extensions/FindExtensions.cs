using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Client.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Brio.Docs.Utility.Extensions
{
    internal static class FindExtensions
    {
        public static async Task<T> FindOrThrowAsync<T>(
            this DbSet<T> set,
            int id,
            CancellationToken cancellationToken = new CancellationToken())
            where T : class
        {
            var result = await set.FindAsync(new object[] { id }, cancellationToken);
            if (result == null)
                throw new NotFoundException<T>(id);

            return result;
        }

        public static async Task<T> FindOrThrowAsync<T>(
            this DbContext dbContext,
            int id,
            CancellationToken cancellationToken = new CancellationToken())
            where T : class
        {
            var result = await dbContext.FindAsync<T>(new object[] { id }, cancellationToken);
            if (result == null)
                throw new NotFoundException<T>(id);

            return result;
        }

        public static TValue FindOrThrow<TKey, TValue>(
            this Dictionary<TKey, TValue> dictionary,
            TKey key)
        {
            if (dictionary.TryGetValue(key, out var result))
                return result;

            throw new NotFoundException<TValue>(nameof(key), key.ToString());
        }

        public static async Task<T> FindOrThrowAsync<T, TProperty>(
            this IQueryable<T> set,
            Expression<Func<T, TProperty>> property,
            TProperty propertyValue,
            CancellationToken cancellationToken = new CancellationToken())
            where T : class
            => await set.FindOrThrowAsync(property, propertyValue, e => e, cancellationToken);

        public static async Task<T> FindWithIgnoreCaseOrThrowAsync<T>(
            this IQueryable<T> set,
            Expression<Func<T, string>> property,
            string propertyValue,
            CancellationToken cancellationToken = new CancellationToken())
            where T : class
        {
            var method = typeof(string).GetMethod(nameof(string.ToLower), Array.Empty<Type>());
            return await set.FindOrThrowAsync(
                property,
                propertyValue,
                e => Expression.Call(e, method!),
                cancellationToken);
        }

        private static async Task<T> FindOrThrowAsync<T, TProperty>(
            this IQueryable<T> set,
            Expression<Func<T, TProperty>> property,
            TProperty propertyValue,
            Func<Expression, Expression> createExpressionForModify,
            CancellationToken cancellationToken = new CancellationToken())
            where T : class
        {
            if (!(property.Body is MemberExpression body) || !(body.Expression is ParameterExpression))
            {
                throw new ArgumentValidationException(
                    $"The lambda expression must return member of {typeof(T).Name}",
                    nameof(property));
            }

            var parameterExpression = Expression.Parameter(typeof(T));
            var propertyName = body.Member.Name;

            var predicate = Expression.Lambda<Func<T, bool>>(
                Expression.Equal(
                    createExpressionForModify(Expression.Property(parameterExpression, propertyName)),
                    createExpressionForModify(Expression.Constant(propertyValue))),
                parameterExpression);
            var result = await set.FirstOrDefaultAsync(predicate, cancellationToken);
            if (result == null)
                throw new NotFoundException<T>(propertyName, propertyValue.ToString());

            return result;
        }
    }
}
