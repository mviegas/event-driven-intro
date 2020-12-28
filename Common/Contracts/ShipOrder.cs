namespace Common.Contracts
{
    public class ShipOrder : Message, ICommand
    {
        public const string Topic = "order_ship";
        
        public override string MessageType => Topic;
    }
}