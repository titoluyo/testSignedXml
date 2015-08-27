Imports System.Security.Cryptography
Imports System.Security.Cryptography.X509Certificates
Imports System.Xml
Imports System.Deployment.Internal.CodeSigning
Imports System.ServiceModel.Security
Imports System.Text
Imports System.Xml.Linq
Imports System.Xml.Serialization
Imports System.Security.Cryptography.Xml


Module Module1

    Sub Main()

    End Sub

    Public Function SignXmlFileSha256(ByVal FileName As String,
                                  ByVal SignedFileName As String,
                                  ByVal Key As RSA,
                                  ByVal detached As Boolean,
                                  ByVal certX509 As X509Certificate2,
                                  ByRef sErrMsg As String) As String
        sErrMsg = ""
        Dim root As XmlNode
        Dim refChild As XmlNode
        Dim oLegal As XmlNode
        Dim oSignatureCode As XmlNode
        Dim sRSA_SHA256 As String
        Dim sSHA256 As String
        Dim sCanC14 As String

        Try
            Dim doc As New XmlDocument()
            doc.PreserveWhitespace = True
            doc.Load(New XmlTextReader(FileName))


            Dim signedXml As New SignedXml(doc)
            signedXml.SigningKey = Key

            sCanC14 = setURLCert(UrlCert.CanC14)
            sSHA256 = setURLCert(UrlCert.SHA256)
            sRSA_SHA256 = setURLCert(UrlCert.RSA_SHA256)

            signedXml.SignedInfo.CanonicalizationMethod = sCanC14
            signedXml.SignedInfo.SignatureMethod = sRSA_SHA256

            Dim reference As New Reference()
            reference.Uri = ""

            reference.DigestMethod = sSHA256

            Dim env As New XmlDsigEnvelopedSignatureTransform()
            reference.AddTransform(env)
            signedXml.AddReference(reference)
            Dim keyInfo As New KeyInfo()
            keyInfo.AddClause(New KeyInfoX509Data(certX509))
            signedXml.KeyInfo = keyInfo

            signedXml.ComputeSignature()

            Dim xmlDigitalSignature As XmlElement = signedXml.GetXml()
            root = doc.DocumentElement
            Dim lista As XmlNodeList = root.ChildNodes
            For Each nodeParent As XmlNode In lista
                Dim ss As String = nodeParent.Name
                If nodeParent.Name = "legalAuthenticator" Then
                    oLegal = nodeParent
                End If
            Next

            lista = oLegal.ChildNodes

            For Each nodeParent As XmlNode In lista
                Dim ss As String = nodeParent.Name
                If nodeParent.Name = "signatureCode" Then
                    oSignatureCode = nodeParent
                End If
            Next

            refChild = oLegal.InsertAfter(xmlDigitalSignature, oSignatureCode)

            Return doc.OuterXml

        Catch exc As Exception
            sErrMsg = "Function SignXmlFileSha256: " + exc.Source + " - " + exc.Message + vbNewLine
            Return "Eccezione"
        Finally
            root = Nothing
            refChild = Nothing
            oLegal = Nothing
            oSignatureCode = Nothing
        End Try
    End Function

    Public Function setURLCert(ByVal iTipoUrl As UrlCert) As String
        setURLCert = "No_Url"
        If iTipoUrl = 1 Then
            'http://www.w3.org/2001/10/xml-exc-c14n
            Return SecurityAlgorithms.ExclusiveC14n
        ElseIf iTipoUrl = 2 Then
            'http://www.w3.org/2001/04/xmlenc#sha256
            Return SecurityAlgorithms.Sha256Digest
        ElseIf iTipoUrl = 3 Then
            'http://www.w3.org/2001/04/xmldsig-more#rsa-sha256
            Return SecurityAlgorithms.RsaSha256Signature
        End If
    End Function

End Module
