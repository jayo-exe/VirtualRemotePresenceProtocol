using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualRemotePresenceProtocol.Messaging
{
    class SessionCloseResponseMessage
    {
        public string type = "session-close-response";
        public string response = "ok";
    }
}
