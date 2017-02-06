﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


namespace System.Collections {
    public class DictionaryEntry {
        public object Key;
        public object Value;

        public DictionaryEntry(object key, object value) {
            this.Key = key;
            this.Value = value;
        }
    }
}
