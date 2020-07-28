/*
 * TinyFileSystem driver for TinyCLR 2.0
 * 
 * Version 1.0
 *  - Initial revision, based on Chris Taylor (Taylorza) work
 *  - adaptations to conform to MikroBus.Net drivers design
 *  
 *  
 * Copyright 2020 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

using System.Collections;

namespace GHIElectronics.TinyCLR.IO.TinyFileSystem {
    public partial class TinyFileSystem
    {
        /// <summary>
        /// General utility functions.
        /// </summary>
        public class Utilities
        {
            /// <summary>
            /// Sorts an array of strings.
            /// </summary>
            /// <remarks>
            /// Original code by user "Jay Jay"
            /// http://www.tinyclr.com/codeshare/entry/475
            /// Modified to be specifically suites to sorting arrays of strings.    
            /// </remarks>
            /// <param name="array">Array of string to be sorted.</param>
            public static void Sort(string[] array) => Sort(array, 0, array.Length - 1);

            /// <summary>
            /// This is a generic version of C.A.R Hoare's Quick Sort 
            /// algorithm.  This will handle arrays that are already
            /// sorted, and arrays with duplicate keys.
            /// </summary>
            /// <remarks>
            /// If you think of a one dimensional array as going from
            /// the lowest index on the left to the highest index on the right
            /// then the parameters to this function are lowest index or
            /// left and highest index or right.  The first time you call
            /// this function it will be with the parameters 0, a.length - 1.
            /// </remarks>
            /// <param name="array">Array of string to be sorted.</param>
            /// <param name="l">Left boundary of array partition</param>
            /// <param name="r">Right boundary of array partition</param>
            private static void Sort(string[] array, int l, int r)
            {
                const int M = 4;

                if ((r - l) <= M)
                {
                    InsertionSort(array, l, r);
                }
                else
                {
                    var i = (r + l)/2;


                    if (string.Compare(array[l], array[i]) > 0)

                        Swap(array, l, i);

                    if (string.Compare(array[l], array[r]) > 0)
                        Swap(array, l, r);

                    if (string.Compare(array[i], array[r]) > 0)
                        Swap(array, i, r);

                    var j = r - 1;
                    Swap(array, i, j);

                    i = l;
                    var v = array[j];
                    for (;;)
                    {
                        while (string.Compare(array[++i], v) < 0)
                        {
                        }

                        while (string.Compare(array[--j], v) > 0)
                        {
                        }

                        if (j < i)
                            break;
                        Swap(array, i, j);

                    }
                    Swap(array, i, r - 1);

                    Sort(array, l, j);
                    Sort(array, i + 1, r);
                }
            }

            private static void InsertionSort(string[] array, int lo, int hi)
            {
                int i;

                for (i = lo + 1; i <= hi; i++)
                {
                    var v = array[i];
                    var j = i;
                    while ((j > lo) && (string.Compare(array[j - 1], v) > 0))
                    {

                        array[j] = array[j - 1];
                        --j;
                    }
                    array[j] = v;
                }
            }

            private static void Swap(IList list, int left, int right)
            {
                var swap = list[left];
                list[left] = list[right];
                list[right] = swap;
            }
        }
    }
}
