using System.Diagnostics;
using System.Threading.Tasks;
using Common.Contracts;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orders.Domain;
using Orders.Infra;
using Orders.Orchestrator;

namespace Orders.Controllers
{
    [ApiController]
    [Route("v1")]
    public class OrdersController : ControllerBase
    {
        private readonly ICapPublisher _bus;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(ICapPublisher bus, ILogger<OrdersController> logger)
        {
            _bus = bus;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> PlaceOrder([FromServices] OrdersContext context)
        {
            await using (context.Database.BeginTransaction(_bus, true))
            {
                var order = new Order();

                await context.Orders.AddAsync(order);
                await context.SaveChangesAsync();

                var message = new OrderPlaced()
                {
                    EntityId = order.Id,
                    CorrelationId = Activity.Current.Id
                };

                _logger.LogInformation("Order #{OrderId} was {Status}", message.EntityId, "Placed");

                await _bus.PublishCorrelatedAsync(message);
            }

            return Ok();
        }
    }
}