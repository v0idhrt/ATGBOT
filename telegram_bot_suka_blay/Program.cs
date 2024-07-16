using System.Reflection.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
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
    "Найти собеседника", "Остановить диалог", "Профиль"
};

var startButtons = new KeyboardButton[]
{
    "Найти собеседника", "Профиль"
};

var callbackButtonsAge = new InlineKeyboardButton[][]
{
    new[]
    {
        InlineKeyboardButton.WithCallbackData("Да", "Возраст_да"),
        InlineKeyboardButton.WithCallbackData("Нет", "Возраст_нет")
    }
};

var findComradeKeyboard = new KeyboardButton[]
{
    "Остановить поиск", "Профиль"
};


var bot = new TelegramBotClient("TOKEN");
bot.StartReceiving(OnUpdate, async (bot, ex, cts) => Console.WriteLine(ex));

var me = await bot.GetMeAsync();

Parallel.Invoke(
    () => { LinkComrades(); },
    () => { StopExec(); }
);


async Task OnUpdate(ITelegramBotClient bot, Update update, CancellationToken ct)
{
    switch (update.Type)
    {
        case UpdateType.Message:
            await UpdateMessage(bot, update, ct);
            break;
        case UpdateType.CallbackQuery:
            await UpdateCallbackQuery(bot, update, ct);
            break;
        default:
            Console.WriteLine("Hui!");
            break;
    }
}
async Task UpdateCallbackQuery(ITelegramBotClient bot, Update update, CancellationToken ct)
{
    telegram_bot_suka_blay.User us = await db.GetUser((long)update.CallbackQuery.Message.Chat.Id);
    switch (state[us.Id])
    {
        case "STARTMENU":
            if (update.CallbackQuery.Data == "Возраст_да")
            {
                await bot.SendTextMessageAsync(us.Id, "Хорошо. Сколько тебе лет?");
                state[us.Id] = "AGE";
            }
            else if (update.CallbackQuery.Data == "Возраст_нет")
            {
                await bot.SendTextMessageAsync(us.Id, "Хорошо. Тогда мы начали поиск собеседника!");
                await bot.EditMessageReplyMarkupAsync(us.Id, update.CallbackQuery.Message.MessageId);
                state[us.Id] = "READYTOCHAT";
                usersQueue.Add(us);
                
            }
            break;
        default:
            break;
    }
    try
    {
        await bot.EditMessageReplyMarkupAsync(us.Id, update.CallbackQuery.Message.MessageId);
    }
    catch { }
}

async Task UpdateMessage(ITelegramBotClient bot, Update update, CancellationToken ct)
{
    var msg = update.Message;

    if (msg is null)
    {
        return;
    }

    telegram_bot_suka_blay.User us = await db.GetUser((long)msg.Chat.Id);
    if (us.Id == 0)
    {
        us.Id = (long)msg.Chat.Id;
        db.InsertUser(us);
    }
    Console.WriteLine($"{us.Id}, {(long)msg.Chat.Id}");

    switch (msg.Text)
    {
        case "/start":
        {
                await bot.SendStickerAsync(us.Id,
                InputFile.FromString("CAACAgIAAxkBAAEG84NmltOgcSQZnp8OgfT_lZtIUSbdoQACJhcAAgOz-UvLvlelPb0vtzUE"));
                await bot.SendTextMessageAsync(us.Id, "Привет!", replyMarkup:
                    new ReplyKeyboardMarkup(startButtons) {ResizeKeyboard = true});
                await bot.SendTextMessageAsync(us.Id, "Хочешь расскать что-нибудь о себе?!", replyMarkup: 
                    new InlineKeyboardMarkup(callbackButtonsAge));
                state[us.Id] = "STARTMENU";
                break;
            }
        case "Найти собеседника":
            {
                await db.GetUser(us.Id);
                if (ContainsById(us) == false)
                {
                    usersQueue.Add(us);
                    await bot.SendTextMessageAsync(us.Id, "Вы начали поиск собеседника", 
                        replyMarkup: new ReplyKeyboardMarkup(findComradeKeyboard) {ResizeKeyboard = true});
                }

                state[us.Id] = "FINDING";
                break;
            }
        case "Остановить поиск":
        {
            if (state[us.Id] == "FINDING" && ContainsById(us))
            {
                RemoveById(us);
                await bot.SendTextMessageAsync(us.Id, "Вы остановили поиск собеседника",
                    replyMarkup: new ReplyKeyboardMarkup(startButtons){ResizeKeyboard = true});
            }
            break;
        }
        case "Остановить диалог":
            {
                if (us.ComradeId == 0 && ContainsById(us))
                {
                    RemoveById(us);
                    await bot.SendTextMessageAsync(us.Id, "Вы остнановили чат");
                }
                else if (us.ComradeId != 0)
                {
                    telegram_bot_suka_blay.User comrade = await db.GetUser(us.ComradeId);
                    us.ComradeId = 0;
                    comrade.ComradeId = 0;
                    db.UpdateUser(us);
                    db.UpdateUser(comrade);
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
                db.DeleteUser(us);
                break;
            }
        default:
            {
                Console.WriteLine(msg.Type);
                Console.WriteLine($"Received message '{msg.Text}' in {msg.Chat}");
                Console.WriteLine($"{us.Id}");
                if (us.ComradeId == 0)
                {
                    switch (state[us.Id])
                    {
                        case "AGE":
                        {
                            int age;
                            Int32.TryParse(msg.Text, out age);
                            if (age is > 0 and < 100)
                            {
                                us.Age = age;
                                await bot.SendTextMessageAsync(us.Id, $"Услышали! Вам {age} лет");
                                state[us.Id] = "GENDER";
                            }
                            else
                            {
                                await bot.SendTextMessageAsync(us.Id, "Нам кажется, что вы ввели некорретный возраст.   \nДавайте попробуем ещё раз");
                                state[us.Id] = "AGE";
                            }
                            break;
                        }
                    }
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

void StopExec()
{
    Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");
    Console.ReadLine();
    isExec = false;
    cts.Cancel();
}

async void LinkComrades()
{
    while (isExec)
    {
        while (usersQueue.Count >= 2)
        {
            usersQueue[0].ComradeId = usersQueue[1].Id;
            usersQueue[1].ComradeId = usersQueue[0].Id;
            db.UpdateUser(usersQueue[0]);
            db.UpdateUser(usersQueue[1]);
            await bot.SendTextMessageAsync(usersQueue[0].Id, "Собеседник найден!");
            await bot.SendTextMessageAsync(usersQueue[1].Id, "Собеседник найден!");
            RemoveById(usersQueue[0]);
            RemoveById(usersQueue[0]);
        }

        Thread.Sleep(1000);
    }
}


bool ContainsById(telegram_bot_suka_blay.User us)
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

void RemoveById(telegram_bot_suka_blay.User us)
{
    for (int i = 0; i < usersQueue.Count; i++)
    {
        if (us.Id == usersQueue[i].Id)
        {
            usersQueue.RemoveAt(i);
        }
    }
}