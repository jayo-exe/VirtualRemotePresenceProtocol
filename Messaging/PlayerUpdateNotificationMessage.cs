using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualRemotePresenceProtocol.Messaging
{
    class PlayerUpdateNotificationMessage
    {
        public string type = "notify-player-update";
        public VrppPlayer player { get; set; }
    }
}
