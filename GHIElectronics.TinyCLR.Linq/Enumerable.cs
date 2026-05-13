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

        public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            if (source == null) throw new ArgumentNullException();
            if (predicate == null) throw new ArgumentNullException();
            return WhereIterator(source, predicate);
        }

        private static IEnumerable<TSource> WhereIterator<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            foreach (var item in source)
                if (predicate(item)) yield return item;
        }

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

        public static IEnumerable<TResult> OfType<TResult>(this IEnumerable source) {
            if (source == null) throw new ArgumentNullException();
            return OfTypeIterator<TResult>(source);
        }

        private static IEnumerable<TResult> OfTypeIterator<TResult>(IEnumerable source) {
            foreach (var item in source)
                if (item is TResult t) yield return t;
        }

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

        public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector) {
            if (source == null) throw new ArgumentNullException();
            if (selector == null) throw new ArgumentNullException();
            return SelectIterator(source, selector);
        }

        private static IEnumerable<TResult> SelectIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TResult> selector) {
            foreach (var item in source) yield return selector(item);
        }

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

        public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw new ArgumentNullException();
            return DistinctIterator(source, EqualityComparer<TSource>.Default);
        }

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

        public static IEnumerable<TSource> Concat<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second) {
            if (first == null) throw new ArgumentNullException();
            if (second == null) throw new ArgumentNullException();
            return ConcatIterator(first, second);
        }

        private static IEnumerable<TSource> ConcatIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second) {
            foreach (var item in first) yield return item;
            foreach (var item in second) yield return item;
        }

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

        public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second) =>
            SequenceEqual(first, second, EqualityComparer<TSource>.Default);

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

        public static bool Any<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw new ArgumentNullException();
            using (var e = source.GetEnumerator()) return e.MoveNext();
        }

        public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            if (source == null) throw new ArgumentNullException();
            if (predicate == null) throw new ArgumentNullException();
            foreach (var item in source) if (predicate(item)) return true;
            return false;
        }

        public static bool All<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            if (source == null) throw new ArgumentNullException();
            if (predicate == null) throw new ArgumentNullException();
            foreach (var item in source) if (!predicate(item)) return false;
            return true;
        }

        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value) =>
            Contains(source, value, EqualityComparer<TSource>.Default);

        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource> comparer) {
            if (source == null) throw new ArgumentNullException();
            if (comparer == null) comparer = EqualityComparer<TSource>.Default;
            foreach (var item in source) if (comparer.Equals(item, value)) return true;
            return false;
        }

        // ===== Element accessors =====

        public static TSource First<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw new ArgumentNullException();
            using (var e = source.GetEnumerator()) {
                if (!e.MoveNext()) throw new InvalidOperationException();
                return e.Current;
            }
        }

        public static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            if (source == null) throw new ArgumentNullException();
            if (predicate == null) throw new ArgumentNullException();
            foreach (var item in source) if (predicate(item)) return item;
            throw new InvalidOperationException();
        }

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw new ArgumentNullException();
            using (var e = source.GetEnumerator()) return e.MoveNext() ? e.Current : default(TSource);
        }

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            if (source == null) throw new ArgumentNullException();
            if (predicate == null) throw new ArgumentNullException();
            foreach (var item in source) if (predicate(item)) return item;
            return default(TSource);
        }

        public static TSource Last<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw new ArgumentNullException();
            using (var e = source.GetEnumerator()) {
                if (!e.MoveNext()) throw new InvalidOperationException();
                var last = e.Current;
                while (e.MoveNext()) last = e.Current;
                return last;
            }
        }

        public static TSource Last<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            if (source == null) throw new ArgumentNullException();
            if (predicate == null) throw new ArgumentNullException();
            var found = false;
            var last = default(TSource);
            foreach (var item in source) if (predicate(item)) { last = item; found = true; }
            if (!found) throw new InvalidOperationException();
            return last;
        }

        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw new ArgumentNullException();
            using (var e = source.GetEnumerator()) {
                if (!e.MoveNext()) return default(TSource);
                var last = e.Current;
                while (e.MoveNext()) last = e.Current;
                return last;
            }
        }

        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            if (source == null) throw new ArgumentNullException();
            if (predicate == null) throw new ArgumentNullException();
            var last = default(TSource);
            foreach (var item in source) if (predicate(item)) last = item;
            return last;
        }

        public static TSource Single<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw new ArgumentNullException();
            using (var e = source.GetEnumerator()) {
                if (!e.MoveNext()) throw new InvalidOperationException();
                var first = e.Current;
                if (e.MoveNext()) throw new InvalidOperationException();
                return first;
            }
        }

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

        public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw new ArgumentNullException();
            using (var e = source.GetEnumerator()) {
                if (!e.MoveNext()) return default(TSource);
                var first = e.Current;
                if (e.MoveNext()) throw new InvalidOperationException();
                return first;
            }
        }

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

        public static int Count<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw new ArgumentNullException();
            // Fast path for ICollection<T>.
            if (source is ICollection<TSource> col) return col.Count;
            var n = 0;
            using (var e = source.GetEnumerator()) while (e.MoveNext()) n++;
            return n;
        }

        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
            if (source == null) throw new ArgumentNullException();
            if (predicate == null) throw new ArgumentNullException();
            var n = 0;
            foreach (var item in source) if (predicate(item)) n++;
            return n;
        }

        public static long LongCount<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw new ArgumentNullException();
            var n = 0L;
            using (var e = source.GetEnumerator()) while (e.MoveNext()) n++;
            return n;
        }

        // Sum overloads - typed for the numeric primitives most code uses.
        public static int Sum(this IEnumerable<int> source) {
            if (source == null) throw new ArgumentNullException();
            var s = 0;
            foreach (var x in source) s += x;
            return s;
        }

        public static int Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector) {
            if (source == null) throw new ArgumentNullException();
            if (selector == null) throw new ArgumentNullException();
            var s = 0;
            foreach (var item in source) s += selector(item);
            return s;
        }

        public static long Sum(this IEnumerable<long> source) {
            if (source == null) throw new ArgumentNullException();
            var s = 0L;
            foreach (var x in source) s += x;
            return s;
        }

        public static long Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector) {
            if (source == null) throw new ArgumentNullException();
            if (selector == null) throw new ArgumentNullException();
            var s = 0L;
            foreach (var item in source) s += selector(item);
            return s;
        }

        public static double Sum(this IEnumerable<double> source) {
            if (source == null) throw new ArgumentNullException();
            var s = 0d;
            foreach (var x in source) s += x;
            return s;
        }

        public static double Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector) {
            if (source == null) throw new ArgumentNullException();
            if (selector == null) throw new ArgumentNullException();
            var s = 0d;
            foreach (var item in source) s += selector(item);
            return s;
        }

        public static float Sum(this IEnumerable<float> source) {
            if (source == null) throw new ArgumentNullException();
            var s = 0f;
            foreach (var x in source) s += x;
            return s;
        }

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

        public static double Average(this IEnumerable<int> source) {
            if (source == null) throw new ArgumentNullException();
            var sum = 0L;
            var count = 0;
            foreach (var x in source) { sum += x; count++; }
            if (count == 0) throw new InvalidOperationException();
            return (double)sum / count;
        }

        public static double Average(this IEnumerable<long> source) {
            if (source == null) throw new ArgumentNullException();
            var sum = 0d; // double accumulator to avoid overflow risk
            var count = 0;
            foreach (var x in source) { sum += x; count++; }
            if (count == 0) throw new InvalidOperationException();
            return sum / count;
        }

        public static double Average(this IEnumerable<double> source) {
            if (source == null) throw new ArgumentNullException();
            var sum = 0d;
            var count = 0;
            foreach (var x in source) { sum += x; count++; }
            if (count == 0) throw new InvalidOperationException();
            return sum / count;
        }

        public static TAccumulate Aggregate<TSource, TAccumulate>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func) {
            if (source == null) throw new ArgumentNullException();
            if (func == null) throw new ArgumentNullException();
            var acc = seed;
            foreach (var item in source) acc = func(acc, item);
            return acc;
        }

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

        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) =>
            OrderBy(source, keySelector, null);

        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer) {
            if (source == null) throw new ArgumentNullException();
            if (keySelector == null) throw new ArgumentNullException();
            return new OrderedEnumerable<TSource>(
                source,
                new OrderedEnumerable<TSource>.SortKey<TKey>(keySelector, comparer ?? Comparer<TKey>.Default, false));
        }

        public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) =>
            OrderByDescending(source, keySelector, null);

        public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer) {
            if (source == null) throw new ArgumentNullException();
            if (keySelector == null) throw new ArgumentNullException();
            return new OrderedEnumerable<TSource>(
                source,
                new OrderedEnumerable<TSource>.SortKey<TKey>(keySelector, comparer ?? Comparer<TKey>.Default, true));
        }

        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector) {
            if (source == null) throw new ArgumentNullException();
            return source.CreateOrderedEnumerable(keySelector, null, false);
        }

        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer) {
            if (source == null) throw new ArgumentNullException();
            return source.CreateOrderedEnumerable(keySelector, comparer, false);
        }

        public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector) {
            if (source == null) throw new ArgumentNullException();
            return source.CreateOrderedEnumerable(keySelector, null, true);
        }

        public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer) {
            if (source == null) throw new ArgumentNullException();
            return source.CreateOrderedEnumerable(keySelector, comparer, true);
        }

        // ===== Grouping =====

        public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) =>
            GroupBy(source, keySelector, EqualityComparer<TKey>.Default);

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

        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
                this IEnumerable<TSource> source,
                Func<TSource, TKey> keySelector,
                Func<TSource, TElement> elementSelector) =>
            GroupBy(source, keySelector, elementSelector, EqualityComparer<TKey>.Default);

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

        public static List<TSource> ToList<TSource>(this IEnumerable<TSource> source) {
            if (source == null) throw new ArgumentNullException();
            var list = new List<TSource>();
            foreach (var item in source) list.Add(item);
            return list;
        }

        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) =>
            ToDictionary(source, keySelector, EqualityComparer<TKey>.Default);

        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) {
            if (source == null) throw new ArgumentNullException();
            if (keySelector == null) throw new ArgumentNullException();
            var d = new Dictionary<TKey, TSource>(comparer ?? EqualityComparer<TKey>.Default);
            foreach (var item in source) d.Add(keySelector(item), item);
            return d;
        }

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(
                this IEnumerable<TSource> source,
                Func<TSource, TKey> keySelector,
                Func<TSource, TElement> elementSelector) =>
            ToDictionary(source, keySelector, elementSelector, EqualityComparer<TKey>.Default);

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

        public static IEnumerable<int> Range(int start, int count) {
            if (count < 0) throw new ArgumentOutOfRangeException();
            // Guard against int overflow (start + count - 1 > int.MaxValue).
            if (count > 0 && (long)start + count - 1 > int.MaxValue) throw new ArgumentOutOfRangeException();
            return RangeIterator(start, count);
        }

        private static IEnumerable<int> RangeIterator(int start, int count) {
            for (var i = 0; i < count; i++) yield return start + i;
        }

        public static IEnumerable<TResult> Repeat<TResult>(TResult element, int count) {
            if (count < 0) throw new ArgumentOutOfRangeException();
            return RepeatIterator(element, count);
        }

        private static IEnumerable<TResult> RepeatIterator<TResult>(TResult element, int count) {
            for (var i = 0; i < count; i++) yield return element;
        }

        public static IEnumerable<TResult> Empty<TResult>() => EmptyArray<TResult>.Instance;

        private static class EmptyArray<T> {
            public static readonly T[] Instance = new T[0];
        }
    }
}
