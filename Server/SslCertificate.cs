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

        private const string certName = "x509cert.pfx";
        private X509Certificate2 Certificate;

        public X509Certificate2 GetCertificate()
        {
            return Certificate;
        }

        private void GenerateX509Certificate()
        {
            //Previous we took cert from special library after creating cert in command line. Now we take cert from file in app folder.
            //here is needed a x509 cert 
            //in app folder must be a cert equal to server (copy of it in server and client).
            //to create cert in your system you have to open power shell as administrator and write some lines:
            ///New-SelfSignedCertificate -Subject "CN=MySslSocketCertificate" -KeySpec "Signature" -CertStoreLocation "Cert:\CurrentUser\My"
            ///dir cert:\CurrentUser\My     here you have to copy thumbprint of cert
            ///$PFXPass = ConvertTo-SecureString -String “MyPassword” -Force -AsPlainText
            ///Export-PfxCertificate -Cert cert:\CurrentUser\My\___Thumbprint_of_cert____ -Password $PFXPass -FilePath C:\Users\x509cert.pfx
            Certificate = new X509Certificate2("x509cert.pfx", "MyPassword", X509KeyStorageFlags.MachineKeySet);
        }

        public bool IsCertificateValid(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return sslPolicyErrors == SslPolicyErrors.None || sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors;
        }

    }
}
