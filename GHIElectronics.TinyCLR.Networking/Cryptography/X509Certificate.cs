namespace System.Security.Cryptography.X509Certificates {
    public class X509Certificate {

    }

    public class X509Certificate2 : X509Certificate {
        private byte[] data;

        public X509Certificate2(byte[] certificate) => this.data = certificate;

        public byte[] GetRawCertData() => this.data;
        public byte[] PrivateKey { get; set; }
    }
}

