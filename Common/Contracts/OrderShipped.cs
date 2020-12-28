namespace Common.Contracts
{
    public class OrderShipped : Message, IEvent
    {
        public const string Topic = "order_shipped";
        
        public override string MessageType => Topic;
    }
}