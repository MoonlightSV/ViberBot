using Viber.Bot;
using Newtonsoft.Json;

namespace ViberBot
{
    public class Program
    {
        private const string BOT_TOKEN = "TOKEN";
        private static ViberBotClient viberBotClient = new ViberBotClient(BOT_TOKEN);
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var app = builder.Build();

            app.MapPost($"/{BOT_TOKEN}", async (HttpRequest request) =>
            {
                var body = new StreamReader(request.Body).ReadToEndAsync();

                var isSignatureValid = viberBotClient.ValidateWebhookHash(
                    request.Headers[ViberBotClient.XViberContentSignatureHeader],
                    body.Result);

                if (!isSignatureValid)
                {
                    throw new Exception("Invalid viber content signature");
                }

                var callbackData = JsonConvert.DeserializeObject<CallbackData>(body.Result);

                if (callbackData.Event is EventType.Message)
                {
                    var message = (TextMessage)callbackData.Message; 
                    var result = await viberBotClient.SendTextMessageAsync(new TextMessage
                    {
                        Receiver = callbackData.Sender.Id,
                        Sender = new UserBase
                        {
                            Name = "Bot Name"
                        },
                        Text = message.Text
                    });
                }

                return Results.Ok();
            });

            app.Run();
        }
    }
}