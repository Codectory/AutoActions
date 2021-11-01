using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace AutoHDR.Profiles.Actions
{

    public class ListOfProfileActions : ObservableCollection<IProfileAction>, IXmlSerializable
    {
        public ListOfProfileActions() : base() { }

        public System.Xml.Schema.XmlSchema GetSchema() { return null; }

        public void ReadXml(XmlReader reader)
        {

            try
            {
                object value = reader.GetAttribute("ProfileActions");
                if (string.IsNullOrEmpty((string)value))
                    return;
            }
            catch (Exception)
            {
                return;
            }
            reader.ReadStartElement("ProfileActions");
            while (reader.IsStartElement("IProfileAction"))
            {
                Type type = Type.GetType(reader.GetAttribute("AssemblyQualifiedName"));
                XmlSerializer serial = new XmlSerializer(type);

                reader.ReadStartElement("IProfileAction");
                this.Add((IProfileAction)serial.Deserialize(reader));
                reader.ReadEndElement();
            }
            reader.ReadEndElement();


        }

        public void WriteXml(XmlWriter writer)
        {
            foreach (IProfileAction dispatcher in this)
            {
                writer.WriteStartElement("IProfileAction");
                writer.WriteAttributeString
                ("AssemblyQualifiedName", dispatcher.GetType().AssemblyQualifiedName);
                XmlSerializer xmlSerializer = new XmlSerializer(dispatcher.GetType());
                xmlSerializer.Serialize(writer, dispatcher);
                writer.WriteEndElement();
            }
        }
    }
}
