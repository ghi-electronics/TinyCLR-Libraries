namespace System.Security.Cryptography.X509Certificates {
    public class X509Certificate {
        private byte[] data;

        public X509Certificate(byte[] certificate) => this.data = certificate;

        public byte[] GetRawCertData() => this.data;
        public byte[] PrivateKey { get; set; }

        public string Password { get; set; }
    }
}

