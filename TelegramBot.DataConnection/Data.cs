using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Helpers;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;


namespace TelegramBot.DataConnection
{
    public class Data : IDisposable
    {

        #region Main

        public static bool InitConnection(string connectionString)
        {
            try
            {
                var factory = (OdbcFactory)DbProviderFactories.GetFactory("System.Data.Odbc");
                var connection = (OdbcConnection)factory.CreateConnection();
                connection.ConnectionString = connectionString;
                connection.Open();
                connection.Close();
                Current=new Data(connectionString);
                return true;
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "InitConnection: " + e.Message));
                return false;
            }

        }

        public static Data Current { get; set; }

        private OdbcConnection Connection { get; }

        private Data(string connectionString)
        {
            var factory = (OdbcFactory)DbProviderFactories.GetFactory("System.Data.Odbc");
            Connection = (OdbcConnection)factory.CreateConnection();
            Connection.ConnectionString = connectionString;
            Connection.Open();
        }

        public void Dispose()
        {
            Connection.Close();
            Connection?.Dispose();
        }

        #endregion

        #region Insert
        public async void InsertOrUpdateCurrentBot(User bot)
        {
            if (bot == null) return;
            try
            {
                await new OdbcCommand("DELETE FROM telegram_currentbot;", Connection).ExecuteNonQueryAsync();
                var command = new OdbcCommand($"INSERT INTO telegram_currentbot (id) VALUES ({bot.Id});", Connection);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR,"InsertOrUpdateCurrentBot: " + e.Message));
#if DEBUG
                throw;
#endif
            }
        }

        public async void InsertOrUpdateChat(Chat chat, bool? isClosed = null, bool? isDialogOpened = null)
        {
            if (chat == null) return;
            try
            {
                var test = new OdbcCommand($"select * from telegram_chats where id={chat.Id}", Connection);
                bool isUpdate = test.ExecuteReader().Read();
                var command = new OdbcCommand {Connection = Connection};
                if (isUpdate)
                {
                    command.CommandText =
                        $"UPDATE telegram_chats SET type = '{chat.Type}', title = '{chat.Title?.Replace("\\", "\\\\").Replace("\'","\\\'") }', username = '{chat.Username?.Replace("\\", "\\\\").Replace("\'","\\\'")}', first_name = '{chat.FirstName?.Replace("\\", "\\\\").Replace("\'","\\\'")}', last_name = '{chat.LastName?.Replace("\\", "\\\\").Replace("\'","\\\'")}'";
                    if (isClosed.HasValue)
                        command.CommandText += $", is_closed = {isClosed}";
                    if (isDialogOpened.HasValue)
                        command.CommandText += $", is_dialog_opened = {isDialogOpened}";
                    command.CommandText += $" WHERE id={chat.Id};";
                }
                else
                {
                    command.CommandText = $"INSERT INTO telegram_chats (id ,type ,title ,username ,first_name ,last_name ,is_closed, is_dialog_opened) VALUES ({chat.Id}, '{chat.Type}', '{chat.Title?.Replace("\\", "\\\\").Replace("\'","\\\'")}', '{chat.Username?.Replace("\\", "\\\\").Replace("\'","\\\'")}', '{chat.FirstName?.Replace("\\", "\\\\").Replace("\'","\\\'")}', '{chat.LastName?.Replace("\\", "\\\\").Replace("\'","\\\'")}', {isClosed??false}, {isDialogOpened??false});";
                }
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "InsertOrUpdateChat: " + e.Message));
#if DEBUG 
                throw;
#endif
            }
        }

        public async void InsertOrUpdateUser(User user)
        {
            if (user == null) return;
            try
            {
                var test = new OdbcCommand($"select * from telegram_users where id={user.Id};", Connection);
                var command = new OdbcCommand()
                {
                    Connection= Connection,
                    CommandText =
                        test.ExecuteReader().Read()
                            ? $"UPDATE telegram_users SET first_name = '{user.FirstName?.Replace("\\", "\\\\").Replace("\'","\\\'")}', last_name = '{user.LastName?.Replace("\\", "\\\\").Replace("\'","\\\'")}', username = '{user.Username?.Replace("\\", "\\\\").Replace("\'","\\\'")}' WHERE id = {user.Id};"
                            : $"INSERT INTO telegram_users (id, first_name, last_name, username) VALUES ({user.Id}, '{user.FirstName?.Replace("\\", "\\\\").Replace("\'","\\\'")}', '{user.LastName?.Replace("\\", "\\\\").Replace("\'","\\\'")}', '{user.Username?.Replace("\\", "\\\\").Replace("\'","\\\'")}');"
                };
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, $"InsertOrUpdateUser: {e.Message}" ));
#if DEBUG
                throw;
#endif
            }
        }

        public async void InsertOrUpdateClient(User user, Contact contact = null)
        {
            if (user == null) return;
            try
            {
                InsertOrUpdateUser(user);

                var test = new OdbcCommand($"select * from telegram_clients where telegramuserid={user.Id}", Connection);
                var command = new OdbcCommand()
                {
                    Connection= Connection,
                    CommandText =
                        (test.ExecuteReader().Read() && contact != null)
                            ? $"UPDATE telegram_clients SET phone = '{contact?.PhoneNumber?.Replace("\\", "\\\\").Replace("\'","\\\'")}' WHERE telegramuserid = {user.Id};"
                            : $"INSERT INTO telegram_clients (phone,telegramuserid) VALUES('{contact?.PhoneNumber?.Replace("\\", "\\\\").Replace("\'","\\\'")}', {user.Id});"
                };
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "InsertOrUpdateClient: " + e.Message));
#if DEBUG
                throw;
#endif
            }
        }

        public async void InsertOpportunity(User user, string description)
        {
            if (user == null || string.IsNullOrEmpty(description)) return;
            try
            {
                var reader = await new OdbcCommand($"SELECT id FROM telegram_clients where telegramuserid={user.Id};", Connection).ExecuteReaderAsync();
                if (!reader.Read()) return;
                var command = new OdbcCommand($"INSERT INTO opportunities (clientid,description) VALUES({reader[0]}, '{description?.Replace("\\", "\\\\").Replace("\'","\\\'")}');", Connection);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "InsertOpportunity: " + e.Message));
#if DEBUG
                throw;
#endif
            }
        }

        #endregion

        #region Select

        public async Task<List<Venue>> GetOfficesAsync()
        {
            try
            {
                var offices = new DataTable();
                offices.Load(await new OdbcCommand("SELECT latitude, longitude, title, address, foursquare_id FROM telegram_offices;", Connection).ExecuteReaderAsync());
                return Enumerable.Select(offices.AsEnumerable(), row => new Venue
                {
                    Location = new Location
                    {
                        Longitude = float.Parse(row["longitude"].ToString()),
                        Latitude = float.Parse(row["latitude"].ToString()),
                    },
                    Title = row["title"].ToString(),
                    Address = row["address"].ToString(),
                    FoursquareId = row["foursquare_id"].ToString(),
                }).ToList();
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "GetOfficesAsync: " + e.Message));
#if DEBUG
                throw;
#endif
                return new List<Venue>(0);
            }
        }

        public async Task<List<Opportunity>> GetAllOpportunitiesAsync()
        {

            try
            {
                var opportunities = new DataTable();
                opportunities.Load(await new OdbcCommand("SELECT id, clientid, description FROM opportunities;", Connection).ExecuteReaderAsync());
                var result = new List<Opportunity>(opportunities.AsEnumerable().Count());
                foreach (var row in opportunities.AsEnumerable())
                {
                    var o = new Opportunity
                    {
                        Description = row[2].ToString(),
                        Contact = new Contact()
                    };
                    var command = new OdbcCommand($"SELECT phone, telegramuserid FROM telegram_clients where id={row[1]} limit 1;", Connection);
                    var reader = await command.ExecuteReaderAsync();
                    if (reader.Read())
                    {

                        o.Contact.PhoneNumber = reader.GetString(0);
                        var userc = new OdbcCommand($"SELECT first_name, last_name FROM telegram_users where id={reader[1]} limit 1;", Connection);
                        var reader2 = await userc.ExecuteReaderAsync();
                        if (reader2.Read())
                        {
                            o.Contact.FirstName = reader2[0].ToString();
                            o.Contact.LastName = reader2[1].ToString();
                        }
                    }
                    result.Add(o);
                }
                return result;
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "GetAllOpportunitiesAsync: " + e.Message));
#if DEBUG
                throw;
#endif
                return new List<Opportunity>(0);
            }
        }

        public async Task<bool> IsChatClosedAsync(Chat chat)
        {
            if (chat == null) return true;
            try
            {
                var command = new OdbcCommand($"SELECT is_closed FROM telegram_chats where id={chat.Id};", Connection);
                var reader = await command.ExecuteReaderAsync();
                return !reader.Read() || reader.GetBoolean(0);
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR,  "IsChatClosedAsync: " + e.Message));
#if DEBUG
                throw;
#endif
                return true;
            }
        }

        public async Task<bool> IsDialogWhithManagerOpenedAsync(Chat chat)
        {
            if (chat == null) return false;
            try
            {
                var command = new OdbcCommand($"SELECT is_dialog_opened FROM telegram_chats where id={chat.Id};", Connection);
                var reader = await command.ExecuteReaderAsync();
                return reader.Read() && reader.GetBoolean(0);
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "IsDialogWhithManagerOpenedAsync: " + e.Message));
#if DEBUG
                throw;
#endif
                return false;
            }
        }

        public async Task<bool> IsAutorizedAsync(Chat chat)
        {
            if (chat == null) return false;
            try
            {
                var command = new OdbcCommand($"SELECT * FROM telegram_managers where telegramchatid={chat.Id};", Connection);
                var reader = await command.ExecuteReaderAsync();
                return reader.Read();
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "IsAutorizedAsync: " + e.Message));
#if DEBUG
                throw;
#endif
                return false;
            }
        }

        #endregion

        #region InsertsForMessage

        private async void InsertAudio(Audio audio)
        {
            if (audio == null) return;
            try
            {
                var command = new OdbcCommand($"INSERT INTO telegram_audios (file_id, duration, performer, title, mime_type, file_size) VALUES ('{audio.FileId?.Replace("\\", "\\\\").Replace("\'","\\\'")}', {audio.Duration}, '{audio.Performer?.Replace("\\", "\\\\").Replace("\'", "\\\'")}', '{audio.Title?.Replace("\\", "\\\\").Replace("\'", "\\\'")}', '{audio.MimeType?.Replace("\\", "\\\\").Replace("\'","\\\'")}', {audio.FileSize});", Connection);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR,  "InsertAudio: " + e.Message));
#if DEBUG
                throw;
#endif
            }

        }

        private async void InsertDocument(Document document)
        {
            if (document == null) return;
            try
            {
                var command = new OdbcCommand($"INSERT INTO telegram_documents (file_id, file_name, mime_type, file_size) VALUES ('{document.FileId?.Replace("\\", "\\\\").Replace("\'","\\\'")}', '{document.FileName?.Replace("\\", "\\\\").Replace("\'","\\\'")}', '{document.MimeType?.Replace("\\", "\\\\").Replace("\'","\\\'")}', {document.FileSize});", Connection);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR,  "InsertDocument: " + e.Message));
#if DEBUG
                throw;
#endif
            }

        }

        private async void InsertLocation(Message message)
        {
            if (message?.Location == null) return;
            try
            {
                var command = new OdbcCommand($"INSERT INTO telegram_locations (message_id, latitude, longitude) VALUES ({message.MessageId}, {message.Location.Latitude}, {message.Location.Longitude});", Connection);

                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR,  "InsertLocation: " + e.Message));
#if DEBUG
                throw;
#endif
            }

        }

        private async void InsertMessageEntities(Message message)
        {
            if (message?.Entities == null || message.Entities.Count == 0) return;
            foreach (var entity in message.Entities)
            {
                try
                {
                    var command = new OdbcCommand($"INSERT INTO telegram_messageentities (message_id, type, \"offset\", length, url) VALUES ({message.MessageId},'{entity.Type}', {entity.Offset}, {entity.Length},'{entity.Url?.Replace("\\", "\\\\").Replace("\'","\\\'")}');", Connection);
                    await command.ExecuteNonQueryAsync();
                }
                catch (Exception e)
                {
                    Log.Add(new Log.LogMessage(Log.MessageType.ERROR,  "InsertMessageEntities: " + e.Message));
#if DEBUG
                    throw;
#endif
                }
            }


        }

        private async void InsertPhotos(Message message)
        {
            if (message?.Photo == null || !message.Photo.Any()) return;
            foreach (var photo in message.Photo)
            {
                try
                {
                    var command = new OdbcCommand($"INSERT INTO telegram_photos (messageid, file_id, width, height, file_size) VALUES ({message.MessageId}, '{photo.FileId?.Replace("\\", "\\\\").Replace("\'","\\\'")}', {photo.Width}, {photo.Height},{photo.FileSize});", Connection);
                    await command.ExecuteNonQueryAsync();
                }
                catch (Exception e)
                {
                    Log.Add(new Log.LogMessage(Log.MessageType.ERROR,  "InsertPhotos: " + e.Message));
#if DEBUG
                    throw;
#endif
                }
            }

        }

        private async void InsertSticker(Sticker sticker)
        {
            if (sticker == null) return;
            try
            {
                var command = new OdbcCommand($"INSERT INTO telegram_stickers ( file_id, width, height, file_size) VALUES ('{sticker.FileId?.Replace("\\", "\\\\").Replace("\'","\\\'")}', {sticker.Width}, {sticker.Height},{sticker.FileSize});", Connection);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR,  "InsertSticker: " + e.Message));
#if DEBUG
                throw;
#endif
            }
        }

        private async void InsertVenue(Message message)
        {
            if (message?.Venue == null) return;
            try
            {
                var command = new OdbcCommand($"INSERT INTO telegram_venues ( message_id, latitude, longitude, title, address, foursquare_id) VALUES ({message.MessageId}, {message.Venue.Location.Latitude}, {message.Venue.Location.Longitude}, '{message.Venue.Title?.Replace("\\", "\\\\").Replace("\'","\\\'")}', '{message.Venue.Address?.Replace("\\", "\\\\").Replace("\'","\\\'")}', '{message.Venue.FoursquareId?.Replace("\\", "\\\\").Replace("\'","\\\'")}');", Connection);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR,  "InsertVenue: " + e.Message));
#if DEBUG
                throw;
#endif
            }
        }

        private async void InsertVideo(Video video)
        {
            if (video == null) return;

            var command = new OdbcCommand($"INSERT INTO telegram_videos ( file_id, width, height, duration, mime_type, file_size) VALUES ( '{video.FileId?.Replace("\\", "\\\\").Replace("\'","\\\'")}', {video.Width}, {video.Height}, {video.Duration}, '{video.MimeType?.Replace("\\", "\\\\").Replace("\'","\\\'")}', {video.FileSize});", Connection);
            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR,  "InsertVideo: " + e.Message));
#if DEBUG
                throw;
#endif
            }
        }

        private async void InsertVoice(Voice voice)
        {
            if (voice == null) return;

            try
            {
                var command = new OdbcCommand($"INSERT INTO telegram_voices ( file_id, duration, mime_type, file_size) VALUES ( '{voice.FileId?.Replace("\\", "\\\\").Replace("\'","\\\'")}', {voice.Duration}, '{voice.MimeType?.Replace("\\", "\\\\").Replace("\'","\\\'")}', {voice.FileSize});", Connection);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR,  "InsertVoice: " + e.Message));
#if DEBUG
                throw;
#endif
            }
        }

        #endregion


        //TODO убрать превью из стикера
        public async void InsertMessage(Message message, bool? isSended = null)
        {
            if (message == null) return;

            try
            {
                InsertAudio(message.Audio);
                InsertDocument(message.Document);
                InsertLocation(message);
                InsertMessageEntities(message);
                InsertPhotos(message);
                InsertSticker(message.Sticker);
                InsertVenue(message);
                InsertVideo(message.Video);
                InsertVoice(message.Voice);
                if(message.ForwardFrom!=null)InsertOrUpdateUser(message.ForwardFrom);
                if(message.ReplyToMessage!=null)InsertMessage(message.ReplyToMessage);
                if(message.PinnedMessage!=null)InsertMessage(message.PinnedMessage);
                var command = new OdbcCommand($"INSERT INTO  telegram_messages (message_id, from_id, date, chat_id, text, forward_from, forward_date, reply_to_message_id, pinned_message_id, document_id, caption, audio_id, video_id, voice_id, sticker_id, issended) VALUES({message.MessageId}, {message.From?.Id.ToString() ?? "NULL"}, {message.Date.ToUnixTime()}, {message.Chat.Id}, '{message.Text?.Replace("\\", "\\\\").Replace("\'","\\\'")}', {message.ForwardFrom?.Id.ToString() ?? "NULL"}, {message.ForwardDate?.ToUnixTime().ToString() ?? "NULL"}, {message.ReplyToMessage?.MessageId.ToString() ?? "NULL"}, {message.PinnedMessage?.MessageId.ToString() ?? "NULL"}, '{message.Document?.FileId?.Replace("\\", "\\\\").Replace("\'","\\\'")}', '{message.Caption?.Replace("\\", "\\\\").Replace("\'","\\\'")}', '{message.Audio?.FileId?.Replace("\\", "\\\\").Replace("\'","\\\'")}', '{message.Video?.FileId?.Replace("\\", "\\\\").Replace("\'","\\\'")}', '{message.Voice?.FileId?.Replace("\\", "\\\\").Replace("\'","\\\'")}', '{message.Sticker?.Thumb.FileId?.Replace("\\", "\\\\").Replace("\'","\\\'")}', {isSended??true});", Connection);

                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "InsertMessage: " + e.Message));
#if DEBUG
                throw;
#endif
            }
        }


        // TODO: отсылка прикрепленных объектов и нормальная замена их в бд
        public async Task<List<Message>> GetUnsendedMessages()
        {

            try
            {
                var messages = new DataTable();
                messages.Load(await new OdbcCommand($"SELECT message_id, from_id, date, chat_id, text FROM telegram_messages where not issended;", Connection).ExecuteReaderAsync());

                var command = new OdbcCommand("DELETE FROM telegram_messages WHERE message_id < 0;", Connection);
                await command.ExecuteNonQueryAsync();

                return Enumerable.Select(messages.AsEnumerable(), row => new Message
                {
                    MessageId = int.Parse(row[0].ToString()),
                    Chat = new Chat { Id = int.Parse(row[3].ToString()) },
                    Text = row[4].ToString()
                }).ToList();
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR,  "GetUnsendedMessages: " + e.Message));
#if DEBUG
                throw;
#endif
                return new List<Message>(0);
            }
        }



        public async Task<bool> AutorizeAsync(string token, Chat chat)
        {
            if (string.IsNullOrEmpty(token) || chat == null) return false;
            try
            {
                var command = new OdbcCommand($"SELECT managerid, telegramchatid, token FROM telegram_managers where token='{token.Replace("\\", "\\\\").Replace("\'","\\\'")}';", Connection);
                var reader = await command.ExecuteReaderAsync();
                if (!reader.Read()) return false;
                var updcommand = new OdbcCommand($"UPDATE telegram_managers SET telegramchatid ={chat.Id}, token=null where token='{token?.Replace("\\", "\\\\").Replace("\'","\\\'")}'; ", Connection);
                await updcommand.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "AutorizeAsync: " + e.Message));
#if DEBUG
                throw;
#endif
                return false;
            }
        }

        public async void SetToken(int managerId, string token)
        {
            if (string.IsNullOrEmpty(token)) return;
            try
            {
                var command = new OdbcCommand($"UPDATE telegram_managers SET telegramchatid = NULL, token ='{token?.Replace("\\", "\\\\").Replace("\'","\\\'")}' WHERE managerid={managerId}; ", Connection);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "SetToken: " + e.Message));
#if DEBUG
                throw;
#endif
            }
        }

        #region GUI

        public async Task<User> GetUserByIdAsync(int id)
        {
            try
            {
                var reader = await new OdbcCommand($"SELECT id, first_name, last_name, username FROM telegram_users WHERE id={id};", Connection).ExecuteReaderAsync();
                reader.Read();
                return new User
                {
                    Id = int.Parse(reader[0].ToString()),
                    FirstName = reader[1].ToString(),
                    LastName = reader[2].ToString(),
                    Username = reader[3].ToString()
                };
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "GetUserByIdAsync: " + e.Message));
#if DEBUG
                throw;
#endif
                return null;
            }
        }

        public async Task<User> GetBotAsync()
        {
            try
            {
                var reader = await new OdbcCommand("SELECT id FROM telegram_currentbot;", Connection).ExecuteReaderAsync();
                if (!reader.Read()) return null;
                return await GetUserByIdAsync(int.Parse(reader[0].ToString()));
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "GetBotAsync: " + e.Message));
#if DEBUG
                throw;
#endif
                return null;
            }
        }

        public async Task<List<string>> GetTablesAsync()
        {
            var tables = new DataTable();
            try
            {
                tables.Load(await new OdbcCommand("SELECT table_name FROM information_schema.tables where table_schema=\'public\' ORDER BY table_name;", Connection).ExecuteReaderAsync());
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "GetTablesAsync: " + e.Message));
#if DEBUG
                throw;
#endif
            }
            return Enumerable.Select(tables.AsEnumerable(), row => row[0].ToString()).ToList();
        }

        public async Task<DataTable> GetTableByNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            var offices = new DataTable();
            try
            {
                offices.Load(await new OdbcCommand($"SELECT * FROM {name};", Connection).ExecuteReaderAsync());
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "GetTableByNameAsync: " + e.Message));
#if DEBUG
                throw;
#endif
            }
            return offices;
        }

        public async Task<List<int>> GetAllManagerIdsAsync()
        {
            var managerIds = new DataTable();
            try
            {
                managerIds.Load(await new OdbcCommand($"SELECT managerid FROM telegram_managers;", Connection).ExecuteReaderAsync());
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "GetAllManagerIdsAsync: " + e.Message));
#if DEBUG
                throw;
#endif
            }
            return Enumerable.Select(managerIds.AsEnumerable(), row => int.Parse(row[0].ToString())).ToList();
        }

        public async Task<List<Chat>> GetManagerChatsAsync()
        {
            var result = new List<Chat>();
            var chatids = new DataTable();
            try
            {
                chatids.Load(await new OdbcCommand($"SELECT telegramchatid FROM telegram_managers where telegramchatid IS NOT NULL;", Connection).ExecuteReaderAsync());

                foreach (var chatid in chatids.AsEnumerable())
                {
                    var chats = new DataTable();
                    chats.Load(await new OdbcCommand($"SELECT id, type, title, username, first_name, last_name FROM telegram_chats where id={chatid[0]};", Connection).ExecuteReaderAsync());

                    result.AddRange(Enumerable.Select(chats.AsEnumerable(), row => new Chat
                    {
                        Id = int.Parse(row[0].ToString()),
                        Type = (ChatType)Enum.Parse(typeof(ChatType), row[1].ToString()),
                        Title = row[2].ToString(),
                        Username = row[3].ToString(),
                        FirstName = row[4].ToString(),
                        LastName = row[5].ToString(),
                    }));

                }
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "GetManagerChatsAsync: " + e.Message));
#if DEBUG
                throw;
#endif
            }
            return result;

        }

        //TODO: полноценная загрузка сообщений
        public async Task<List<Message>> GetMessagesFromChatAsync(Chat chat)
        {
            var messages = new DataTable();
            var users = new DataTable();

            try
            {
                messages.Load(await new OdbcCommand($"SELECT message_id, from_id, date, text, sticker_id FROM telegram_messages where chat_id={chat.Id};", Connection).ExecuteReaderAsync());
                users.Load(await new OdbcCommand("SELECT id, first_name, last_name, username FROM telegram_users;", Connection).ExecuteReaderAsync());

            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "GetMessagesFromChatAsync: " + e.Message));
#if DEBUG
                throw;
#endif
            }
            var startDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Enumerable.Select(messages.AsEnumerable(), row => new Message
            {
                MessageId = int.Parse(row[0].ToString()),
                From = Enumerable.Select(users.AsEnumerable(), user => new User
                {
                    Id = int.Parse(user[0].ToString()),
                    FirstName = user[1].ToString(),
                    LastName = user[2].ToString(),
                    Username = user[3].ToString()
                }).FirstOrDefault(r => r.Id == int.Parse(row[1].ToString())),
                Date = startDateTime.AddSeconds(int.Parse(row[2].ToString())),
                Chat = chat,
                Text = row[3].ToString(),
                Sticker = new Sticker
                {
                    FileId = row[4].ToString()
                }
            }).ToList();
        }

        public async Task<List<Chat>> GetOpenedDialogChatsAsync()
        {

            var chats = new DataTable();
            try
            {
                chats.Load(await new OdbcCommand("SELECT id, type, title, username, first_name, last_name FROM telegram_chats where is_dialog_opened and not is_closed;", Connection).ExecuteReaderAsync());

            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "GetOpenedChatsAsync: " + e.Message));
#if DEBUG
                throw;
#endif
            }
            return Enumerable.Select(chats.AsEnumerable(), row => new Chat
            {
                Id = int.Parse(row[0].ToString()),
                Type = (ChatType)Enum.Parse(typeof(ChatType), row[1].ToString()),
                Title = row[2].ToString(),
                Username = row[3].ToString(),
                FirstName = row[4].ToString(),
                LastName = row[5].ToString(),
            }).ToList();
        }

        public async Task<List<Chat>> GetAllChatsAsync()
        {

            var chats = new DataTable();
            try
            {
                chats.Load(await new OdbcCommand("SELECT id, type, title, username, first_name, last_name FROM telegram_chats where not is_closed;", Connection).ExecuteReaderAsync());

            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "GetOpenedChatsAsync: " + e.Message));
#if DEBUG
                throw;
#endif
            }
            return Enumerable.Select(chats.AsEnumerable(), row => new Chat
            {
                Id = int.Parse(row[0].ToString()),
                Type = (ChatType)Enum.Parse(typeof(ChatType), row[1].ToString()),
                Title = row[2].ToString(),
                Username = row[3].ToString(),
                FirstName = row[4].ToString(),
                LastName = row[5].ToString(),
            }).ToList();
        }

        public async Task<PhotoSize> GetPhotoFromMessageAsync(Message message)
        {
            try
            {
                var reader = await new OdbcCommand($"SELECT file_id, width, height, file_size FROM telegram_photos where messageid={message.MessageId} and width=320;", Connection).ExecuteReaderAsync();
                if (!reader.Read()) return null;
                return new PhotoSize
                {
                    FileId = reader[0].ToString(),
                    Width = int.Parse(reader[1].ToString()),
                    Height = int.Parse(reader[2].ToString()),
                    FileSize = string.IsNullOrWhiteSpace(reader[3].ToString())?0:int.Parse(reader[3].ToString())
                };
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "GetPhotoFromMessageAsync: " + e.Message));
#if DEBUG
                throw;
#endif
                return null;
            }
        }

        #endregion
    }
}
