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

using System;

namespace GHIElectronics.TinyCLR.IO.TinyFileSystem {
    public partial class TinyFileSystem
    {
        /// <summary>
        /// Dynamically growing array of UInt16 (ushort) elements.    
        /// </summary>
        public class UInt16Array
        {
            private const int DefaultCapacity = 4;
            private int capacity = DefaultCapacity;
            private ushort[] array = new ushort[DefaultCapacity];

            /// <summary>
            /// Gets or sets the element at the specified index.
            /// </summary>
            /// <param name="index">The zero-based index of the element to get or set.</param>
            /// <returns>The element a the specified index.</returns>
            public ushort this[int index] {
                get {
                    if (index < 0 || index >= this.Count) throw new ArgumentOutOfRangeException("index");
                    return this.array[index];
                }
                set => this.Set(index, value);
            }

            /// <summary>
            /// Gets the number of elements contained in the array.
            /// </summary>
            public int Count { get; private set; }

            /// <summary>
            /// Adds an element to the end of the collection.
            /// </summary>
            /// <param name="value">Value of the element to add.</param>
            /// <returns>The new count of items in the array.</returns>
            public int Add(ushort value)
            {
                if (this.Count == this.capacity)
                {
                    this.Grow(this.capacity << 1);
                }
                this.array[this.Count++] = value;
                return this.Count;
            }

            /// <summary>
            /// Adjusts the length of the array. 
            /// This can be used to trim the end of the array.
            /// </summary>
            /// <param name="length">New length of the array.</param>
            public void SetLength(int length)
            {
                if (length < 0) throw new ArgumentOutOfRangeException("length");
                if (length > this.Count)
                {
                    if (length > this.capacity)
                        this.Grow(DefaultCapacity +
                             (int) Math.Ceiling((double) length/DefaultCapacity)*DefaultCapacity);
                }
                this.Count = length;
            }

            /// <summary>
            /// Sets the value of an element at the specified index.
            /// If the index is beyond the end of the array, the array will grow 
            /// to accomodate the new element.
            /// </summary>
            /// <param name="index">The zero-based index of the element to set.</param>
            /// <param name="value"></param>
            private void Set(int index, ushort value)
            {
                if (index < 0) throw new ArgumentOutOfRangeException("index");
                if (index >= this.capacity)
                    this.Grow(DefaultCapacity + (int) Math.Ceiling((double) index/DefaultCapacity)*DefaultCapacity);
                this.array[index] = value;
                if (index >= this.Count) this.Count = index + 1;
            }

            /// <summary>
            /// Grows the internal array to increase the capacity.
            /// </summary>
            /// <param name="newSize">New size of the array.</param>
            private void Grow(int newSize)
            {
                var newArray = new ushort[newSize];
                Array.Copy(this.array, newArray, this.capacity);
                this.capacity = newArray.Length;
                this.array = newArray;
            }
        }
    }
}
