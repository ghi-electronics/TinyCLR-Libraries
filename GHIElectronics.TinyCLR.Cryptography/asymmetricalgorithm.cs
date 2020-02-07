// ==++==   
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>Microsoft</OWNER>
// 

//
// AsymmetricAlgorithm.cs
//

using System;

namespace GHIElectronics.TinyCLR.Cryptography {

    public abstract class AsymmetricAlgorithm : IDisposable {
        protected int KeySizeValue;
        protected KeySizes[] LegalKeySizesValue;

        //
        // public constructors
        //

        protected AsymmetricAlgorithm() {}

        // AsymmetricAlgorithm implements IDisposable

        public void Dispose() => this.Clear();

        public void Clear() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            return;
        }
    
        //
        // public properties
        //
    
        public virtual int KeySize {
            get => this.KeySizeValue;
            set {
                int i;
                int j;

                for (i = 0; i < this.LegalKeySizesValue.Length; i++) {
                    if (this.LegalKeySizesValue[i].SkipSize == 0) {
                        if (this.LegalKeySizesValue[i].MinSize == value) { // assume MinSize = MaxSize
                            this.KeySizeValue = value;
                            return;
                        }
                    }
                    else {
                        for (j = this.LegalKeySizesValue[i].MinSize; j <= this.LegalKeySizesValue[i].MaxSize;
                             j += this.LegalKeySizesValue[i].SkipSize) {
                            if (j == value) {
                                this.KeySizeValue = value;
                                return;
                            }
                        }
                    }
                }
                throw new CryptographicException("Invalid key size.");
            }
        }

        public virtual KeySizes[] LegalKeySizes => (KeySizes[])this.LegalKeySizesValue.Clone();

        // This method must be implemented by derived classes. In order to conform to the contract, it cannot be abstract.
        public virtual string SignatureAlgorithm => throw new NotImplementedException();

        // This method must be implemented by derived classes. In order to conform to the contract, it cannot be abstract.
        public virtual string KeyExchangeAlgorithm => throw new NotImplementedException();

        //
        // public methods
        //

        //static public AsymmetricAlgorithm Create() {
        //    // Use the crypto config system to return an instance of
        //    // the default AsymmetricAlgorithm on this machine
        //    return Create("System.Security.Cryptography.AsymmetricAlgorithm");
        //}

        //static public AsymmetricAlgorithm Create(string algName) {
        //    return (AsymmetricAlgorithm) CryptoConfig.CreateFromName(algName);
        //}

        // This method must be implemented by derived classes. In order to conform to the contract, it cannot be abstract.
        //public virtual void FromXmlString(String xmlString) {
        //    throw new NotImplementedException();
        //}

        //// This method must be implemented by derived classes. In order to conform to the contract, it cannot be abstract.
        //public virtual String ToXmlString(bool includePrivateParameters) {
        //    throw new NotImplementedException();
        //}
    }
}    
