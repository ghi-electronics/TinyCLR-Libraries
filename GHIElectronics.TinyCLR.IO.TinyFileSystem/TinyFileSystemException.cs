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
        [Serializable]
        internal class TinyFileSystemException : Exception
        {
            public TinyFileSystemException()
            {
            }

            public TinyFileSystemException(string message)
                : base(message)
            {
            }

            public TinyFileSystemException(string message, TfsErrorCode errorCode)
                : base(message) => this.ErrorCode = errorCode;

            public TinyFileSystemException(string message, Exception innerException)
                : base(message, innerException)
            {
            }

            public TfsErrorCode ErrorCode { get; }

            public enum TfsErrorCode
            {
                NotFormatted = -1,
                FileInUse = -2,
                DiskFull = -3
            }
        }
    }
}
