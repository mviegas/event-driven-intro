using System;

namespace Common.Contracts
{
    public abstract class Message
    {
        public Message()
        {
            Timestamp = DateTime.UtcNow;
            MessageId = Guid.NewGuid();
        }

        public Guid MessageId { get; private set; }
        public Guid EntityId { get; set; }
        public abstract string MessageType { get; }
        public DateTime Timestamp { get; private set; }
        public string CorrelationId { get; set; }
    }
}