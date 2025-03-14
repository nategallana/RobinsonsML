using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RobinsonMallLinkingWBOX.ModelsRequest
{
    public class ExternalTransactionRequest
    {
        [XmlAttribute("time")]
        public string time { get; set; }
        
    }
}
