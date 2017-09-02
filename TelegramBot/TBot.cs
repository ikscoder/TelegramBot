using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.DAL;

namespace TelegramBot
{
    // ReSharper disable once InconsistentNaming
    public static class TBot
    {
        private static string _help = @"
/help - Справка
";

        private static string _helpManager = "\n";

        private const string UnAutorized = @"
Пожалуйста, авторизуйтесь. Для этого введите /login <Токен>.
Токен авторизации можно получить в CRM системе.
";

        public static TelegramBotClient Bot { get;private set; }
        public static User Me { get; private set; }
        public static bool Load()
        {
            if (string.IsNullOrEmpty(Settings.Current.APIKey))
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "Empty API Key"));
                return false;
            }
            Bot = new TelegramBotClient(Settings.Current.APIKey);

            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnReceiveError += BotOnReceiveError;
            Me = Bot.GetMeAsync().Result;

            Data.Current.InsertOrUpdateUser(Me);

            _help += Settings.Current.AvailableCommands["dialog"]? "/dialog - Начать диалог с менеджером\n" :"";
            _help += Settings.Current.AvailableCommands["officelist"] ? "/officelist - Получить список офисов\n" : "";
            _help += Settings.Current.AvailableCommands["nearestoffice"] ? "/nearestoffice - Получить адрес ближайшего офиса\n" : "";
            _help += Settings.Current.AvailableCommands["recall"] ? "/recall - Заказать звонок\n" : "";
            _help += Settings.Current.AvailableCommands["opportunity"] ? "/opportunity <описание> - Добавить возможность\n" : "";
            _helpManager += Settings.Current.AvailableCommands["opportunitieslist"] ? "/opportunitieslist - Получить список возможностей\n" : "";

            return true;
        }



        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            //Log.Add(new Log.LogMessage(Log.MessageType.ERROR, receiveErrorEventArgs.ApiRequestException.Message));
        }

        private static readonly HashSet<long> Waiters=new HashSet<long>();

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            if (message == null) return;
            try
            {
                if (message.Text==null&&(await Data.Current.IsDialogWhithManagerOpenedAsync(message.Chat) || await Data.Current.IsAutorizedAsync(message.Chat)))
                {
                    Data.Current.InsertMessage(message);
                }
                if (message.Contact != null && Waiters.Contains(message.Chat.Id))
                {
                    Data.Current.InsertOrUpdateClient(message.From,message.Contact);
                    Data.Current.InsertOpportunity(message.From,"Перезвонить");
                    Waiters.Remove(message.Chat.Id);
                    await Bot.SendTextMessageAsync(message.Chat.Id, $"Наш менеджер перезвонит вам на: +{message.Contact.PhoneNumber}, {message.Contact.LastName} {message.Contact.FirstName}", replyMarkup: new ReplyKeyboardRemove());
                }
                else if (message.Location != null && Waiters.Contains(message.Chat.Id))
                {
                    Venue office = null;
                    double min = double.MaxValue;
                    foreach (var o in await Data.Current.GetOfficesAsync())
                    {
                        double dist =
                            Math.Sqrt(Math.Pow(o.Location.Longitude - message.Location.Longitude, 2) +
                                      Math.Pow(o.Location.Latitude - message.Location.Latitude, 2));
                        if (!(dist < min)) continue;
                        min = dist;
                        office = o;
                    }
                    if (office != null)
                    {
                        await Bot.SendTextMessageAsync(message.Chat.Id, $"Ближайший офис: {office.Address}", replyMarkup: new ReplyKeyboardRemove());
                        await Bot.SendVenueAsync(message.Chat.Id, office.Location.Latitude, office.Location.Longitude, office.Title, office.Address, replyMarkup: new ReplyKeyboardRemove());
                    }
                    else
                    {
                        await Bot.SendTextMessageAsync(message.Chat.Id, "Нет офисов.", replyMarkup: new ReplyKeyboardRemove());
                    }

                    Waiters.Remove(message.Chat.Id);
                }

                if (string.IsNullOrEmpty(message.Text)) return;
            
                if (message.Text.StartsWith("/start"))
                {
                    Data.Current.InsertOrUpdateChat(message.Chat);
                    Data.Current.InsertOrUpdateClient(message.From);
                    await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);  
                    await Bot.SendTextMessageAsync(message.Chat.Id, $"Здравствуйте, {message.From?.FirstName ?? message.From?.Username ?? "Незнакомец"}!\n\n \n{_help + (!await Data.Current.IsAutorizedAsync(message.Chat) ? "" : _helpManager)}",replyMarkup: new ReplyKeyboardRemove());
                }
                else if (message.Text.StartsWith("/stop"))
                {
                    Data.Current.InsertOrUpdateChat(message.Chat,isClosed:true);
                    await Bot.SendTextMessageAsync(message.Chat.Id, $"Прощайте, {message.From?.FirstName ?? message.From?.Username ?? "Незнакомец"}!",
                        replyMarkup: new ReplyKeyboardRemove());
                }
                else if (Settings.Current.AvailableCommands["officelist"]&&(message.Text.StartsWith("/officelist")|| message.Text.StartsWith("/officeslist")))
                {
                    if (await Data.Current.IsChatClosedAsync(message.Chat))
                    {
                        Data.Current.InsertOrUpdateChat(message.Chat, isClosed: false);
                        await Bot.SendTextMessageAsync(message.Chat.Id, $"Добро пожаловать снова, {message.From?.FirstName ?? message.From?.Username ?? "Незнакомец"}!", replyMarkup: new ReplyKeyboardRemove());
                    }
                    string offices = (await Data.Current.GetOfficesAsync()).Aggregate("", (current, office) => current + $"{office.Title}\n{office.Address}\n\n");
                    if (string.IsNullOrEmpty(offices))
                    {
                        await Bot.SendTextMessageAsync(message.Chat.Id, "Нет офисов", replyMarkup: new ReplyKeyboardRemove());
                    }
                    else await Bot.SendTextMessageAsync(message.Chat.Id, offices, replyMarkup: new ReplyKeyboardRemove());              
                }
                else if (Settings.Current.AvailableCommands["nearestoffice"] && message.Text.StartsWith("/nearestoffice"))
                {
                    if (await Data.Current.IsChatClosedAsync(message.Chat))
                    {
                        Data.Current.InsertOrUpdateChat(message.Chat, isClosed: false);
                        await Bot.SendTextMessageAsync(message.Chat.Id, $"Добро пожаловать снова, {message.From?.FirstName ?? message.From?.Username ?? "Незнакомец"}!", replyMarkup: new ReplyKeyboardRemove());
                    }
                    var keyboard = new ReplyKeyboardMarkup(new[]
                    {
                        new []
                        {
                            new KeyboardButton("Отправить месторасположение")
                            {
                                RequestLocation = true,
                            },
                        }
                    });
                    Waiters.Add(message.Chat.Id);
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Пожалуйста, отправьте ваше месторасположение для определения ближайшего офиса", replyMarkup: keyboard);
                }
                else if (message.Text.StartsWith("/help")|| message.Text.StartsWith("/hlep"))
                {
                    await Bot.SendTextMessageAsync(message.Chat.Id, _help+(!await Data.Current.IsAutorizedAsync(message.Chat)? "": _helpManager), replyMarkup: new ReplyKeyboardRemove());
                }
                else if (Settings.Current.AvailableCommands["dialog"] && message.Text.StartsWith("/dialog"))
                {
                    if (await Data.Current.IsChatClosedAsync(message.Chat))
                    {
                        Data.Current.InsertOrUpdateChat(message.Chat, isClosed: false,isDialogOpened:true);
                        await Bot.SendTextMessageAsync(message.Chat.Id, $"Добро пожаловать снова, {message.From?.FirstName ?? message.From?.Username ?? "Незнакомец"}!", replyMarkup: new ReplyKeyboardRemove());
                    }
                    else Data.Current.InsertOrUpdateChat(message.Chat, isDialogOpened: true);

                    await Bot.SendTextMessageAsync(message.Chat.Id, "Менеджер свяжется с вами совсем скоро.", replyMarkup: new ReplyKeyboardRemove());
                }
                else if (Settings.Current.AvailableCommands["recall"] && message.Text.StartsWith("/recall"))
                {
                    if (await Data.Current.IsChatClosedAsync(message.Chat)) {
                        Data.Current.InsertOrUpdateChat(message.Chat,isClosed:false);
                        await Bot.SendTextMessageAsync(message.Chat.Id, $"Добро пожаловать снова, {message.From?.FirstName ?? message.From?.Username ?? "Незнакомец"}!", replyMarkup: new ReplyKeyboardRemove());
                    }
                    var keyboard = new ReplyKeyboardMarkup(new[]
                    {
                        new []
                        {
                            new KeyboardButton("Отправить контактные данные")
                            {
                                RequestContact = true,
                            },
                        }
                    });
                    Waiters.Add(message.Chat.Id);
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Пожалуйста, отправьте нам ваши контактные данные:", replyMarkup: keyboard);
                }
                else if (Settings.Current.AvailableCommands["opportunity"] && message.Text.StartsWith("/opportunity"))
                {
                    if (await Data.Current.IsChatClosedAsync(message.Chat))
                    {
                        Data.Current.InsertOrUpdateChat(message.Chat, isClosed: false);
                        await Bot.SendTextMessageAsync(message.Chat.Id, $"Добро пожаловать снова, {message.From?.FirstName ?? message.From?.Username ?? "Незнакомец"}!", replyMarkup: new ReplyKeyboardRemove());
                    }

                    string description = message.Text.Remove(0, "/opportunity".Length).Trim();
                    if (string.IsNullOrEmpty(description))
                    {
                        await Bot.SendTextMessageAsync(message.Chat.Id, "Пожалуйста, добавьте описание", replyMarkup: new ReplyKeyboardRemove());
                    }
                    else
                    {
                        Data.Current.InsertOpportunity(message.From, description);
                        await Bot.SendTextMessageAsync(message.Chat.Id, "Ок", replyMarkup: new ReplyKeyboardRemove());
                    }
                
                }
                else if (message.Text.StartsWith("/login"))
                {
                    string token = message.Text.Remove(0, 6).Trim();
                    if (string.IsNullOrEmpty(token))
                    {
                        await Bot.SendTextMessageAsync(message.Chat.Id, UnAutorized, replyMarkup: new ReplyKeyboardRemove());
                        return;
                    }
                    if (await Data.Current.AutorizeAsync(token, message.Chat))
                    {
                        await Bot.SendTextMessageAsync(message.Chat.Id, $"Вы успешно авторизовались.\n{_help + (!await Data.Current.IsAutorizedAsync(message.Chat) ? "" : _helpManager)}", replyMarkup: new ReplyKeyboardRemove());
                    }
                    else
                    {
                        await Bot.SendTextMessageAsync(message.Chat.Id, "Неверный токен.", replyMarkup: new ReplyKeyboardRemove());
                    }
                }
                else if (Settings.Current.AvailableCommands["opportunitieslist"] && message.Text.StartsWith("/opportunitieslist"))
                {
                    if (!await Data.Current.IsAutorizedAsync(message.Chat))
                    {
                        await Bot.SendTextMessageAsync(message.Chat.Id, UnAutorized, replyMarkup: new ReplyKeyboardRemove());
                        return;
                    }
                    foreach (var op in await Data.Current.GetAllOpportunitiesAsync())
                    {
                        await Bot.SendTextMessageAsync(message.Chat.Id, op.Description, replyMarkup: new ReplyKeyboardRemove());
                        await Bot.SendContactAsync(message.Chat.Id, op.Contact.PhoneNumber, op.Contact.FirstName, op.Contact.LastName, replyMarkup: new ReplyKeyboardRemove());
                    }

                }
                else
                {
                    if(!(await Data.Current.IsDialogWhithManagerOpenedAsync(message.Chat))&&!await Data.Current.IsAutorizedAsync(message.Chat)) { 
                        await Bot.SendTextMessageAsync(message.Chat.Id, _help + (!await Data.Current.IsAutorizedAsync(message.Chat) ? "" : _helpManager), replyMarkup: new ReplyKeyboardRemove());
                    }
                    else
                    {
                        Data.Current.InsertMessage(message); 
                    }                    
                }
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "BotOnMessageReceived: " + e.Message));
#if DEBUG
                throw;
#endif
            }
        }


        public static async void SendMessages(IEnumerable<Message> messages)
        {
            foreach (var mes in messages)
            {
                var mesage = await Bot.SendTextMessageAsync(mes.Chat.Id, mes.Text, replyMarkup: new ReplyKeyboardRemove());
                Data.Current.InsertMessage(mesage);
            }
        }
    }
}
