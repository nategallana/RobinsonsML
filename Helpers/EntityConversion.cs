using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobinsonMallLinkingWBOX.Helpers
{
    public class EntityConversion
    {
        public static T Deserialize<T>(string stringValue)
        {
            T temp = default(T);
            try
            {
                System.Xml.Serialization.XmlSerializer xml = new System.Xml.Serialization.XmlSerializer(typeof(T));
                using (System.IO.StringReader reader = new System.IO.StringReader(stringValue))
                {
                    temp = (T)xml.Deserialize(reader);
                }
            }
            catch { /*@DO NOTHING */ }
            return temp;
        }

        public static string Serialize<T>(T entity)
        {
            System.Xml.Serialization.XmlSerializer xml = new System.Xml.Serialization.XmlSerializer(typeof(T));
            string serializeValue = string.Empty;
            try
            {
                using (System.IO.StringWriter writer = new System.IO.StringWriter())
                {
                    xml.Serialize(writer, entity);
                    serializeValue = writer.ToString();
                }
            }
            catch { }
            return serializeValue;
        }
    }
}
