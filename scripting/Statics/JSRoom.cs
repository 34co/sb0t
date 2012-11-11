﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jurassic;
using Jurassic.Library;

namespace scripting.Statics
{
    [JSEmbed(Name = "Room")]
    class JSRoom : ObjectInstance
    {
        public JSRoom(ScriptEngine engine)
            : base(engine)
        {
            this.PopulateFunctions();
        }

        [JSProperty(Name = "version")]
        public static int Version
        {
            get { return Server.SCRIPT_VERSION; }
            set { }
        }

        [JSProperty(Name = "botName")]
        public static String BotName
        {
            get { return Server.Chatroom.BotName; }
            set { }
        }

        [JSProperty(Name = "customNames")]
        public static bool CustomNames
        {
            get { return Server.Chatroom.CustomNamesEnabled; }
            set { Server.Chatroom.CustomNamesEnabled = value; }
        }

        [JSProperty(Name = "externalIp")]
        public static String ExternalIP
        {
            get { return Server.Chatroom.ExternalIP.ToString(); }
            set { }
        }

        [JSProperty(Name = "hashlink")]
        public static String Hashlink
        {
            get { return "arlnk://" + Server.Chatroom.Hashlink; }
            set { }
        }

        [JSProperty(Name = "name")]
        public static String Name
        {
            get { return Server.Chatroom.Name; }
            set { }
        }

        [JSProperty(Name = "port")]
        public static int Port
        {
            get { return Server.Chatroom.Port; }
            set { }
        }

        [JSProperty(Name = "startTime")]
        public static double StartTime
        {
            get { return Server.Chatroom.StartTime; }
            set { }
        }

        [JSProperty(Name = "topic")]
        public static String Topic
        {
            get { return Server.Chatroom.Topic; }
            set { Server.Chatroom.Topic = value; }
        }

        [JSFunction(Name = "setUrl", IsWritable = false, IsEnumerable = true)]
        public static void SetUrl(object a, object b)
        {
            if (a == null || b == null)
                return;

            String addr = String.Empty;
            String text = String.Empty;

            if (!(a is Undefined || a is Null))
                addr = a.ToString();

            if (!(b is Undefined || b is Null))
                text = b.ToString();

            if (String.IsNullOrEmpty(addr) && String.IsNullOrEmpty(text))
                Server.Chatroom.ClearURL();
            else if (!String.IsNullOrEmpty(addr))
                if (!String.IsNullOrEmpty(text))
                    Server.Chatroom.UpdateURL(addr, text);
        }

        [JSFunction(Name = "clearUrl", IsWritable = false, IsEnumerable = true)]
        public static void ClearUrl()
        {
            Server.Chatroom.ClearURL();
        }
    }
}
