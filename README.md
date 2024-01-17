# MassTransit ile RabbitMQ Kullanarak Mesajlaşma Örneği

Bu proje, MassTransit ve RabbitMQ kullanarak basit bir mesajlaşma sistemi örneğidir. Sistem, bir "Requester" Web API'si ve iki "Consumer" Web API'sinden oluşmaktadır. Requester, kullanıcıdan gelen mesajları RabbitMQ üzerinden Consumer'lara gönderir ve Consumer'lar bu mesajları işleyip bir yanıt döndürür.

## Bileşenler

### Requester Web API

Requester, RabbitMQ ile etkileşime girebilmek için MassTransit ve RabbitMQ yapılandırmasını içerir.

```csharp
builder.Services.AddMassTransit(x =>
{
    x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
    {
        cfg.Host(new Uri("rabbitmq://localhost"), h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
    }));

    // TestMessage için IRequestClient yapılandırması
    x.AddRequestClient<TestMessage>(new Uri("queue:test_queue"));
    x.AddMassTransitHostedService();
});
```
Consumer Web API'ler
Her bir Consumer, farklı kuyrukları dinleyecek şekilde yapılandırılmıştır.

```csharp
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
