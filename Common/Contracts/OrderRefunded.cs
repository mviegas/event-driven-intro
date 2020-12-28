namespace Common.Contracts
{
    public class OrderRefusedDueOutOfStock : Message, IEvent
    {
        public const string Topic = "order_refused_due_out_of_stock";
        
        public override string MessageType => Topic;
    }
    
    public class OrderRefunded : Message, IEvent
    {
        public const string Topic = "order_refunded";
        
        public override string MessageType => Topic;
    }
}