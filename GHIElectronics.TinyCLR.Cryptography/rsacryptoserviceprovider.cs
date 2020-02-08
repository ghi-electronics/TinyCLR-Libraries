// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>Microsoft</OWNER>
// 

//
// RSACryptoServiceProvider.cs
//
// CSP-based implementation of RSA
//

namespace GHIElectronics.TinyCLR.Cryptography {
    using System;
    using System.Globalization;
    using System.IO;
    //using System.Security;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;

    //public sealed class RSACryptoServiceProvider : RSA {
    //    private IntPtr impl;

    //    public RSACryptoServiceProvider() =>  this.NativeAcquire();

    //    protected override void Dispose(bool disposing) =>  this.NativeRelase();

    //    public override RSAParameters ExportParameters(bool includePrivateParameters) => this.NativeExportParameters(includePrivateParameters);

    //    public override void ImportParameters(RSAParameters parameters) => this.NativeImportParameters(parameters);

    //    public override byte[] Encrypt(byte[] data, RSAEncryptionPadding padding) => this.NativeEncrypt(data, padding);

    //    public override byte[] Decrypt(byte[] data, RSAEncryptionPadding padding) => this.NativeDecrypt(data, padding);

    //    public override byte[] SignHash(byte[] hash, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) => this.NativeSignHash(hash, hashAlgorithm, padding);

    //    public override bool VerifyHash(byte[] hash, byte[] signature, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding) => this.NativeVerifyHash(hash, signature, hashAlgorithm, padding);

    //    protected override byte[] HashData(byte[] data, int offset, int count, HashAlgorithmName hashAlgorithm) => this.NativeHashData(data, offset, count, hashAlgorithm);

    //    protected override byte[] HashData(Stream data, HashAlgorithmName hashAlgorithm) => throw new NotSupportedException();

    //    [MethodImpl(MethodImplOptions.InternalCall)]
    //    private extern void NativeAcquire();

    //    [MethodImpl(MethodImplOptions.InternalCall)]
    //    private extern void NativeRelase();

    //    [MethodImpl(MethodImplOptions.InternalCall)]
    //    private extern byte[] NativeEncrypt(byte[] data, RSAEncryptionPadding padding);

    //    [MethodImpl(MethodImplOptions.InternalCall)]
    //    private extern byte[] NativeDecrypt(byte[] data, RSAEncryptionPadding padding);

    //    [MethodImpl(MethodImplOptions.InternalCall)]
    //    private extern byte[] NativeSignHash(byte[] hash, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding);

    //    [MethodImpl(MethodImplOptions.InternalCall)]
    //    private extern bool NativeVerifyHash(byte[] hash, byte[] signature, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding);

    //    [MethodImpl(MethodImplOptions.InternalCall)]
    //    private extern byte[] NativeHashData(byte[] data, int offset, int count, HashAlgorithmName hashAlgorithm);

    //    [MethodImpl(MethodImplOptions.InternalCall)]
    //    private extern byte[] NativeHashData(Stream data, HashAlgorithmName hashAlgorithm);

    //    [MethodImpl(MethodImplOptions.InternalCall)]
    //    private extern RSAParameters NativeExportParameters(bool includePrivateParameters);

    //    [MethodImpl(MethodImplOptions.InternalCall)]
    //    private extern void NativeImportParameters(RSAParameters parameters);
    //}
}
