using System.Collections;
using System.Collections.Generic;

namespace System.Linq {
    /// <summary>
    /// LINQ to Objects on top of <see cref="IEnumerable{T}"/>. Subset of the
    /// .NET BCL surface picked for embedded use - covers the common
    /// filter / project / aggregate / order / group / convert operators.
    /// Lazy operators (Where, Select, etc.) use iterator state machines;
    /// terminal operators (ToArray, Sum, OrderBy, etc.) are eager.
    /// </summary>
    public static class Enumerable {

        // ===== Filtering =====

        /// <summary>Filters a sequence of values based on a predicate.</summary>
        public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            if (source == null) throw new ArgumentNullException();
            if (predicate == null) throw new ArgumentNullException();
            return WhereIterator(source, predicate);
        }

        private static IEnumerable<TSource> WhereIterator<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            foreach (var item in source)
                if (predicate(item)) yield return item;
        }

        /// <summary>Filters a sequence of values based on a predicate that receives each element's index.</summary>
        public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate) {
            if (source == null) throw new ArgumentNullException();
            if (predicate == null) throw new ArgumentNullException();
            return WhereIndexedIterator(source, predicate);
        }

        private static IEnumerable<TSource> WhereIndexedIterator<TSource>(IEnumerable<TSource> source, Func<TSource, int, bool> predicate) {
            var i = 0;
            foreach (var item in source) {
                if (predicate(item, i)) yield return item;
                i++;
            }
        }

        /// <summary>Filters the elements of a sequence based on a specified type.</summary>
        public static IEnumerable<TResult> OfType<TResult>(this IEnumerable source) {
            if (source == null) throw new ArgumentNullException();
            return OfTypeIterator<TResult>(source);
        }

        private static IEnumerable<TResult> OfTypeIterator<TResult>(IEnumerable source) {
            foreach (var item in source)
                if (item is TResult t) yield return t;
        }

        /// <summary>Casts the elements of a sequence to the specified type.</summary>
        public static IEnumerable<TResult> Cast<TResult>(this IEnumerable source) {
            if (source == null) throw new ArgumentNullException();
            // Fast path: already the right enumerable type.
            if (source is IEnumerable<TResult> typed) return typed;
            return CastIterator<TResult>(source);
        }

        private static IEnumerable<TResult> CastIterator<TResult>(IEnumerable source) {
            foreach (var item in source) yield return (TResult)item;
        }

        // ===== Projection =====

        /// <summary>Projects each element of a sequence into a new form.</summary>
        public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector) {
            if (source == null) throw new ArgumentNullException();
            if (selector == null) throw new ArgumentNullException();
            return SelectIterator(source, selector);
        }

        private static IEnumerable<TResult> SelectIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TResult> selector) {
            foreach (var item in source) yield return selector(item);
        }

        /// <summary>Projects each element of a sequence into a new form by incorporating the element's index.</summary>
        public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, TResult> selector) {
            if (source == null) throw new ArgumentNullException();
            if (selector == null) throw new ArgumentNullException();
            return SelectIndexedIterator(source, selector);
        }

        private static IEnumerable<TResult> SelectIndexedIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, int, TResult> selector) {
            var i = 0;
            foreach (var item in source) {
                yield return selector(item, i);
                i++;
            }
        }

        /// <summary>Projects each element of a sequence to an <see cref="IEnumerable{T}"/> and flattens the results into one sequence.</summary>
        public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector) {
            if (source == null) throw new ArgumentNullException();
            if (selector == null) throw new ArgumentNullException();
            return SelectManyIterator(source, selector);
        }

        private static IEnumerable<TResult> SelectManyIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector) {
            foreach (var item in source)
                foreach (var sub in selector(item))
                    yield return sub;
        }

        // ===== Partitioning =====

        /// <summary>Bypasses a specified number of elements and returns the remaining elements.</summary>
        public static IEnumerable<TSource> Skip<TSource>(this IEnumerable<TSource> source, int count) {
            if (source == null) throw new ArgumentNullException();
            return SkipIterator(source, count);
        }

        private static IEnumerable<TSource> SkipIterator<TSource>(IEnumerable<TSource> source, int count) {
            using (var e = source.GetEnumerator()) {
                while (count > 0 && e.MoveNext()) count--;
                while (e.MoveNext()) yield return e.Current;
            }
        }

        /// <summary>Returns a specified number of contiguous elements from the start of a sequence.</summary>
        public static IEnumerable<TSource> Take<TSource>(this IEnumerable<TSource> source, int count) {
            if (source == null) throw new ArgumentNullException();
            return TakeIterator(source, count);
        }

        private static IEnumerable<TSource> TakeIterator<TSource>(IEnumerable<TSource> source, int count) {
            if (count <= 0) yield break;
            foreach (var item in source) {
                yield return item;
                if (--count == 0) yield break;
            }
        }

        /// <summary>Bypasses elements while a predicate is true and returns the remaining elements.</summary>
        public static IEnumerable<TSource> SkipWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            if (source == null) throw new ArgumentNullException();
            if (predicate == null) throw new ArgumentNullException();
            return SkipWhileIterator(source, predicate);
        }

        private static IEnumerable<TSource> SkipWhileIterator<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            var yielding = false;
            foreach (var item in source) {
                if (!yielding && !predicate(item)) yielding = true;
                if (yielding) yield return item;
            }
        }

        /// <summary>Returns elements from the start of a sequence while a predicate is true.</summary>
        public static IEnumerable<TSource> TakeWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            if (source == null) throw new ArgumentNullException();
            if (predicate == null) throw new ArgumentNullException();
            return TakeWhileIterator(source, predicate);
        }

        private static IEnumerable<TSource> TakeWhileIterator<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            foreach (var item in source) {
                if (!predicate(item)) yield break;
                yield return item;
            }
        }

        // ===== Set / dedup =====

        /// <summary>Returns distinct elements from a sequence using the default equality comparer.</summary>
        public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw new ArgumentNullException();
            return DistinctIterator(source, EqualityComparer<TSource>.Default);
        }

        /// <summary>Returns distinct elements from a sequence using a specified equality comparer.</summary>
        public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer) {
            if (source == null) throw new ArgumentNullException();
            return DistinctIterator(source, comparer ?? EqualityComparer<TSource>.Default);
        }

        private static IEnumerable<TSource> DistinctIterator<TSource>(IEnumerable<TSource> source, IEqualityComparer<TSource> comparer) {
            // Dictionary keyed by element acts as a hash set.
            var seen = new Dictionary<TSource, bool>(comparer);
            foreach (var item in source) {
                if (!seen.ContainsKey(item)) {
                    seen[item] = true;
                    yield return item;
                }
            }
        }

        // ===== Concatenation / reversal =====

        /// <summary>Concatenates two sequences.</summary>
        public static IEnumerable<TSource> Concat<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second) {
            if (first == null) throw new ArgumentNullException();
            if (second == null) throw new ArgumentNullException();
            return ConcatIterator(first, second);
        }

        private static IEnumerable<TSource> ConcatIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second) {
            foreach (var item in first) yield return item;
            foreach (var item in second) yield return item;
        }

        /// <summary>Inverts the order of the elements in a sequence.</summary>
        public static IEnumerable<TSource> Reverse<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw new ArgumentNullException();
            return ReverseIterator(source);
        }

        private static IEnumerable<TSource> ReverseIterator<TSource>(IEnumerable<TSource> source) {
            // Reverse requires full buffering.
            var buf = ToArray(source);
            for (var i = buf.Length - 1; i >= 0; i--) yield return buf[i];
        }

        // ===== Comparison =====

        /// <summary>Determines whether two sequences are equal using the default equality comparer.</summary>
        public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second) =>
            SequenceEqual(first, second, EqualityComparer<TSource>.Default);

        /// <summary>Determines whether two sequences are equal using a specified equality comparer.</summary>
        public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer) {
            if (first == null) throw new ArgumentNullException();
            if (second == null) throw new ArgumentNullException();
            if (comparer == null) comparer = EqualityComparer<TSource>.Default;
            using (var e1 = first.GetEnumerator())
            using (var e2 = second.GetEnumerator()) {
                while (e1.MoveNext()) {
                    if (!e2.MoveNext()) return false;
                    if (!comparer.Equals(e1.Current, e2.Current)) return false;
                }
                return !e2.MoveNext();
            }
        }

        // ===== Quantifiers =====

        /// <summary>Determines whether a sequence contains any elements.</summary>
        public static bool Any<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw new ArgumentNullException();
            using (var e = source.GetEnumerator()) return e.MoveNext();
        }

        /// <summary>Determines whether any element of a sequence satisfies a predicate.</summary>
        public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            if (source == null) throw new ArgumentNullException();
            if (predicate == null) throw new ArgumentNullException();
            foreach (var item in source) if (predicate(item)) return true;
            return false;
        }

        /// <summary>Determines whether all elements of a sequence satisfy a predicate.</summary>
        public static bool All<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            if (source == null) throw new ArgumentNullException();
            if (predicate == null) throw new ArgumentNullException();
            foreach (var item in source) if (!predicate(item)) return false;
            return true;
        }

        /// <summary>Determines whether a sequence contains a specified element using the default equality comparer.</summary>
        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value) =>
            Contains(source, value, EqualityComparer<TSource>.Default);

        /// <summary>Determines whether a sequence contains a specified element using a specified equality comparer.</summary>
        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource> comparer) {
            if (source == null) throw new ArgumentNullException();
            if (comparer == null) comparer = EqualityComparer<TSource>.Default;
            foreach (var item in source) if (comparer.Equals(item, value)) return true;
            return false;
        }

        // ===== Element accessors =====

        /// <summary>Returns the first element of a sequence, throwing if the sequence is empty.</summary>
        public static TSource First<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw new ArgumentNullException();
            using (var e = source.GetEnumerator()) {
                if (!e.MoveNext()) throw new InvalidOperationException();
                return e.Current;
            }
        }

        /// <summary>Returns the first element that satisfies a predicate, throwing if none match.</summary>
        public static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            if (source == null) throw new ArgumentNullException();
            if (predicate == null) throw new ArgumentNullException();
            foreach (var item in source) if (predicate(item)) return item;
            throw new InvalidOperationException();
        }

        /// <summary>Returns the first element of a sequence, or a default value if the sequence is empty.</summary>
        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw new ArgumentNullException();
            using (var e = source.GetEnumerator()) return e.MoveNext() ? e.Current : default(TSource);
        }

        /// <summary>Returns the first element that satisfies a predicate, or a default value if none match.</summary>
        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            if (source == null) throw new ArgumentNullException();
            if (predicate == null) throw new ArgumentNullException();
            foreach (var item in source) if (predicate(item)) return item;
            return default(TSource);
        }

        /// <summary>Returns the last element of a sequence, throwing if the sequence is empty.</summary>
        public static TSource Last<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw new ArgumentNullException();
            using (var e = source.GetEnumerator()) {
                if (!e.MoveNext()) throw new InvalidOperationException();
                var last = e.Current;
                while (e.MoveNext()) last = e.Current;
                return last;
            }
        }

        /// <summary>Returns the last element that satisfies a predicate, throwing if none match.</summary>
        public static TSource Last<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            if (source == null) throw new ArgumentNullException();
            if (predicate == null) throw new ArgumentNullException();
            var found = false;
            var last = default(TSource);
            foreach (var item in source) if (predicate(item)) { last = item; found = true; }
            if (!found) throw new InvalidOperationException();
            return last;
        }

        /// <summary>Returns the last element of a sequence, or a default value if the sequence is empty.</summary>
        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw new ArgumentNullException();
            using (var e = source.GetEnumerator()) {
                if (!e.MoveNext()) return default(TSource);
                var last = e.Current;
                while (e.MoveNext()) last = e.Current;
                return last;
            }
        }

        /// <summary>Returns the last element that satisfies a predicate, or a default value if none match.</summary>
        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            if (source == null) throw new ArgumentNullException();
            if (predicate == null) throw new ArgumentNullException();
            var last = default(TSource);
            foreach (var item in source) if (predicate(item)) last = item;
            return last;
        }

        /// <summary>Returns the only element of a sequence, throwing if it is empty or has more than one element.</summary>
        public static TSource Single<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw new ArgumentNullException();
            using (var e = source.GetEnumerator()) {
                if (!e.MoveNext()) throw new InvalidOperationException();
                var first = e.Current;
                if (e.MoveNext()) throw new InvalidOperationException();
                return first;
            }
        }

        /// <summary>Returns the only element that satisfies a predicate, throwing if none or more than one match.</summary>
        public static TSource Single<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            if (source == null) throw new ArgumentNullException();
            if (predicate == null) throw new ArgumentNullException();
            var found = false;
            var match = default(TSource);
            foreach (var item in source) {
                if (predicate(item)) {
                    if (found) throw new InvalidOperationException();
                    match = item;
                    found = true;
                }
            }
            if (!found) throw new InvalidOperationException();
            return match;
        }

        /// <summary>Returns the only element of a sequence, a default value if empty, or throws if more than one element.</summary>
        public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw new ArgumentNullException();
            using (var e = source.GetEnumerator()) {
                if (!e.MoveNext()) return default(TSource);
                var first = e.Current;
                if (e.MoveNext()) throw new InvalidOperationException();
                return first;
            }
        }

        /// <summary>Returns the only element that satisfies a predicate, a default value if none match, or throws if more than one matches.</summary>
        public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            if (source == null) throw new ArgumentNullException();
            if (predicate == null) throw new ArgumentNullException();
            var found = false;
            var match = default(TSource);
            foreach (var item in source) {
                if (predicate(item)) {
                    if (found) throw new InvalidOperationException();
                    match = item;
                    found = true;
                }
            }
            return match;
        }

        /// <summary>Returns the element at a specified index in a sequence.</summary>
        public static TSource ElementAt<TSource>(this IEnumerable<TSource> source, int index) {
            if (source == null) throw new ArgumentNullException();
            if (index < 0) throw new ArgumentOutOfRangeException();
            if (source is IList<TSource> list) return list[index];
            foreach (var item in source) {
                if (index == 0) return item;
                index--;
            }
            throw new ArgumentOutOfRangeException();
        }

        /// <summary>Returns the element at a specified index, or a default value if the index is out of range.</summary>
        public static TSource ElementAtOrDefault<TSource>(this IEnumerable<TSource> source, int index) {
            if (source == null) throw new ArgumentNullException();
            if (index < 0) return default(TSource);
            if (source is IList<TSource> list) return index < list.Count ? list[index] : default(TSource);
            foreach (var item in source) {
                if (index == 0) return item;
                index--;
            }
            return default(TSource);
        }

        // ===== Aggregation =====

        /// <summary>Returns the number of elements in a sequence.</summary>
        public static int Count<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw new ArgumentNullException();
            // Fast path for ICollection<T>.
            if (source is ICollection<TSource> col) return col.Count;
            var n = 0;
            using (var e = source.GetEnumerator()) while (e.MoveNext()) n++;
            return n;
        }

        /// <summary>Returns the number of elements in a sequence that satisfy a predicate.</summary>
        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            if (source == null) throw new ArgumentNullException();
            if (predicate == null) throw new ArgumentNullException();
            var n = 0;
            foreach (var item in source) if (predicate(item)) n++;
            return n;
        }

        /// <summary>Returns the number of elements in a sequence as a 64-bit integer.</summary>
        public static long LongCount<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw new ArgumentNullException();
            var n = 0L;
            using (var e = source.GetEnumerator()) while (e.MoveNext()) n++;
            return n;
        }

        /// <summary>Computes the sum of a sequence of 32-bit integers.</summary>
        // Sum overloads - typed for the numeric primitives most code uses.
        public static int Sum(this IEnumerable<int> source) {
            if (source == null) throw new ArgumentNullException();
            var s = 0;
            foreach (var x in source) s += x;
            return s;
        }

        /// <summary>Computes the sum of the 32-bit integer values selected from each element.</summary>
        public static int Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector) {
            if (source == null) throw new ArgumentNullException();
            if (selector == null) throw new ArgumentNullException();
            var s = 0;
            foreach (var item in source) s += selector(item);
            return s;
        }

        /// <summary>Computes the sum of a sequence of 64-bit integers.</summary>
        public static long Sum(this IEnumerable<long> source) {
            if (source == null) throw new ArgumentNullException();
            var s = 0L;
            foreach (var x in source) s += x;
            return s;
        }

        /// <summary>Computes the sum of the 64-bit integer values selected from each element.</summary>
        public static long Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector) {
            if (source == null) throw new ArgumentNullException();
            if (selector == null) throw new ArgumentNullException();
            var s = 0L;
            foreach (var item in source) s += selector(item);
            return s;
        }

        /// <summary>Computes the sum of a sequence of double-precision values.</summary>
        public static double Sum(this IEnumerable<double> source) {
            if (source == null) throw new ArgumentNullException();
            var s = 0d;
            foreach (var x in source) s += x;
            return s;
        }

        /// <summary>Computes the sum of the double-precision values selected from each element.</summary>
        public static double Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector) {
            if (source == null) throw new ArgumentNullException();
            if (selector == null) throw new ArgumentNullException();
            var s = 0d;
            foreach (var item in source) s += selector(item);
            return s;
        }

        /// <summary>Computes the sum of a sequence of single-precision values.</summary>
        public static float Sum(this IEnumerable<float> source) {
            if (source == null) throw new ArgumentNullException();
            var s = 0f;
            foreach (var x in source) s += x;
            return s;
        }

        /// <summary>Returns the minimum value in a sequence using the default comparer.</summary>
        // Min/Max - generic via Comparer<T>.Default.
        public static TSource Min<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw new ArgumentNullException();
            var cmp = Comparer<TSource>.Default;
            using (var e = source.GetEnumerator()) {
                if (!e.MoveNext()) throw new InvalidOperationException();
                var min = e.Current;
                while (e.MoveNext()) {
                    var cur = e.Current;
                    if (cmp.Compare(cur, min) < 0) min = cur;
                }
                return min;
            }
        }

        /// <summary>Returns the minimum value selected from each element of a sequence.</summary>
        public static TResult Min<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector) {
            if (source == null) throw new ArgumentNullException();
            if (selector == null) throw new ArgumentNullException();
            var cmp = Comparer<TResult>.Default;
            using (var e = source.GetEnumerator()) {
                if (!e.MoveNext()) throw new InvalidOperationException();
                var min = selector(e.Current);
                while (e.MoveNext()) {
                    var cur = selector(e.Current);
                    if (cmp.Compare(cur, min) < 0) min = cur;
                }
                return min;
            }
        }

        /// <summary>Returns the maximum value in a sequence using the default comparer.</summary>
        public static TSource Max<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw new ArgumentNullException();
            var cmp = Comparer<TSource>.Default;
            using (var e = source.GetEnumerator()) {
                if (!e.MoveNext()) throw new InvalidOperationException();
                var max = e.Current;
                while (e.MoveNext()) {
                    var cur = e.Current;
                    if (cmp.Compare(cur, max) > 0) max = cur;
                }
                return max;
            }
        }

        /// <summary>Returns the maximum value selected from each element of a sequence.</summary>
        public static TResult Max<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector) {
            if (source == null) throw new ArgumentNullException();
            if (selector == null) throw new ArgumentNullException();
            var cmp = Comparer<TResult>.Default;
            using (var e = source.GetEnumerator()) {
                if (!e.MoveNext()) throw new InvalidOperationException();
                var max = selector(e.Current);
                while (e.MoveNext()) {
                    var cur = selector(e.Current);
                    if (cmp.Compare(cur, max) > 0) max = cur;
                }
                return max;
            }
        }

        /// <summary>Computes the average of a sequence of 32-bit integers.</summary>
        public static double Average(this IEnumerable<int> source) {
            if (source == null) throw new ArgumentNullException();
            var sum = 0L;
            var count = 0;
            foreach (var x in source) { sum += x; count++; }
            if (count == 0) throw new InvalidOperationException();
            return (double)sum / count;
        }

        /// <summary>Computes the average of a sequence of 64-bit integers.</summary>
        public static double Average(this IEnumerable<long> source) {
            if (source == null) throw new ArgumentNullException();
            var sum = 0d; // double accumulator to avoid overflow risk
            var count = 0;
            foreach (var x in source) { sum += x; count++; }
            if (count == 0) throw new InvalidOperationException();
            return sum / count;
        }

        /// <summary>Computes the average of a sequence of double-precision values.</summary>
        public static double Average(this IEnumerable<double> source) {
            if (source == null) throw new ArgumentNullException();
            var sum = 0d;
            var count = 0;
            foreach (var x in source) { sum += x; count++; }
            if (count == 0) throw new InvalidOperationException();
            return sum / count;
        }

        /// <summary>Applies an accumulator function over a sequence starting from a seed value.</summary>
        public static TAccumulate Aggregate<TSource, TAccumulate>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func) {
            if (source == null) throw new ArgumentNullException();
            if (func == null) throw new ArgumentNullException();
            var acc = seed;
            foreach (var item in source) acc = func(acc, item);
            return acc;
        }

        /// <summary>Applies an accumulator function over a sequence using the first element as the seed.</summary>
        public static TSource Aggregate<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func) {
            if (source == null) throw new ArgumentNullException();
            if (func == null) throw new ArgumentNullException();
            using (var e = source.GetEnumerator()) {
                if (!e.MoveNext()) throw new InvalidOperationException();
                var acc = e.Current;
                while (e.MoveNext()) acc = func(acc, e.Current);
                return acc;
            }
        }

        // ===== Ordering =====

        /// <summary>Sorts the elements of a sequence in ascending order by a key.</summary>
        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) =>
            OrderBy(source, keySelector, null);

        /// <summary>Sorts the elements of a sequence in ascending order by a key using a specified comparer.</summary>
        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer) {
            if (source == null) throw new ArgumentNullException();
            if (keySelector == null) throw new ArgumentNullException();
            return new OrderedEnumerable<TSource>(
                source,
                new OrderedEnumerable<TSource>.SortKey<TKey>(keySelector, comparer ?? Comparer<TKey>.Default, false));
        }

        /// <summary>Sorts the elements of a sequence in descending order by a key.</summary>
        public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) =>
            OrderByDescending(source, keySelector, null);

        /// <summary>Sorts the elements of a sequence in descending order by a key using a specified comparer.</summary>
        public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer) {
            if (source == null) throw new ArgumentNullException();
            if (keySelector == null) throw new ArgumentNullException();
            return new OrderedEnumerable<TSource>(
                source,
                new OrderedEnumerable<TSource>.SortKey<TKey>(keySelector, comparer ?? Comparer<TKey>.Default, true));
        }

        /// <summary>Performs a subsequent ascending sort on an already ordered sequence by a key.</summary>
        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector) {
            if (source == null) throw new ArgumentNullException();
            return source.CreateOrderedEnumerable(keySelector, null, false);
        }

        /// <summary>Performs a subsequent ascending sort on an already ordered sequence by a key using a specified comparer.</summary>
        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer) {
            if (source == null) throw new ArgumentNullException();
            return source.CreateOrderedEnumerable(keySelector, comparer, false);
        }

        /// <summary>Performs a subsequent descending sort on an already ordered sequence by a key.</summary>
        public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector) {
            if (source == null) throw new ArgumentNullException();
            return source.CreateOrderedEnumerable(keySelector, null, true);
        }

        /// <summary>Performs a subsequent descending sort on an already ordered sequence by a key using a specified comparer.</summary>
        public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer) {
            if (source == null) throw new ArgumentNullException();
            return source.CreateOrderedEnumerable(keySelector, comparer, true);
        }

        // ===== Grouping =====

        /// <summary>Groups the elements of a sequence by a key.</summary>
        public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) =>
            GroupBy(source, keySelector, EqualityComparer<TKey>.Default);

        /// <summary>Groups the elements of a sequence by a key using a specified equality comparer.</summary>
        public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) {
            if (source == null) throw new ArgumentNullException();
            if (keySelector == null) throw new ArgumentNullException();
            if (comparer == null) comparer = EqualityComparer<TKey>.Default;
            return GroupByIterator(source, keySelector, comparer);
        }

        private static IEnumerable<IGrouping<TKey, TSource>> GroupByIterator<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) {
            var groupsByKey = new Dictionary<TKey, Grouping<TKey, TSource>>(comparer);
            var groupOrder = new List<Grouping<TKey, TSource>>();
            foreach (var item in source) {
                var key = keySelector(item);
                if (!groupsByKey.TryGetValue(key, out var g)) {
                    g = new Grouping<TKey, TSource>(key);
                    groupsByKey[key] = g;
                    groupOrder.Add(g);
                }
                g.Add(item);
            }
            foreach (var g in groupOrder) {
                yield return g;
            }
        }

        /// <summary>Groups the elements of a sequence by a key and projects each element with an element selector.</summary>
        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
                this IEnumerable<TSource> source,
                Func<TSource, TKey> keySelector,
                Func<TSource, TElement> elementSelector) =>
            GroupBy(source, keySelector, elementSelector, EqualityComparer<TKey>.Default);

        /// <summary>Groups the elements of a sequence by a key, projecting each element with an element selector and using a specified equality comparer.</summary>
        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
                this IEnumerable<TSource> source,
                Func<TSource, TKey> keySelector,
                Func<TSource, TElement> elementSelector,
                IEqualityComparer<TKey> comparer) {
            if (source == null) throw new ArgumentNullException();
            if (keySelector == null) throw new ArgumentNullException();
            if (elementSelector == null) throw new ArgumentNullException();
            if (comparer == null) comparer = EqualityComparer<TKey>.Default;
            return GroupByIterator(source, keySelector, elementSelector, comparer);
        }

        private static IEnumerable<IGrouping<TKey, TElement>> GroupByIterator<TSource, TKey, TElement>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer) {
            var groupsByKey = new Dictionary<TKey, Grouping<TKey, TElement>>(comparer);
            var groupOrder = new List<Grouping<TKey, TElement>>();
            foreach (var item in source) {
                var key = keySelector(item);
                if (!groupsByKey.TryGetValue(key, out var g)) {
                    g = new Grouping<TKey, TElement>(key);
                    groupsByKey[key] = g;
                    groupOrder.Add(g);
                }
                g.Add(elementSelector(item));
            }
            foreach (var g in groupOrder) yield return g;
        }

        // ===== Conversion =====

        /// <summary>Creates an array from a sequence.</summary>
        public static TSource[] ToArray<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw new ArgumentNullException();
            // Fast path: already an ICollection - one allocation, no resize loop.
            if (source is ICollection<TSource> col) {
                var arr = new TSource[col.Count];
                col.CopyTo(arr, 0);
                return arr;
            }
            var list = new List<TSource>();
            foreach (var item in source) list.Add(item);
            return list.ToArray();
        }

        /// <summary>Creates a <see cref="List{T}"/> from a sequence.</summary>
        public static List<TSource> ToList<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw new ArgumentNullException();
            var list = new List<TSource>();
            foreach (var item in source) list.Add(item);
            return list;
        }

        /// <summary>Creates a <see cref="Dictionary{TKey,TValue}"/> from a sequence using a key selector.</summary>
        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) =>
            ToDictionary(source, keySelector, EqualityComparer<TKey>.Default);

        /// <summary>Creates a <see cref="Dictionary{TKey,TValue}"/> from a sequence using a key selector and a specified equality comparer.</summary>
        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) {
            if (source == null) throw new ArgumentNullException();
            if (keySelector == null) throw new ArgumentNullException();
            var d = new Dictionary<TKey, TSource>(comparer ?? EqualityComparer<TKey>.Default);
            foreach (var item in source) d.Add(keySelector(item), item);
            return d;
        }

        /// <summary>Creates a <see cref="Dictionary{TKey,TValue}"/> from a sequence using key and element selectors.</summary>
        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(
                this IEnumerable<TSource> source,
                Func<TSource, TKey> keySelector,
                Func<TSource, TElement> elementSelector) =>
            ToDictionary(source, keySelector, elementSelector, EqualityComparer<TKey>.Default);

        /// <summary>Creates a <see cref="Dictionary{TKey,TValue}"/> from a sequence using key and element selectors and a specified equality comparer.</summary>
        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(
                this IEnumerable<TSource> source,
                Func<TSource, TKey> keySelector,
                Func<TSource, TElement> elementSelector,
                IEqualityComparer<TKey> comparer) {
            if (source == null) throw new ArgumentNullException();
            if (keySelector == null) throw new ArgumentNullException();
            if (elementSelector == null) throw new ArgumentNullException();
            var d = new Dictionary<TKey, TElement>(comparer ?? EqualityComparer<TKey>.Default);
            foreach (var item in source) d.Add(keySelector(item), elementSelector(item));
            return d;
        }

        // ===== Generation =====

        /// <summary>Generates a sequence of consecutive integers starting at a specified value.</summary>
        public static IEnumerable<int> Range(int start, int count) {
            if (count < 0) throw new ArgumentOutOfRangeException();
            // Guard against int overflow (start + count - 1 > int.MaxValue).
            if (count > 0 && (long)start + count - 1 > int.MaxValue) throw new ArgumentOutOfRangeException();
            return RangeIterator(start, count);
        }

        private static IEnumerable<int> RangeIterator(int start, int count) {
            for (var i = 0; i < count; i++) yield return start + i;
        }

        /// <summary>Generates a sequence that contains one repeated value.</summary>
        public static IEnumerable<TResult> Repeat<TResult>(TResult element, int count) {
            if (count < 0) throw new ArgumentOutOfRangeException();
            return RepeatIterator(element, count);
        }

        private static IEnumerable<TResult> RepeatIterator<TResult>(TResult element, int count) {
            for (var i = 0; i < count; i++) yield return element;
        }

        /// <summary>Returns an empty sequence of the specified type.</summary>
        public static IEnumerable<TResult> Empty<TResult>() => EmptyArray<TResult>.Instance;

        private static class EmptyArray<T> {
            public static readonly T[] Instance = new T[0];
        }
    }
}
