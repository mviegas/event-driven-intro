namespace Common.Contracts
{
    public class OrderDelivered : Message, IEvent
    {
        public const string Topic = "order_delivered";
        
        public override string MessageType => Topic;
    }
}