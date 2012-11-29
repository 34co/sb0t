﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using iconnect;

namespace commands
{
    class Urls
    {
        private static uint last = 0;
        private static int position = 0;

        private class Item
        {
            public String Address { get; set; }
            public String Text { get; set; }
        }

        private static List<Item> list { get; set; }

        private static String l_addr = String.Empty;
        private static String l_text = String.Empty;

        public static void Tick(uint time)
        {
            if (time > (last + 15))
            {
                last = time;

                if (list.Count > 0)
                    if (Settings.Url)
                    {
                        if (++position >= list.Count)
                            position = 0;

                        if (list[position].Address == l_addr && list[position].Text == l_text)
                            return;

                        Server.Chatroom.UpdateURL(list[position].Address, list[position].Text);
                        l_addr = list[position].Address;
                        l_text = list[position].Text;
                    }
            }
        }

        public static void EnableDisable(bool enable)
        {
            if (enable)
            {
                l_addr = String.Empty;
                l_text = String.Empty;
                last = 16;
                Tick(Server.Time);
            }
        }

        public static void Add(String addr, String text)
        {
            list.Add(new Item
            {
                Address = addr,
                Text = text
            });

            Save();
            last = 16;
            Tick(Server.Time);
        }

        public static bool Remove(int index)
        {
            if (index >= 0 && index < list.Count)
            {
                list.RemoveAt(index);
                Save();
                position = -1;
                Server.Chatroom.ClearURL();
                last = 16;
                Tick(Server.Time);
                return true;
            }

            return false;
        }

        public static void List(IUser client)
        {
            if (list.Count == 0)
                client.Print(Template.Text(Category.Urls, 0));

            for (int i = 0; i < list.Count; i++)
                client.Print(i + " - " + list[i].Text + " [" + list[i].Address + "]");
        }

        public static void Load()
        {
            list = new List<Item>();
            l_addr = String.Empty;
            l_text = String.Empty;

            try
            {
                XmlDocument xml = new XmlDocument();
                xml.PreserveWhitespace = true;
                xml.Load(Path.Combine(Server.DataPath, "urls.xml"));

                XmlNodeList nodes = xml.GetElementsByTagName("item");

                foreach (XmlElement e in nodes)
                    list.Add(new Item
                    {
                        Address = e.GetElementsByTagName("address")[0].InnerText,
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
            XmlNode root = xml.AppendChild(xml.CreateElement("urls"));

            foreach (Item i in list)
            {
                XmlNode item = root.OwnerDocument.CreateNode(XmlNodeType.Element, "item", root.BaseURI);
                root.AppendChild(item);

                XmlNode address = item.OwnerDocument.CreateNode(XmlNodeType.Element, "address", item.BaseURI);
                item.AppendChild(address);
                address.InnerText = i.Address;

                XmlNode text = item.OwnerDocument.CreateNode(XmlNodeType.Element, "text", item.BaseURI);
                item.AppendChild(text);
                text.InnerText = i.Text;
            }

            try { xml.Save(Path.Combine(Server.DataPath, "urls.xml")); }
            catch { }
        }
    }
}
