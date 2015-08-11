using System;
using System.Collections.Generic;
using System.Deployment.Internal.CodeSigning;
using System.IO;
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
    public class Sign2
    {
        public static void Sign(XmlDocument doc)
        {
            string signatureCanonicalizationMethod = "http://www.w3.org/2001/10/xml-exc-c14n#";
            string signatureMethod = @"http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
            string digestMethod = @"http://www.w3.org/2001/04/xmlenc#sha256";

            CryptoConfig.AddAlgorithm(typeof(RSAPKCS1SHA256SignatureDescription), signatureMethod);

            X509Certificate2 signingCertificate = GetCertificate();


            SignedXml signer = new SignedXml(doc);

            CspParameters cspParams = new CspParameters(24);
            cspParams.KeyContainerName = "XML_DISG_RSA_KEY";

            RSACryptoServiceProvider key = new RSACryptoServiceProvider(cspParams);
            var strKey = signingCertificate.PrivateKey.ToXmlString(true);
            key.FromXmlString(strKey);

            signer.SigningKey = key;
            signer.KeyInfo = new KeyInfo();
            signer.KeyInfo.AddClause(new KeyInfoX509Data(signingCertificate));

            signer.SignedInfo.CanonicalizationMethod = signatureCanonicalizationMethod;
            signer.SignedInfo.SignatureMethod = signatureMethod;

            //XmlDsigEnvelopedSignatureTransform envelopeTransform = new XmlDsigEnvelopedSignatureTransform();
            XmlDsigExcC14NTransform cn14Transform = new XmlDsigExcC14NTransform();

            Reference signatureReference = new Reference("#FATCA");
            //signatureReference.Uri = signatureReferenceURI;
            //signatureReference.AddTransform(envelopeTransform);
            signatureReference.AddTransform(cn14Transform);
            signatureReference.DigestMethod = digestMethod;

            signer.AddReference(signatureReference);
            //signer.AddReference(new Reference("#tag1"));
            //signer.AddReference(new Reference("#tag3"));

            signer.ComputeSignature();

            XmlElement signatureElement = signer.GetXml();
            String strDoc = signatureElement.OuterXml;
            XmlDocument docResult = new XmlDocument();
            docResult.LoadXml(strDoc);
            //XmlElement obj = docResult.CreateElement("Object",docResult.FirstChild.NamespaceURI);
            //obj.SetAttribute("Id", "FACTA");

            XmlDocumentFragment xfrag = docResult.CreateDocumentFragment();
            xfrag.InnerXml = doc.OuterXml;
            //obj.AppendChild(xfrag);
            //docResult.DocumentElement.AppendChild(obj);
            docResult.DocumentElement.AppendChild(xfrag);

            //doc.DocumentElement.AppendChild(signer.GetXml());

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = false;
            settings.Indent = true;

            var writer = XmlWriter.Create("output1.xml", settings);
            docResult.Save(writer);
            //docResult.Save("output.xml");

            Console.WriteLine("Namespace:{0}",docResult.FirstChild.NamespaceURI);
            //Console.ReadLine();
        }

        private static X509Certificate2 GetCertificate()
        {
            String thumbPrint = "‎44 d5 b4 cf d1 1e b9 e6 37 9c ea 29 8c 71 c8 a6 92 10 db 39";
            thumbPrint = Regex.Replace(thumbPrint, @"\s+", "").Remove(0, 1);
            Console.WriteLine(thumbPrint);

            X509Certificate2 card = null;
            card = new X509Certificate2(@"D:\Fuentes\testSignedXml\TestPacket\SenderCert\sender.p12", "password", X509KeyStorageFlags.Exportable);
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
