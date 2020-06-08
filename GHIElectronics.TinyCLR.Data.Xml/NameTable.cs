////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace System.Xml
{

    // <devdoc>
    //    <para>
    //       XmlNameTable implemented as a simple hash table.
    //    </para>
    // </devdoc>
    public class NameTable : XmlNameTable
    {
        //
        // Private types
        //
        class Entry
        {
            internal string str;
            internal int hashCode;
            internal Entry next;

            internal Entry(string str, int hashCode, Entry next)
            {
                this.str = str;
                this.hashCode = hashCode;
                this.next = next;
            }
        }

        //
        // Fields
        //
        Entry[] entries;
        int count;
        int mask;

        //
        // Constructor
        //
        // <devdoc>
        //      Public constructor.
        // </devdoc>
        public NameTable()
        {
            this.mask = 31;
            this.entries = new Entry[this.mask + 1];
        }

        //
        // XmlNameTable public methods
        //
        // <devdoc>
        //      Add the given string to the NameTable or return
        //      the existing string if it is already in the NameTable.
        // </devdoc>
        public override string Add(string array)
        {
            if (array == null)
            {
                throw new ArgumentNullException("key");
            }

            if (array.Length == 0)
            {
                return "";
            }

            var len = array.Length;
            var hashCode = len;
            // use array.Length to eliminate the rangecheck
            for (var i = 0; i < array.Length; i++)
            {
                hashCode += (hashCode << 7) ^ array[i];
            }

            // mix it a bit more
            hashCode -= hashCode >> 17;
            hashCode -= hashCode >> 11;
            hashCode -= hashCode >> 5;

            for (var e = this.entries[hashCode & this.mask]; e != null; e = e.next)
            {
                if (e.hashCode == hashCode && string.Compare(e.str, array) == 0)
                {
                    return e.str;
                }
            }

            return this.AddEntry(array, hashCode);
        }

        // <devdoc>
        //      Add the given string to the NameTable or return
        //      the existing string if it is already in the NameTable.
        // </devdoc>
        public override string Add(char[] array, int offset, int length)
        {
            if (length == 0)
            {
                return "";
            }

            var hashCode = length;
            hashCode += (hashCode << 7) ^ array[offset];   // this will throw IndexOutOfRangeException in case the start index is invalid
            var end = offset + length;
            for (var i = offset + 1; i < end; i++)
            {
                hashCode += (hashCode << 7) ^ array[i];
            }

            // mix it a bit more
            hashCode -= hashCode >> 17;
            hashCode -= hashCode >> 11;
            hashCode -= hashCode >> 5;

            for (var e = this.entries[hashCode & this.mask]; e != null; e = e.next)
            {
                if (e.hashCode == hashCode && TextEquals(e.str, array, offset))
                {
                    return e.str;
                }
            }

            return this.AddEntry(new string(array, offset, length), hashCode);
        }

        // <devdoc>
        //      Find the matching string in the NameTable.
        // </devdoc>
        public override string Get(string array)
        {
            if (array == null)
            {
                throw new ArgumentNullException("value");
            }

            if (array.Length == 0)
            {
                return "";
            }

            var len = array.Length;
            var hashCode = len;
            // use array.Length to eliminate the rangecheck
            for (var i = 0; i < array.Length; i++)
            {
                hashCode += (hashCode << 7) ^ array[i];
            }

            // mix it a bit more
            hashCode -= hashCode >> 17;
            hashCode -= hashCode >> 11;
            hashCode -= hashCode >> 5;

            for (var e = this.entries[hashCode & this.mask]; e != null; e = e.next)
            {
                if (e.hashCode == hashCode && string.Compare(e.str, array) == 0)
                {
                    return e.str;
                }
            }

            return null;
        }

        // <devdoc>
        //      Find the matching string atom given a range of
        //      characters.
        // </devdoc>
        public override string Get(char[] array, int offset, int length)
        {
            if (length == 0)
            {
                return "";
            }

            var hashCode = length;
            hashCode += (hashCode << 7) ^ array[offset];   // this will throw IndexOutOfRangeException in case the start index is invalid
            var end = offset + length;
            for (var i = offset + 1; i < end; i++)
            {
                hashCode += (hashCode << 7) ^ array[i];
            }

            // mix it a bit more
            hashCode -= hashCode >> 17;
            hashCode -= hashCode >> 11;
            hashCode -= hashCode >> 5;

            for (var e = this.entries[hashCode & this.mask]; e != null; e = e.next)
            {
                if (e.hashCode == hashCode && TextEquals(e.str, array, offset))
                {
                    return e.str;
                }
            }

            return null;
        }

        //
        // Private methods
        //

        private string AddEntry(string str, int hashCode)
        {
            var index = hashCode & this.mask;
            var e = new Entry(str, hashCode, this.entries[index]);
            this.entries[index] = e;
            if (this.count++ == this.mask)
            {
                this.Grow();
            }

            return e.str;
        }

        private void Grow()
        {
            var newMask = this.mask * 2 + 1;
            var oldEntries = this.entries;
            var newEntries = new Entry[newMask + 1];

            // use oldEntries.Length to eliminate the rangecheck
            for (var i = 0; i < oldEntries.Length; i++)
            {
                var e = oldEntries[i];
                while (e != null)
                {
                    var newIndex = e.hashCode & newMask;
                    var tmp = e.next;
                    e.next = newEntries[newIndex];
                    newEntries[newIndex] = e;
                    e = tmp;
                }
            }

            this.entries = newEntries;
            this.mask = newMask;
        }

        private static bool TextEquals(string array, char[] text, int start)
        {
            // use array.Length to eliminate the rangecheck
            for (var i = 0; i < array.Length; i++)
            {
                if (array[i] != text[start + i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}


