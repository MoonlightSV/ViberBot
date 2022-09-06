using Microsoft.AspNetCore.Server.Kestrel.Core;
using Viber.Bot;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ViberBot
{
    public class Program
    {
        private static ViberBotClient viberBotClient = new ViberBotClient("TOKEN");
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            var app = builder.Build();

            app.MapPost("/", async (HttpContext context) =>
            {
                var body = new StreamReader(context.Request.Body).ReadToEnd();
                var isSignatureValid = viberBotClient.ValidateWebhookHash(
                    context.Request.Headers[ViberBotClient.XViberContentSignatureHeader],
                    body);
                if (!isSignatureValid)
                {
                    throw new Exception("Invalid viber content signature");
                }

                var callbackData = JsonConvert.DeserializeObject<CallbackData>(body);

                if (callbackData.Event is EventType.Message)
                {
                    var text = JObject.Parse(body)["message"]["text"];
                    var result = await viberBotClient.SendTextMessageAsync(new TextMessage
                    {
                        Receiver = callbackData.Sender.Id,
                        Sender = new UserBase
                        {
                            Name = "BotName"
                        },
                        Text = text.ToString()
                    });
                }
                return Results.Ok();
            });

            app.Run();
        }
    }
}