using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace MassTransitTests
{
    public class EventConsumer : IConsumer<ValueEntered>
    {
        private readonly ILogger<EventConsumer> _logger;

        public EventConsumer(ILogger<EventConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ValueEntered> context)
        {
            _logger.LogInformation("Value: {Value}", context.Message.Value);

            await context.Publish<ValueEntered2>(new
            {
                Value = $"Prev value: {context.Message.Value}"
            });
        }
    }
}
