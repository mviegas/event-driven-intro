namespace Common.Contracts
{
    public class RefundOrder : Message, ICommand
    {
        public const string Topic = "order_refund";
        
        public override string MessageType => Topic;
    }
}