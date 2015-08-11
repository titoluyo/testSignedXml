using System;
using System.Collections.Generic;
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
            String strFile = @"D:\Fuentes\testSignedXml\TestPacket\000000.00000.TA.124.xml";
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.Load(new XmlTextReader(strFile));
            Sign1.SignDocument(doc);
        }
    }
}
