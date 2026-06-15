namespace System.Security.Cryptography.X509Certificates {
    /// <summary>Represents an X.509 certificate.</summary>
    public class X509Certificate {
        private byte[] data;

        /// <summary>Initializes a new certificate from the specified raw certificate data.</summary>
        public X509Certificate(byte[] certificate) => this.data = certificate;

        /// <summary>Returns the raw data of the certificate.</summary>
        public byte[] GetRawCertData() => this.data;
        /// <summary>The private key associated with the certificate.</summary>
        public byte[] PrivateKey { get; set; }

        /// <summary>The password used to access the certificate's private key.</summary>
        public string Password { get; set; }
    }
}

