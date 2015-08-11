using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Deployment.Internal.CodeSigning;

namespace SignXml
{
    public class SignatureSupportUtility
    {

        private bool IsSignatureContentTransform
        {
            get
            {
                return true;
                //get IsSignatureContentTransform                
            }
        }

        public SignatureSupportUtility()
        {
            Register();
        }


        private static void Register()
        {
            CryptoConfig.AddAlgorithm(typeof (RSAPKCS1SHA256SignatureDescription),
                "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256");
        }

        private void Sign(Message message, string[] elementIdsToSign, string[] attachmentsToSign, string wssNamespace,
            X509Certificate2 certificate)
        {
            //Prepare XML to encrypt and sign
            var element = this.PrepareEncyrptSign(message);

            bool signEntireDocument = true;
            string elementToBeSigned = string.Empty;
            var signedMessage = new XmlDocument();
            signedMessage.AppendChild(signedMessage.ImportNode(element, true));

            SignatureType signAs = SignatureType.InternallyDetached;
            signedMessage.PreserveWhitespace = false;

            OverrideSignedXml signedXml = new OverrideSignedXml(signedMessage);
            signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;

            if (elementIdsToSign != null && elementIdsToSign.Length > 0)
            {
                bool isContentTransform = this.IsSignatureContentTransform;

                foreach (string s in elementIdsToSign)
                {
                    // Create a reference to be signed.
                    Reference reference = new Reference(string.Format("#{0}", s));
                    reference.AddTransform(new XmlDsigExcC14NTransform());
                    reference.DigestMethod = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";


                    // Add the reference to the SignedXml object.
                    signedXml.AddReference(reference);
                }

                signEntireDocument = false;
            }

            // Reference attachments to sign
            if (attachmentsToSign != null && attachmentsToSign.Length > 0)
            {
                bool isContentTransform = this.IsSignatureContentTransform;

                foreach (string attachmentId in attachmentsToSign)
                {
                    // Create a reference to be signed.
                    Reference reference = new Reference(string.Format("{0}{1}", Constants.CidUriScheme, attachmentId));
                    reference.DigestMethod = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";

                    if (isContentTransform)
                    {
                        AttachmentContentSignatureTransform env = new AttachmentContentSignatureTransform();
                        reference.AddTransform(env);
                    }
                    else
                    {
                        AttachmentCompleteSignatureTransform env = new AttachmentCompleteSignatureTransform();
                        reference.AddTransform(env);
                    }

                    // Add the reference to the SignedXml object.
                    signedXml.AddReference(reference);
                }

                signEntireDocument = false;
            }

            if (signEntireDocument)
            {
                Reference reference = new Reference();
                reference.Uri = "";
                reference.DigestMethod = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";

                XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
                reference.AddTransform(env);
                signedXml.AddReference(reference);
                signAs = SignatureType.Enveloped;
            }

            string x509CertificateReferenceId = string.Format("{0}-{1}", Constants.IdAttributeName,
                Guid.NewGuid().ToString("N"));
            KeyInfo keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509SecurityTokenReference(string.Format("#{0}", x509CertificateReferenceId),
                wssNamespace));
            signedXml.KeyInfo = keyInfo;
            signedXml.SignedInfo.SignatureMethod = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";

            RSA key = (RSACryptoServiceProvider) certificate.PrivateKey;
            signedXML.SigningKey = key;
            CidWebRequest.Message = message;

            signedXml.ComputeSignature();
            var xmlSignature = signedXml.GetXml();
            XmlDocument unsignedEnvelopeDoc = new XmlDocument();
            unsignedEnvelopeDoc.LoadXml(message.MessageAsString);
        }
    }
} 
