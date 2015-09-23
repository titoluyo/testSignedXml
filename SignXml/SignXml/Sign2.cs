using System;
using System.Deployment.Internal.CodeSigning;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text.RegularExpressions;
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
            var x509Data = new KeyInfoX509Data(signingCertificate);
            //SubjectName
            var dname = new X500DistinguishedName(signingCertificate.SubjectName);
            Console.WriteLine("X500DistinguishedName: {0}{1}", dname.Name, Environment.NewLine);
            x509Data.AddSubjectName(dname.Name);
            signer.KeyInfo.AddClause(x509Data);

            signer.SignedInfo.CanonicalizationMethod = signatureCanonicalizationMethod;
            signer.SignedInfo.SignatureMethod = signatureMethod;

            //XmlDsigEnvelopedSignatureTransform envelopeTransform = new XmlDsigEnvelopedSignatureTransform();
            Transform cn14Transform = new XmlDsigExcC14NTransform();
            //Transform cn14Transform = new XmlDsigC14NTransform();

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
            Console.WriteLine(signatureElement.NamespaceURI);
            String strDoc = signatureElement.OuterXml;
            XmlDocument docResult = new XmlDocument();
            docResult.LoadXml(strDoc);
            Console.WriteLine(signatureElement.NamespaceURI);
            //XmlElement obj = docResult.CreateElement("Object",docResult.FirstChild.NamespaceURI);
            //obj.SetAttribute("Id", "FACTA");

            var xfrag = docResult.CreateDocumentFragment();


            xfrag.InnerXml = doc.OuterXml;
            //xfrag.FirstChild.Attributes.Append(xfrag.C.CreateAttribute("xx")).Value = docResult.NamespaceURI;

            //obj.AppendChild(xfrag);
            //docResult.DocumentElement.AppendChild(obj);
            if (docResult.DocumentElement == null)
                throw new Exception("docResult.DocumentElement no puede ser NULL");
            var node = docResult.DocumentElement.AppendChild(xfrag);
            //node.FirstChild.Attributes.Append(doc.CreateAttribute("xx")).Value = docResult.NamespaceURI;

            //doc.DocumentElement.AppendChild(signer.GetXml());

            var settings = new XmlWriterSettings();
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
            //Sample testPacket
            //String thumbPrint = "‎44 d5 b4 cf d1 1e b9 e6 37 9c ea 29 8c 71 c8 a6 92 10 db 39";
            //Certificado Ejemplo Raiz
            var thumbPrint = "‎da c8 62 b8 0a d2 ec cc e6 fb 6c 62 f3 ae 45 a0 2e 57 1a 9d";
            thumbPrint = Regex.Replace(thumbPrint, @"\s+", "").Remove(0, 1);
            Console.WriteLine(thumbPrint);

            X509Certificate2 card;
            //card = new X509Certificate2(@"D:\Fuentes\testSignedXml\TestPacket\SenderCert\sender.p12", "password", X509KeyStorageFlags.Exportable);
            card = new X509Certificate2(@"D:\Fuentes\testSignedXml\Certificates\RaizPeruV1.pfx", "raizperu", X509KeyStorageFlags.Exportable);
            //card = new X509Certificate2(@"D:\Fuentes\testSignedXml\EDPYME RAIZ S.A. RUC 2042572411900-2048-SHA256withRSA.pfx", "raizperu", X509KeyStorageFlags.Exportable);
            /*
            X509Store store = new X509Store(StoreName.TrustedPublisher, StoreLocation.CurrentUser);
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
