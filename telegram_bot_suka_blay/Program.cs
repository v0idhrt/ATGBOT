using System.Reflection.Metadata;
using telegram_bot_suka_blay;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.Passport;
using Telegram.Bot.Types.ReplyMarkups;

DataBase db = new DataBase();
List<telegram_bot_suka_blay.User> usersQueue = new List<telegram_bot_suka_blay.User>();
Dictionary<long, string> state = new Dictionary<long, string>();

bool isExec = true;

using var cts = new CancellationTokenSource();
var buttons = new KeyboardButton[]
{
    "найти собеседника", "остановить диалог", "ebatnya"
};

var callbackButtonsAge = new InlineKeyboardButton[][]
{
    new[]
    {
        InlineKeyboardButton.WithCallbackData("Да", "Возраст_да"),
        InlineKeyboardButton.WithCallbackData("Нет", "Возраст_нет")
    }
};

var bot = new TelegramBotClient("");
bot.StartReceiving(OnUpdate, async (bot, ex, cts) => Console.WriteLine(ex));

var me = await bot.GetMeAsync();

Parallel.Invoke(
    () => { linkComrads(); },
    () => { stopExec(); }
);


async Task OnUpdate(ITelegramBotClient bot, Update update, CancellationToken ct)
{
    var msg = update.Message;
    if (msg is null)
    {
        return;
    }

    telegram_bot_suka_blay.User us = await db.getUser((long)msg.Chat.Id);
    if (us.Id == 0)
    {
        us.Id = (long)msg.Chat.Id;
        db.insertUser(us);
    }
    Console.WriteLine($"{us.Id}, {(long)msg.Chat.Id}");
    
    switch (msg.Text)
    {
        case "/start":
        {
            await bot.SendTextMessageAsync(us.Id, "Привет! Хочешь расскать что-нибудь о себе?", replyMarkup: 
                new InlineKeyboardMarkup(callbackButtonsAge));
            state[us.Id] = "AGE";
            break;
        }
        case "найти собеседника":
        {
            await db.getUser(us.Id);
            if (containsById(us) == false)
            {
                usersQueue.Add(us);
                await bot.SendTextMessageAsync(us.Id, "Вы начали поиск собеседника");
            }

            break;
        }
        case "остановить диалог":
        {
            if (us.ComradeId == 0 && containsById(us))
            {
                removeById(us);
                await bot.SendTextMessageAsync(us.Id, "Вы остнановили чат");
            }
            else if (us.ComradeId != 0)
            {
                telegram_bot_suka_blay.User comrade = await db.getUser(us.ComradeId);
                us.ComradeId = 0;
                comrade.ComradeId = 0;
                db.updateUser(us);
                db.updateUser(comrade);
                await bot.SendTextMessageAsync(us.Id, $"Вы остнановили чат",
                    replyMarkup: new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true });
                await bot.SendTextMessageAsync(comrade.Id, $"Ваш собеседник остановил чат",
                    replyMarkup: new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true });
                //     await bot.SendTextMessageAsync(us.Id, "Жмакает блять жмакает...");
            }

            break;
        }
        case "ebatnya":
        {
            db.deleteUser(us);
            break;
        }
        default:
        {
            Console.WriteLine(msg.Type);
            Console.WriteLine($"Received message '{msg.Text}' in {msg.Chat}");
            Console.WriteLine($"{us.Id}");
            if (us.ComradeId == 0)
            {
                
            }
            else
            {
                switch (msg.Type)
                {
                    case MessageType.Voice:
                        await bot.SendVoiceAsync(us.ComradeId, InputFile.FromFileId(msg.Voice.FileId));
                        break;
                    case MessageType.Text:
                        await bot.SendTextMessageAsync(us.ComradeId, $"{msg.Text}");
                        break;
                    case MessageType.Photo:
                        await bot.SendPhotoAsync(us.ComradeId, InputFile.FromFileId(msg.Photo[2].FileId));
                        break;
                    case MessageType.Sticker:
                        await bot.SendStickerAsync(us.ComradeId, InputFile.FromFileId(msg.Sticker.FileId));
                        break;
                }
                Console.WriteLine($"Sent to {us.ComradeId}");
            }

            break;
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
            await bot.SendTextMessageAsync(usersQueue[0].Id, "Собеседник найден!");
            await bot.SendTextMessageAsync(usersQueue[1].Id, "Собеседник найден!");
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