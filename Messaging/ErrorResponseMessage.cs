using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualRemotePresenceProtocol.Messaging
{
    class ErrorResponseMessage
    {
        public string type = "error-response";
        public string message { get; set; }
    }
}
