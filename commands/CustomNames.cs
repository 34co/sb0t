﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using iconnect;

namespace commands
{
    class CustomNames
    {
        private class Item
        {
            public Guid Guid { get; set; }
            public String Name { get; set; }
            public String Text { get; set; }
        }

        private static List<Item> list { get; set; }

        public static void Set(IUser client)
        {
            Item i = list.Find(x => x.Guid.Equals(client.Guid) && x.Name == client.Name);

            if (i != null)
                client.CustomName = i.Text;
        }

        public static void UpdateCustomName(IUser client)
        {
            list.RemoveAll(x => x.Guid.Equals(client.Guid) && x.Name == client.Name);

            if (!String.IsNullOrEmpty(client.CustomName))
                list.Add(new Item
                {
                    Guid = client.Guid,
                    Name = client.Name,
                    Text = client.CustomName
                });

            Save();
        }

        public static void Load()
        {
            list = new List<Item>();

            try
            {
                XmlDocument xml = new XmlDocument();
                xml.PreserveWhitespace = true;
                xml.Load(Path.Combine(Server.DataPath, "customnames.xml"));

                XmlNodeList nodes = xml.GetElementsByTagName("item");

                foreach (XmlElement e in nodes)
                    list.Add(new Item
                    {
                        Guid = new Guid(e.GetElementsByTagName("guid")[0].InnerText),
                        Name = e.GetElementsByTagName("name")[0].InnerText,
                        Text = e.GetElementsByTagName("text")[0].InnerText
                    });
            }
            catch { }
        }

        private static void Save()
        {
            XmlDocument xml = new XmlDocument();
            xml.PreserveWhitespace = true;
            xml.AppendChild(xml.CreateXmlDeclaration("1.0", null, null));
            XmlNode root = xml.AppendChild(xml.CreateElement("customnames"));

            foreach (Item i in list)
            {
                XmlNode item = root.OwnerDocument.CreateNode(XmlNodeType.Element, "item", root.BaseURI);
                root.AppendChild(item);

                XmlNode guid = item.OwnerDocument.CreateNode(XmlNodeType.Element, "guid", item.BaseURI);
                item.AppendChild(guid);
                guid.InnerText = i.Guid.ToString();

                XmlNode name = item.OwnerDocument.CreateNode(XmlNodeType.Element, "name", item.BaseURI);
                item.AppendChild(name);
                name.InnerText = i.Name;

                XmlNode text = item.OwnerDocument.CreateNode(XmlNodeType.Element, "text", item.BaseURI);
                item.AppendChild(text);
                text.InnerText = i.Text;
            }

            try { xml.Save(Path.Combine(Server.DataPath, "customnames.xml")); }
            catch { }
        }
    }
}
