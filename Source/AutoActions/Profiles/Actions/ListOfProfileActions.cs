using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace AutoActions.Profiles.Actions
{


    public class ListOfProfileActions : ObservableCollection<IProfileAction>/*, IXmlSerializable*/
    {
        public ListOfProfileActions() : base() { }

        public System.Xml.Schema.XmlSchema GetSchema() { return null; }

        public void ReadXml(XmlReader reader)
        {

            reader.Read();
            if (!reader.IsStartElement("IProfileAction"))
                return;
            while (reader.IsStartElement("IProfileAction"))
            {
                object value = reader.GetAttribute("AssemblyQualifiedName");
                if (string.IsNullOrEmpty((string)value))
                    return;

                Type type = Type.GetType((string)value);

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

                DataContractSerializer serializer = new DataContractSerializer(dispatcher.GetType());
                serializer.WriteObject(writer, dispatcher);
                writer.WriteEndElement();
            }
        }


        public static void SaveSettings(UserAppSettings settings, string path)
        {
            try
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(UserAppSettings));

                var writeSettings = new XmlWriterSettings()
                {
                    Indent = true

                };


                using (var xmlWriter = XmlWriter.Create(path, writeSettings))
                    serializer.WriteObject(xmlWriter, settings);
            }
            catch (Exception ex)
            {
                Globals.Logs.AddException(ex);
                throw;
            }
        }
    }
    
}
