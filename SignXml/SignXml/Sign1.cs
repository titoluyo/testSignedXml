using System;
using System.Collections.Generic;
using System.Deployment.Internal.CodeSigning;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace SignXml
{
    public class Sign1
    {
        public static XmlDocument SignDocument(XmlDocument doc)
        {
            ////////////////
            string signatureCanonicalizationMethod = "http://www.w3.org/2001/10/xml-exc-c14n#";
            string signatureMethod = @"http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
            string digestMethod = @"http://www.w3.org/2001/04/xmlenc#sha256";

            string signatureReferenceURI = "#_73e63a41-156d-4fda-a26c-8d79dcade713";

            CryptoConfig.AddAlgorithm(typeof(RSAPKCS1SHA256SignatureDescription), signatureMethod);

            X509Certificate2 signingCertificate = GetCertificate();
            //
            /* add the following lines of code after var signingCertificate = GetCertificate();*/
            CspParameters cspParams = new CspParameters(24);
            //cspParams.KeyContainerName = "XML_DISG_RSA_KEY";
            RSACryptoServiceProvider key = new RSACryptoServiceProvider(cspParams);
            var strKey = signingCertificate.PrivateKey.ToXmlString(true);
            key.FromXmlString(strKey);
            /*assign the new key to signer's SigningKey */
            //metadataSigner.SigningKey = key;

            //
            SignedXml signer = new SignedXml(doc);
            signer.SigningKey = key;//signingCertificate.PrivateKey;
            signer.KeyInfo = new KeyInfo();
            signer.KeyInfo.AddClause(new KeyInfoX509Data(signingCertificate));

            signer.SignedInfo.CanonicalizationMethod = signatureCanonicalizationMethod;
            signer.SignedInfo.SignatureMethod = signatureMethod;

            XmlDsigEnvelopedSignatureTransform envelopeTransform = new XmlDsigEnvelopedSignatureTransform();
            XmlDsigExcC14NTransform cn14Transform = new XmlDsigExcC14NTransform();

            Reference signatureReference = new Reference();
            signatureReference.Uri = signatureReferenceURI;
            signatureReference.AddTransform(envelopeTransform);
            signatureReference.AddTransform(cn14Transform);
            signatureReference.DigestMethod = digestMethod;

            signer.AddReference(signatureReference);

            signer.ComputeSignature();
            XmlElement signatureElement = signer.GetXml();

            doc.DocumentElement.AppendChild(signer.GetXml());

            return doc;
        }


        private static X509Certificate2 GetCertificate()
        {
            String thumbPrint = "‎44 d5 b4 cf d1 1e b9 e6 37 9c ea 29 8c 71 c8 a6 92 10 db 39";
            thumbPrint = Regex.Replace(thumbPrint, @"\s+", "").Remove(0,1);
            Console.WriteLine(thumbPrint);

            X509Certificate2 card = null;
            card = new X509Certificate2(@"D:\Fuentes\testSignedXml\TestPacket\SenderCert\sender.p12", "password");
            /*
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            Console.WriteLine("Cantidad:{0}", store.Certificates.Count);
            foreach (X509Certificate2 cert in store.Certificates)
            {
                if (!cert.HasPrivateKey) continue;
                Console.WriteLine(cert.Thumbprint);
                if (cert.Thumbprint.Equals(thumbPrint, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Found!");
                    card = cert;
                    break;
                }
            }
            store.Close();
            */

            return card;
        }
    }
}
