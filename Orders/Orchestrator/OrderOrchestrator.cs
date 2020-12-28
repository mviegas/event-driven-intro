using System;
using System.Threading.Tasks;
using Common.Contracts;
using DotNetCore.CAP;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Orders.Domain;
using Orders.Infra;

namespace Orders.Orchestrator
{
    internal interface IOrchestrator
    {
        Task BillOrder(JToken obj);
        Task ConfirmOrderBilledAndShipOrder(JToken obj);
        Task ConfirmOrderOutOfStockAndAskForRefund(JToken obj);
        Task ConfirmOrderRefunded(JToken obj);
        Task ConfirmOrderShipped(JToken obj);
        Task ConfirmOrderDelivered(JToken obj);
    }

    internal sealed class Orchestrator : IOrchestrator, ICapSubscribe
    {
        private readonly OrdersContext _context;
        private readonly ILogger<Orchestrator> _logger;
        private readonly ICapPublisher _bus;


        public Orchestrator(
            OrdersContext context,
            ILogger<Orchestrator> logger,
            ICapPublisher bus)
        {
            _context = context;
            _logger = logger;
            _bus = bus;
        }

        [CapSubscribe(OrderPlaced.Topic)]
        public async Task BillOrder(JToken obj)
        {
            var message = obj.ToObject(typeof(OrderPlaced)) as OrderPlaced;
            
            var command = new BillOrder()
            {
                EntityId = message.EntityId,
                CorrelationId = message.CorrelationId
            };
            
            _logger.LogInformation("Order #{OrderId} is awaiting payment", message.EntityId);

            await _bus.PublishCorrelatedAsync(command);
        }
        
        [CapSubscribe(OrderBilled.Topic)]
        public async Task ConfirmOrderBilledAndShipOrder(JToken obj)
        {
            await ProcessCurrentAndDispatchNext<OrderBilled, ShipOrder>(obj,
                async (message) => await UpdateOrderStatus(message.EntityId, EOrderStatus.Billed));
        }

        [CapSubscribe(OrderRefusedDueOutOfStock.Topic)]
        public async Task ConfirmOrderOutOfStockAndAskForRefund(JToken obj)
        {
            await ProcessCurrentAndDispatchNext<OrderRefusedDueOutOfStock, RefundOrder>(obj,
                async (message) => await UpdateOrderStatus(message.EntityId, EOrderStatus.OutOfStock));
        }

        [CapSubscribe(OrderRefunded.Topic)]
        public async Task ConfirmOrderRefunded(JToken obj)
        {
            await ProcessCurrent<OrderRefunded>(obj,
                async (message) => await UpdateOrderStatus(message.EntityId, EOrderStatus.Refunded));
        }

        [CapSubscribe(OrderShipped.Topic)]
        public async Task ConfirmOrderShipped(JToken obj)
        {
            await ProcessCurrent<OrderShipped>(obj,
                async (message) => await UpdateOrderStatus(message.EntityId, EOrderStatus.Shipped));
        }

        [CapSubscribe(OrderDelivered.Topic)]
        public async Task ConfirmOrderDelivered(JToken obj)
        {
            await ProcessCurrent<OrderDelivered>(obj,
                async (message) => await UpdateOrderStatus(message.EntityId, EOrderStatus.Delivered));
        }

        private async Task ProcessCurrentAndDispatchNext<TCurrent, TNext>(JToken obj, Func<Message, Task> action)
            where TCurrent : Message
            where TNext : Message
        {
            await using var transaction = _context.Database.BeginTransaction(_bus, true);

            var message = await ProcessCurrent<TCurrent>(obj, action);

            var nextMessage = Activator.CreateInstance<TNext>() as Message;
            nextMessage.EntityId = message.EntityId;
            nextMessage.CorrelationId = message.CorrelationId;

            await _bus.PublishCorrelatedAsync(nextMessage);
        }

        private async Task<Message> ProcessCurrent<TCurrent>(JToken obj, Func<Message, Task> action)
            where TCurrent : Message
        {
            var message = obj.ToObject(typeof(TCurrent)) as TCurrent;

            if (message is null) throw new ArgumentNullException(nameof(message));

            await action(message);

            return message;
        }

        private async Task UpdateOrderStatus(Guid orderId, EOrderStatus status)
        {
            var order = await _context.Orders.FindAsync(orderId);

            order.Status = status;

            _context.Orders.Update(order);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Order #{OrderId} was {Status}", orderId, status.ToString());
        }
    }
}