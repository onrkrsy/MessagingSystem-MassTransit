# MassTransit ile RabbitMQ Kullanarak Mesajlaşma Örneği

Bu proje, MassTransit ve RabbitMQ kullanarak basit bir mesajlaşma sistemi örneğidir. Sistem, bir "Requester" Web API'si ve iki "Consumer" Web API'sinden oluşmaktadır. Requester, kullanıcıdan gelen mesajları RabbitMQ üzerinden Consumer'lara gönderir ve Consumer'lar bu mesajları işleyip bir yanıt döndürür.

## Bileşenler

### Requester Web API

Requester, RabbitMQ ile etkileşime girebilmek için MassTransit ve RabbitMQ yapılandırmasını içerir.

```csharp

    public async Task<IActionResult> SendMessage([FromBody] TestMessage message)
    {  
        var (response, _) = await _requestClient.GetResponse<TestMessageResponse>(message);
        return Ok(response.Message);
    }
    
    public async Task<IActionResult> SendDynamicQueue([FromBody] TestMessage message)
    {
        // Gelen mesaja göre kuyruk adını belirle
        var queueName = message.QueueName;
    
        // Yanıt beklemek için bir RequestClient oluştur
        var requestClient = _bus.CreateRequestClient<TestMessage>(new Uri($"queue:{queueName}"));
    
        // Mesajı gönder ve yanıtı bekle
        var (response, _) = await requestClient.GetResponse<TestMessageResponse>(message);
    
        return Ok(response.Message);
    
    }
```
Consumer Web API'ler
Her bir Consumer, farklı kuyrukları dinleyecek şekilde yapılandırılmıştır.

```csharp
 public async Task Consume(ConsumeContext<TestMessage> context)
        {
            var messageText = context.Message.Text; 

            await context.RespondAsync(new TestMessageResponse { ResponseText = "Queue 2 -> Received: " + messageText });
        }

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<TestMessageConsumer>();

    x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
    {
        cfg.Host(new Uri("rabbitmq://localhost"), h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("test_queue", e =>
        {
            e.ConfigureConsumer<TestMessageConsumer>(provider);
        });
    }));
});

builder.Services.AddMassTransitHostedService();
```
## Kullanım
### Mesaj Gönderimi
Requester Web API'sindeki TestController içinde iki endpoint bulunur:
1. SendMessage: Sabit bir kuyruğa (test_queue) mesaj gönderir.
1. SendDynamicQueue: Gelen mesaja göre dinamik olarak kuyruk seçer ve mesajı o kuyruğa gönderir.
### Mesaj İşleme
Her bir Consumer, RabbitMQ'dan mesajları alır ve basit bir işlem gerçekleştirir. İşlenen mesajlara yanıt olarak, bir TestMessageResponse nesnesi döndürülür.
