using System.Threading.Tasks;
using Common.Contracts;
using DotNetCore.CAP;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Shipments.EventHandlers
{
    internal interface IShipmentHandlers
    {
        Task ShipOrder(JToken obj);
    }

    internal sealed class ShipmentHandlers : IShipmentHandlers, ICapSubscribe
    {
        private readonly ILogger<ShipmentHandlers> _logger;
        private readonly ICapPublisher _bus;


        public ShipmentHandlers(
            ILogger<ShipmentHandlers> logger,
            ICapPublisher bus)
        {
            _logger = logger;
            _bus = bus;
        }

        [CapSubscribe(Common.Contracts.ShipOrder.Topic)]
        public async Task ShipOrder(JToken obj)
        {
            var message = obj.ToObject(typeof(ShipOrder)) as ShipOrder;
            
            var command = new OrderShipped()
            {
                EntityId = message.EntityId,
                CorrelationId = message.CorrelationId
            };

            await _bus.PublishCorrelatedAsync(command);
        }
    }
}