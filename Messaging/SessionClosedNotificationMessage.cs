using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualRemotePresenceProtocol.Messaging
{
    class SessionClosedNotificationMessage
    {
        public string type = "notify-session-closed";
        public string Uuid { get; set; }
    }
}
