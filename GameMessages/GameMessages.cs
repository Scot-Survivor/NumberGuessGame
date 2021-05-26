using System;

namespace GameMessages
{
    public class ClientMessage
    {
        public int ClientId {get; set; }
        public int GameId {get; set;}
        public string Command { get; set; }
        public DateTimeOffset TimeSent { get; set; }
        public string SourceAddr { get; set; }
        public string DestinationAddr { get; set; }
    }
}