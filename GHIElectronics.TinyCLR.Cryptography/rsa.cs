// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>Microsoft</OWNER>
// 

//
//  RSA.cs
//

namespace GHIElectronics.TinyCLR.Cryptography {
    using System.IO;
    using System.Text;
    //using System.Runtime.Serialization;
    //using System.Security.Util;
    using System.Globalization;
    using System;

    //using System.Diagnostics.Contracts;

    // We allow only the public components of an RSAParameters object, the Modulus and Exponent
    // to be serializable.
    [Serializable]

    public struct RSAParameters {
        public byte[]      Exponent;
        public byte[]      Modulus;

        [NonSerialized] public byte[]      P;
        [NonSerialized] public byte[]      Q;
        [NonSerialized] public byte[]      DP;
        [NonSerialized] public byte[]      DQ;
        [NonSerialized] public byte[]      InverseQ;
        [NonSerialized] public byte[]      D;

    }


    public abstract class RSA : AsymmetricAlgorithm {
        protected RSA() { }
        //
        // public methods
        //

        //new static public RSA Create() => (RSA)Create("System.Security.Cryptography.RSA");

        public virtual byte[] Encrypt(byte[] data, RSAEncryptionPadding padding) => throw DerivedClassMustOverride();

        public virtual byte[] Decrypt(byte[] data, RSAEncryptionPadding padding) => throw DerivedClassMustOverride();

        public virtual byte[] SignHash(byte[] hash, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) => throw DerivedClassMustOverride();

        public virtual bool VerifyHash(byte[] hash, byte[] signature, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) => throw DerivedClassMustOverride();

        protected virtual byte[] HashData(byte[] data, int offset, int count, HashAlgorithmName hashAlgorithm) => throw DerivedClassMustOverride();

        protected virtual byte[] HashData(Stream data, HashAlgorithmName hashAlgorithm) => throw DerivedClassMustOverride();

        public byte[] SignData(byte[] data, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) {
            if (data == null) {
                throw new ArgumentNullException("data");
            }
            return this.SignData(data, 0, data.Length, hashAlgorithm, padding);
        }

        public virtual byte[] SignData(byte[] data, int offset, int count, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) {
            if (data == null) {
                throw new ArgumentNullException("data");
            }
            if (offset < 0 || offset > data.Length) {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (count < 0 || count > data.Length - offset) {
                throw new ArgumentOutOfRangeException("count");
            }
            if (string.IsNullOrEmpty(hashAlgorithm.Name)) {
                throw HashAlgorithmNameNullOrEmpty();
            }
            if (padding == null) {
                throw new ArgumentNullException("padding");
            }

            var hash = this.HashData(data, offset, count, hashAlgorithm);
            return this.SignHash(hash, hashAlgorithm, padding);
        }

        public virtual byte[] SignData(Stream data, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) {
            if (data == null) {
                throw new ArgumentNullException("data");
            }
            if (string.IsNullOrEmpty(hashAlgorithm.Name)) {
                throw HashAlgorithmNameNullOrEmpty();
            }
            if (padding == null) {
                throw new ArgumentNullException("padding");
            }
    
            var hash = this.HashData(data, hashAlgorithm);
            return this.SignHash(hash, hashAlgorithm, padding);
        }

        public bool VerifyData(byte[] data, byte[] signature, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) {
            if (data == null) {
                throw new ArgumentNullException("data");
            }
            return this.VerifyData(data, 0, data.Length, signature, hashAlgorithm, padding);
        }

        public virtual bool VerifyData(byte[] data, int offset, int count, byte[] signature, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) {
            if (data == null) {
                throw new ArgumentNullException("data");
            }
            if (offset < 0 || offset > data.Length) {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (count < 0 || count > data.Length - offset) {
                throw new ArgumentOutOfRangeException("count"); 
            }
            if (signature == null) {
                throw new ArgumentNullException("signature");
            }
            if (string.IsNullOrEmpty(hashAlgorithm.Name)) {
                throw HashAlgorithmNameNullOrEmpty();
            }
            if (padding == null) {
                throw new ArgumentNullException("padding");
            }

            var hash = this.HashData(data, offset, count, hashAlgorithm);
            return this.VerifyHash(hash, signature, hashAlgorithm, padding);
        }

        public bool VerifyData(Stream data, byte[] signature, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) {
            if (data == null) {
                throw new ArgumentNullException("data");
            }
            if (signature == null) {
                throw new ArgumentNullException("signature");
            }
            if (string.IsNullOrEmpty(hashAlgorithm.Name)) {
                throw HashAlgorithmNameNullOrEmpty();
            }
            if (padding == null) {
                throw new ArgumentNullException("padding");
            }
 
            var hash = this.HashData(data, hashAlgorithm);
            return this.VerifyHash(hash, signature, hashAlgorithm, padding);
        }

        private static Exception DerivedClassMustOverride() => new NotImplementedException("Not support.");

        internal static Exception HashAlgorithmNameNullOrEmpty() => new ArgumentException("Cryptography HashAlgorithmName null or empty", "hashAlgorithm");
      
        public virtual byte[] DecryptValue(byte[] rgb) => throw new NotSupportedException("Not support");

        public virtual byte[] EncryptValue(byte[] rgb) => throw new NotSupportedException("Not support");
        

        public override string KeyExchangeAlgorithm => "RSA";

        public override string SignatureAlgorithm => "RSA";

        abstract public RSAParameters ExportParameters(bool includePrivateParameters);

        abstract public void ImportParameters(RSAParameters parameters);
    }
}
