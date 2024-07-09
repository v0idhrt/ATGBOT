using System.Reflection.Metadata;
using telegram_bot_suka_blay;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

DataBase db = new DataBase();
List<telegram_bot_suka_blay.User> usersQueue = new List<telegram_bot_suka_blay.User>();
bool isExec = true;

using var cts = new CancellationTokenSource();
var buttons = new KeyboardButton[]
{
    "xui", "pizda", "ebatnya"
};

var bot = new TelegramBotClient("TOKEN");
bot.StartReceiving(OnUpdate, async (bot, ex, cts) => Console.WriteLine(ex));

var me = await bot.GetMeAsync();

Parallel.Invoke(
    () => { linkComrads(); }, 
    () => { stopExec(); }
    );


async Task OnUpdate(ITelegramBotClient bot, Update update, CancellationToken ct)
{
    var msg = update.Message;
    if (msg is null) return;
    if (msg.Text is null) return;

    telegram_bot_suka_blay.User us = await db.getUser((long)msg.Chat.Id);
    if (us.Id == 0)
    {
        us.Id = (long)msg.Chat.Id;
        db.insertUser(us);
    }
    Console.WriteLine($"{us.Id}, {(long)msg.Chat.Id}");
    if (msg.Text == "/start")
    {

    }
    else if (msg.Text == "xui") // start chating
    {
        if (containsById(us) == false)
        {
            usersQueue.Add(us);
            await bot.SendTextMessageAsync(us.Id, "You started searching...");
        }
    }
    else if (msg.Text == "pizda") // stop chating
    {
        if (us.ComradeId == 0 && containsById(us))
        {
            removeById(us);
            await bot.SendTextMessageAsync(us.Id, "You stopped searching");
        }
        else if (us.ComradeId != 0)
        {
            telegram_bot_suka_blay.User comrade = await db.getUser(us.ComradeId);
            us.ComradeId = 0;
            comrade.ComradeId = 0;
            db.updateUser(us);
            db.updateUser(comrade);
            await bot.SendTextMessageAsync(us.Id, $"You stopped dialog", replyMarkup: new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true });
            await bot.SendTextMessageAsync(comrade.Id, $"Your comrade stopped dialog", replyMarkup: new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true });
        }
        else
        {
            await bot.SendTextMessageAsync(us.Id, "Жмакает блять жмакает...");
        }
    }
    else if (msg.Text == "ebatnya")
    {
        db.deleteUser(us);
    }
    else
    {
        Console.WriteLine($"Received message '{msg.Text}' in {msg.Chat}");
        Console.WriteLine($"{us.Id}");
        if (us.ComradeId == 0)
        {
            await bot.SendTextMessageAsync(us.Id, $"Хуесос нормально ботом пользуйся и не пиши хуйню", replyMarkup: new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true });
        }
        else
        {
            await bot.SendTextMessageAsync(us.ComradeId, $"{msg.From} said: {msg.Text}", replyMarkup: new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true });
            Console.WriteLine($"Sent to {us.ComradeId}");
        }
    }
}

void stopExec()
{
    Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");
    Console.ReadLine();
    isExec = false;
    cts.Cancel();
}

async void linkComrads()
{
    while (isExec)
    {
        while (usersQueue.Count >= 2)
        { 
            usersQueue[0].ComradeId = usersQueue[1].Id;
            usersQueue[1].ComradeId = usersQueue[0].Id;
            db.updateUser(usersQueue[0]);
            db.updateUser(usersQueue[1]);
            await bot.SendTextMessageAsync(usersQueue[0].Id, "Comrade found!");
            await bot.SendTextMessageAsync(usersQueue[1].Id, "Comrade found!");
            removeById(usersQueue[0]);
            removeById(usersQueue[0]);
        }
        Thread.Sleep(1000);
    }
}


bool containsById(telegram_bot_suka_blay.User us)
{
    foreach (telegram_bot_suka_blay.User u in usersQueue)
    {
        if (u.Id == us.Id)
        {
            return true;
        }
    }
    return false;
}

void removeById(telegram_bot_suka_blay.User us)
{
    for (int i = 0; i < usersQueue.Count; i++)
    {
        if (us.Id == usersQueue[i].Id)
        {
            usersQueue.RemoveAt(i);
        }
    }
}