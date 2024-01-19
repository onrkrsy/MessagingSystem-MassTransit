using Core;
using MassTransit;

namespace Consumer2.Infrastructer
{
    public class TestMessageConsumer : IConsumer<TestMessage>
    {

        public async Task Consume(ConsumeContext<TestMessage> context)
        {
            var messageText = context.Message.Text; 

            await context.RespondAsync(new TestMessageResponse { ResponseText = "Queue 2 -> Received: " + messageText });
        }
    }
}
