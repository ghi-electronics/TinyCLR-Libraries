namespace System.Security.Cryptography.X509Certificates {
    using System;

    public class X509Certificate {
        private byte[] m_certificate;
        private string m_password;

        protected string m_issuer;
        protected string m_subject;
        protected DateTime m_effectiveDate;
        protected DateTime m_expirationDate;
        protected byte[] m_handle;
        protected byte[] m_sessionHandle;

        public X509Certificate() {

        }

        public X509Certificate(byte[] certificate) => this.m_certificate = certificate;

        public virtual string Issuer => this.m_issuer;

        public virtual string Subject => this.m_subject;

        public virtual DateTime GetEffectiveDate() => this.m_effectiveDate;

        public virtual DateTime GetExpirationDate() => this.m_expirationDate;

        public virtual byte[] GetRawCertData() => this.m_certificate;
    }
}

