using System;
using System.Collections;

namespace GHIElectronics.TinyCLR.Networking.Mqtt.Utility
{
    /// <summary>
    /// Extension class for a Queue
    /// </summary>
    internal static class QueueExtension
    {
        /// <summary>
        /// Predicate for searching inside a queue
        /// </summary>
        /// <param name="item">Item of the queue</param>
        /// <returns>Result of predicate</returns>
        internal delegate bool QueuePredicate(object item);

        /// <summary>
        /// Get (without removing) an item from queue based on predicate
        /// </summary>
        /// <param name="queue">Queue in which to search</param>
        /// <param name="predicate">Predicate to verify to get item</param>
        /// <returns>Item matches the predicate</returns>
        internal static object Get(this Queue queue, QueuePredicate predicate)
        {
            foreach (var item in queue)
            {
                if (predicate(item))
                    return item;
            }
            return null;
        }
    }
}
