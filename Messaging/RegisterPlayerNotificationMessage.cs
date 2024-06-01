using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualRemotePresenceProtocol.Messaging
{
    class RegisterPlayerNotificationMessage
    {
        public string type = "notify-register-player";
        public VrppPlayer player { get; set; }
    }
}
