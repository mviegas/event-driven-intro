using System;

namespace Common.Contracts
{
    public class BillOrder : Message, ICommand
    {
        public const string Topic = "order_bill";
        public override string MessageType => Topic;
    }
}