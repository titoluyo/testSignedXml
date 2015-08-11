using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SignXml
{
    class Program
    {
        static void Main(string[] args)
        {
            //String strFile = @"D:\Fuentes\testSignedXml\TestPacket\000000.00000.TA.124.xml";
            //String strFile = @"test1.xml";
            String strFile = @"D:\Fuentes\testSignedXml\8UNBTN.99999.SL.604.xml";
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.Load(new XmlTextReader(strFile));

            doc = RemoveHeader(doc);

            //Sign1.SignDocument(doc);
            Sign2.Sign(doc);
        }

        static XmlDocument RemoveHeader(XmlDocument doc)
        {

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;

            var stringWriter = new StringWriter();
            var writer = XmlWriter.Create(stringWriter, settings);
            doc.Save(writer);

            XmlDocument docResult = new XmlDocument();
            var objElem = docResult.CreateElement("Object");
            objElem.SetAttribute("Id", "FATCA");
            docResult.AppendChild(objElem);

            XmlDocumentFragment xfrag = docResult.CreateDocumentFragment();
            xfrag.InnerXml = stringWriter.ToString();
            docResult.DocumentElement.AppendChild(xfrag);
            //docResult.LoadXml(stringWriter.ToString());
            Console.WriteLine(docResult.OuterXml);
            return docResult;
        }
    }
}
