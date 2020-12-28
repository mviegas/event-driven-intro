using System.Threading.Tasks;
using Common.Contracts;
using DotNetCore.CAP;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Payments.EventHandlers
{
    internal interface IPaymentHandlers
    {
        Task BillOrder(JToken obj);
        Task RefundOrder(JToken obj);
    }

    internal sealed class PaymentHandlers : IPaymentHandlers, ICapSubscribe
    {
        private readonly ILogger<PaymentHandlers> _logger;
        private readonly ICapPublisher _bus;


        public PaymentHandlers(
            ILogger<PaymentHandlers> logger,
            ICapPublisher bus)
        {
            _logger = logger;
            _bus = bus;
        }

        [CapSubscribe(Common.Contracts.BillOrder.Topic)]
        public async Task BillOrder(JToken obj)
        {
            var message = obj.ToObject(typeof(BillOrder)) as BillOrder;
            
            var command = new OrderBilled()
            {
                EntityId = message.EntityId,
                CorrelationId = message.CorrelationId
            };

            await _bus.PublishCorrelatedAsync(command);
        }
        
        [CapSubscribe(Common.Contracts.RefundOrder.Topic)]
        public async Task RefundOrder(JToken obj)
        {
            var message = obj.ToObject(typeof(RefundOrder)) as RefundOrder;
            
            var command = new OrderRefunded()
            {
                EntityId = message.EntityId,
                CorrelationId = message.CorrelationId
            };

            await _bus.PublishCorrelatedAsync(command);
        }
    }
}