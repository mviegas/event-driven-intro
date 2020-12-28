using System;

namespace Common.Contracts
{
    public class OrderBilled : Message, IEvent
    {
        public const string Topic = "order_billed";
        
        public override string MessageType => Topic;
    }
}