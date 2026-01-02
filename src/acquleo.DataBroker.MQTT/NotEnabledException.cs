using System;

namespace acquleo.DataBroker.MQTT
{
    public class NotEnabledException : Exception
    {
        public NotEnabledException(string message) : base(message)
        {

        }
    }
}
