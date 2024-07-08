using System.Reflection.Metadata;
using telegram_bot_suka_blay;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;


using var cts = new CancellationTokenSource();
var buttons = new KeyboardButton[]
{
    "Xuy", "Pizda",
};

var bot = new TelegramBotClient("7401680833:AAFW5okF7m1oSwKhROvqk2OusoF4uUn9AlM");
bot.StartReceiving(OnUpdate, async (bot, ex, cts) => Console.WriteLine(ex));

var me = await bot.GetMeAsync();


Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");
Console.ReadLine();
cts.Cancel();

async Task OnUpdate(ITelegramBotClient bot, Update update, CancellationToken ct)
{

    if (update.Message is null) return;			
    if (update.Message.Text is null) return;	
    var msg = update.Message;
    Console.WriteLine($"Received message '{msg.Text}' in {msg.Chat}");

    await bot.SendTextMessageAsync(msg.Chat, $"{msg.From} said: {msg.Text}", replyMarkup: new ReplyKeyboardMarkup(buttons) {ResizeKeyboard = true});
}