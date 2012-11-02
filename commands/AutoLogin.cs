﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Net;
using iconnect;

namespace commands
{
    class AutoLogin
    {
        private class Item
        {
            public Guid Guid { get; set; }
            public String Name { get; set; }
            public ILevel Level { get; set; }
            public IPAddress IP { get; set; }
        }

        private static List<Item> list { get; set; }

        public static void Add(IUser client, ILevel level)
        {
            // guid and ip range
            byte[] client_bytes = client.ExternalIP.GetAddressBytes();

            Item i = list.Find(x =>
            {
                if (x.Guid.Equals(client.Guid))
                {
                    byte[] ip_bytes = x.IP.GetAddressBytes();

                    if (client_bytes[0] == ip_bytes[0])
                        if (client_bytes[1] == ip_bytes[1])
                            return true;
                }

                return false;
            });

            if (i != null)
            {
                i.IP = client.ExternalIP;
                i.Level = level;
                Save();
                return;
            }

            // ip address
            i = list.Find(x => x.IP.Equals(client.ExternalIP));

            if (i != null)
            {
                i.Guid = client.Guid;
                i.Level = level;
                Save();
                return;
            }

            // new
            list.Add(new Item
            {
                Guid = client.Guid,
                IP = client.ExternalIP,
                Level = level,
                Name = client.Name
            });

            Save();
        }

        public static String Remove(int id)
        {
            if (id >= 0 && id < list.Count)
            {
                Item i = list[id];
                list.RemoveAt(id);
                Save();

                Server.Users.Ares(x =>
                {
                    if (x.Guid.Equals(i.Guid) || x.ExternalIP.Equals(i.IP))
                        x.SetLevel(ILevel.Regular);
                });

                Server.Users.Web(x =>
                {
                    if (x.Guid.Equals(i.Guid) || x.ExternalIP.Equals(i.IP))
                        x.SetLevel(ILevel.Regular);
                });

                return i.Name;
            }

            return null;
        }

        public static void ListAdmins(IUser client)
        {
            for (int i = 0; i < list.Count; i++)
                client.Print(i + " - " + list[i].Name + " [" + list[i].IP + "] [" + list[i].Level + "]");
        }

        public static ILevel GetLevel(IUser client)
        {
            // guid and ip range
            byte[] client_bytes = client.ExternalIP.GetAddressBytes();

            Item i = list.Find(x =>
            {
                if (x.Guid.Equals(client.Guid))
                {
                    byte[] ip_bytes = x.IP.GetAddressBytes();

                    if (client_bytes[0] == ip_bytes[0])
                        if (client_bytes[1] == ip_bytes[1])
                            return true;
                }

                return false;
            });

            if (i != null)
            {
                i.IP = client.ExternalIP;
                Save();
                return i.Level;
            }

            // ip address
            i = list.Find(x => x.IP.Equals(client.ExternalIP));

            if (i != null)
            {
                i.Guid = client.Guid;
                Save();
                return i.Level;
            }

            return ILevel.Regular;
        }

        public static void Load()
        {
            list = new List<Item>();

            try
            {
                XmlDocument xml = new XmlDocument();
                xml.PreserveWhitespace = true;
                xml.Load(Path.Combine(Server.DataPath, "autologins.xml"));

                XmlNodeList nodes = xml.GetElementsByTagName("item");

                foreach (XmlElement e in nodes)
                    list.Add(new Item
                    {
                        Guid = new Guid(e.GetElementsByTagName("guid")[0].InnerText),
                        Name = e.GetElementsByTagName("name")[0].InnerText,
                        Level = (ILevel)byte.Parse(e.GetElementsByTagName("level")[0].InnerText),
                        IP = IPAddress.Parse(e.GetElementsByTagName("ip")[0].InnerText)
                    });
            }
            catch { }
        }

        private static void Save()
        {
            XmlDocument xml = new XmlDocument();
            xml.PreserveWhitespace = true;
            xml.AppendChild(xml.CreateXmlDeclaration("1.0", null, null));
            XmlNode root = xml.AppendChild(xml.CreateElement("autologins"));

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

                XmlNode level = item.OwnerDocument.CreateNode(XmlNodeType.Element, "level", item.BaseURI);
                item.AppendChild(level);
                level.InnerText = ((byte)i.Level).ToString();

                XmlNode ip = item.OwnerDocument.CreateNode(XmlNodeType.Element, "ip", item.BaseURI);
                item.AppendChild(ip);
                ip.InnerText = i.IP.ToString();
            }

            try { xml.Save(Path.Combine(Server.DataPath, "autologins.xml")); }
            catch { }
        }
    }
}
