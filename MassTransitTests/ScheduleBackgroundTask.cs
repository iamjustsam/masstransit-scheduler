using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Scheduling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MassTransitTests
{
    public class ScheduleBackgroundTask : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private ISendEndpoint _sendEndpoint;
        private ScheduledRecurringMessage<ValueEntered> _schedule;

        public ScheduleBackgroundTask(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var schedulerAddress = new Uri("queue:quartz");
            var queueAddress = new Uri("queue:event-listener");

            var sendEndpointProvider = scope.ServiceProvider.GetRequiredService<ISendEndpointProvider>();
            _sendEndpoint = await sendEndpointProvider.GetSendEndpoint(schedulerAddress);

            _schedule = await _sendEndpoint.ScheduleRecurringSend<ValueEntered>(queueAddress, new EventSchedule(), new
            {
                Value = "Scheduled message"
            }, cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_schedule == null)
                return;

            await _sendEndpoint.CancelScheduledRecurringSend(_schedule);

            await base.StopAsync(cancellationToken);
        }
    }

    public class EventSchedule : DefaultRecurringSchedule
    {
        public EventSchedule()
        {
            CronExpression = "0 0/1 * 1/1 * ? *"; // this means every minute
        }
    }
}
