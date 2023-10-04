using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Brio.Docs.Tests.Utility
{
    public static class AssertExtensions
    {
        public static void AreEquivalent<T>(this CollectionAssert customAssert, IEnumerable<T> expected, IEnumerable<T> actual, IEqualityComparer<T> comparer)
        {
            // Check whether one is null while the other is not.
            if ((expected == null) != (actual == null))
            {
                throw new AssertFailedException("One of collections is null while other is not");
            }

            // If the references are the same or both collections are null, they
            // are equivalent.
            if (ReferenceEquals(expected, actual) || expected == null)
            {
                return;
            }

            // Check whether the element counts are different.
            var expectedCount = expected.Count();
            var actualCount = actual.Count();
            if (expectedCount != actualCount)
            {
                throw new AssertFailedException($"Collections' lenghts does not match: expected {expectedCount}, got {actualCount}");
            }

            // If both collections are empty, they are equivalent.
            if (expected.Count() == 0)
            {
                return;
            }

            // Search for a mismatched element.
            if (FindMismatchedElement(expected, actual, comparer, out T mismatchedElement))
            {
                throw new AssertFailedException($"Element {mismatchedElement?.ToString() ?? "null"} not found in actual data");
            }

            // All the elements and counts matched.
        }

        /// <summary>
        /// Finds a mismatched element between the two collections. A mismatched
        /// element is one that appears a different number of times in the
        /// expected collection than it does in the actual collection. The
        /// collections are assumed to be different non-null references with the
        /// same number of elements. The caller is responsible for this level of
        /// verification. If there is no mismatched element, the function returns
        /// false and the out parameters should not be used.
        /// </summary>
        /// <param name="expected">The first collection to compare.</param>
        /// <param name="actual">The second collection to compare.</param>
        /// <param name="mismatchedElement">
        /// The mismatched element (may be null) or null if there is no mismatched element.
        /// </param>
        /// <returns>
        /// true if a mismatched element was found; false otherwise.
        /// </returns>
        private static bool FindMismatchedElement<T>(
            IEnumerable<T> expected,
            IEnumerable<T> actual,
            IEqualityComparer<T> comparer,
            out T mismatchedElement)
        {
            var actualList = actual.ToList();

            foreach (var item in expected)
            {
                var index = actualList.IndexOf(item, comparer);

                if (index >= 0)
                {
                    actualList.RemoveAt(index);
                }
                else
                {
                    mismatchedElement = item;
                    return true;
                }
            }

            mismatchedElement = default(T);
            return false;
        }

        private static int IndexOf<T>(this IEnumerable<T> source, T element, IEqualityComparer<T> comparer)
        {
            int index = 0;

            foreach (var item in source)
            {
                if (comparer.Equals(item, element))
                    return index;

                index++;
            }

            return -1;
        }
    }
}
