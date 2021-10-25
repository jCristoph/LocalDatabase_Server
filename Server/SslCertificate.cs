using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace LocalDatabase_Server.Server
{
    public class SslCertificate

       
    {
        public SslCertificate()
        {
            GenerateX509Certificate();
        }


        private X509Certificate2 Certificate;

        public X509Certificate2 GetCertificate()
        {
            return Certificate;
        }

        private void GenerateX509Certificate()
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);

            foreach (X509Certificate2 currentCertificate
               in store.Certificates)
            {
                if (currentCertificate.IssuerName.Name
                   != null && currentCertificate.IssuerName.
                   Name.Equals("CN=MySslSocketCertificate"))
                {
                    this.Certificate = currentCertificate;
                    break;
                }
            }
           
        }

        public bool IsCertificateValid(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return sslPolicyErrors == SslPolicyErrors.None || sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors;
        }

    }
}
