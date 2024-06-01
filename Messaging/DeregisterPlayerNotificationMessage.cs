using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualRemotePresenceProtocol.Messaging
{
    class DeregisterPlayerNotificationMessage
    {
        public string type = "notify-deregister-player";
        public string PlayerUuid { get; set; }
    }
}
