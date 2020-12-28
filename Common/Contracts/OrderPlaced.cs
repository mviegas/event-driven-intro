using System;

namespace Common.Contracts
{
    public class OrderPlaced : Message, IEvent
    {
        public const string Topic = "order_placed";
        
        public override string MessageType => Topic;
    }
}