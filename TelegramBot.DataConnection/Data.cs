using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Helpers;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBot.DAL
{
    public class Data : IDisposable
    {

        #region Main

        public static bool InitConnection(string connectionString)
        {
            try
            {
                var factory = (OleDbFactory)DbProviderFactories.GetFactory("System.Data.OleDb");
                var connection = (OleDbConnection)factory.CreateConnection();
                connection.ConnectionString = connectionString;
                connection.Open();
                connection.Close();
                Current = new Data(connectionString);
                return true;
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "InitConnection: " + e.Message));
                return false;
            }

        }

        public static Data Current { get; set; }

        private OleDbConnection Connection { get; }

        private Data(string connectionString)
        {
            var factory = (OleDbFactory)DbProviderFactories.GetFactory("System.Data.OleDb");
            Connection = (OleDbConnection)factory.CreateConnection();
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

        public async void InsertOrUpdateChat(Chat chat, bool? isClosed = null, bool? isDialogOpened = null)
        {
            if (chat == null) return;
            try
            {
                var test = new OleDbCommand($"select * from telegram_chats where id={chat.Id}", Connection);
                bool isUpdate = test.ExecuteReader()?.Read() ?? false;
                var command = new OleDbCommand { Connection = Connection };
                if (isUpdate)
                {
                    command.CommandText =
                        $"UPDATE telegram_chats SET type = '{chat.Type}', title = '{chat.Title?.Replace("\'", "\'\'") }', username = '{chat.Username?.Replace("\'", "\'\'")}', first_name = '{chat.FirstName?.Replace("\'", "\'\'")}', last_name = '{chat.LastName?.Replace("\'", "\'\'")}'".Replace("\'\'", "NULL").Replace("NULLNULL", "\'\'\'\'");
                    if (isClosed.HasValue)
                        command.CommandText += $", is_closed = {(isClosed.Value ? 1 : 0)}";
                    if (isDialogOpened.HasValue)
                        command.CommandText += $", is_dialog_opened = {(isDialogOpened.Value ? 1 : 0)}";
                    command.CommandText += $" WHERE id={chat.Id};";
                }
                else
                {
                    command.CommandText = $"INSERT INTO telegram_chats (id ,type ,title ,username ,first_name ,last_name ,is_closed, is_dialog_opened) VALUES ({chat.Id}, '{chat.Type}', '{chat.Title?.Replace("\'", "\'\'")}', '{chat.Username?.Replace("\'", "\'\'")}', '{chat.FirstName?.Replace("\'", "\'\'")}', '{chat.LastName?.Replace("\'", "\'\'")}', {((isClosed ?? false) ? 1 : 0)}, {((isDialogOpened ?? false) ? 1 : 0)});".Replace("\'\'", "NULL").Replace("NULLNULL", "\'\'\'\'");
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
                var test = new OleDbCommand($"select * from telegram_users where id={user.Id};", Connection);
                var command = new OleDbCommand()
                {
                    Connection = Connection,
                    CommandText =
                        test.ExecuteReader()?.Read() ?? false
                            ? $"UPDATE telegram_users SET first_name = '{user.FirstName?.Replace("\'", "\'\'")}', last_name = '{user.LastName?.Replace("\'", "\'\'")}', username = '{user.Username?.Replace("\'", "\'\'")}' WHERE id = {user.Id};".Replace("\'\'", "NULL").Replace("NULLNULL", "\'\'\'\'")
                            : $"INSERT INTO telegram_users (id, first_name, last_name, username) VALUES ({user.Id}, '{user.FirstName?.Replace("\'", "\'\'")}', '{user.LastName?.Replace("\'", "\'\'")}', '{user.Username?.Replace("\'", "\'\'")}');".Replace("\'\'", "NULL").Replace("NULLNULL", "\'\'\'\'")
                };
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, $"InsertOrUpdateUser: {e.Message}"));
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
                InsertContact(contact);

                var test = new OleDbCommand($"select * from telegram_clients where telegram_user_id={user.Id}", Connection);
                var command = new OleDbCommand()
                {
                    Connection = Connection,
                    CommandText =
                        ((test.ExecuteReader()?.Read() ?? false) && contact != null)
                            ? $"UPDATE telegram_clients SET phone = '{contact.PhoneNumber?.Replace("\'", "\'\'")}' WHERE telegram_user_id = {user.Id};".Replace("\'\'", "NULL").Replace("NULLNULL", "\'\'\'\'")
                            : $"INSERT INTO telegram_clients (phone,telegram_user_id) VALUES('{contact?.PhoneNumber?.Replace("\'", "\'\'")}', {user.Id});".Replace("\'\'", "NULL").Replace("NULLNULL", "\'\'\'\'")
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
                var reader = await new OleDbCommand($"SELECT id FROM telegram_clients where telegram_user_id={user.Id};", Connection).ExecuteReaderAsync();
                if (!reader.Read()) return;
                var command = new OleDbCommand($"INSERT INTO opportunities (clientid,description) VALUES({reader[0]}, '{description.Replace("\'", "\'\'")}');".Replace("\'\'", "NULL").Replace("NULLNULL", "\'\'\'\'"), Connection);
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
                offices.Load(await new OleDbCommand("SELECT latitude, longitude, title, address, foursquare_id FROM telegram_offices;", Connection).ExecuteReaderAsync());
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
                opportunities.Load(await new OleDbCommand("SELECT id, clientid, description FROM opportunities;", Connection).ExecuteReaderAsync());
                var result = new List<Opportunity>(opportunities.AsEnumerable().Count());
                foreach (var row in opportunities.AsEnumerable())
                {
                    var o = new Opportunity
                    {
                        Description = row[2].ToString(),
                        Contact = new Contact()
                    };
                    var command = new OleDbCommand($"SELECT phone, telegram_user_id FROM telegram_clients where id={row[1]};", Connection);
                    var reader = await command.ExecuteReaderAsync();
                    if (reader.Read())
                    {

                        o.Contact.PhoneNumber = reader.GetString(0);
                        var userc = new OleDbCommand($"SELECT first_name, last_name FROM telegram_users where id={reader[1]};", Connection);
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
                var command = new OleDbCommand($"SELECT is_closed FROM telegram_chats where id={chat.Id};", Connection);
                var reader = await command.ExecuteReaderAsync();
                return !reader.Read() || reader.GetBoolean(0);
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "IsChatClosedAsync: " + e.Message));
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
                var command = new OleDbCommand($"SELECT is_dialog_opened FROM telegram_chats where id={chat.Id};", Connection);
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
                var command = new OleDbCommand($"SELECT * FROM telegram_managers where telegramchatid={chat.Id};", Connection);
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

        private async void InsertContact(Contact contact)
        {
            if (contact == null) return;
            try
            {
                var command = new OleDbCommand($"INSERT INTO telegram_contacts (phone_number, user_id, first_name, last_name) VALUES ('{contact.PhoneNumber?.Replace("\'", "\'\'")}', {contact.UserId}, '{contact.FirstName?.Replace("\'", "\'\'")}', '{contact.LastName?.Replace("\'", "\'\'")}');".Replace("\'\'", "NULL").Replace("NULLNULL", "\'\'\'\'"), Connection);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "InsertContact: " + e.Message));
            }

        }

        private async void InsertAudio(Audio audio)
        {
            if (audio == null) return;
            try
            {
                var command = new OleDbCommand($"INSERT INTO telegram_audios (file_id, duration, performer, title, mime_type, file_size) VALUES ('{audio.FileId?.Replace("\'", "\'\'")}', {audio.Duration}, '{audio.Performer?.Replace("\'", "\'\'")}', '{audio.Title?.Replace("\'", "\'\'")}', '{audio.MimeType?.Replace("\'", "\'\'")}', {audio.FileSize});".Replace("\'\'", "NULL").Replace("NULLNULL", "\'\'\'\'"), Connection);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "InsertAudio: " + e.Message));
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
                var command = new OleDbCommand($"INSERT INTO telegram_documents (file_id, file_name, mime_type, file_size) VALUES ('{document.FileId?.Replace("\'", "\'\'")}', '{document.FileName?.Replace("\'", "\'\'")}', '{document.MimeType?.Replace("\'", "\'\'")}', {document.FileSize});".Replace("\'\'", "NULL").Replace("NULLNULL", "\'\'\'\'"), Connection);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "InsertDocument: " + e.Message));
#if DEBUG
                throw;
#endif
            }

        }

        private async Task<Guid?> InsertLocation(Location location)
        {
            if (location == null) return null;
            try
            {
                var id = Guid.NewGuid();
                var command = new OleDbCommand($"INSERT INTO telegram_locations (id, latitude, longitude) VALUES ('{id}', {location.Latitude.ToString().Replace(',', '.')}, {location.Longitude.ToString().Replace(',', '.')});", Connection);

                await command.ExecuteNonQueryAsync();
                return id;
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "InsertLocation: " + e.Message));
#if DEBUG
                throw;
#endif
                return null;
            }

        }

        private async Task<bool> InsertMessageEntities(Message message)
        {
            if (message?.Entities == null || message.Entities.Count == 0) return false;
            foreach (var entity in message.Entities)
            {
                try
                {
                    var command = new OleDbCommand($"INSERT INTO telegram_message_entities (message_id, type, \"offset\", length, url) VALUES ({message.MessageId},'{entity.Type}', {entity.Offset}, {entity.Length},'{entity.Url?.Replace("\'", "\'\'")}');".Replace("\'\'", "NULL").Replace("NULLNULL", "\'\'\'\'"), Connection);
                    await command.ExecuteNonQueryAsync();
                }
                catch (Exception e)
                {
                    Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "InsertMessageEntities: " + e.Message));
#if DEBUG
                    throw;
#endif
                }
            }
            return true;

        }

        private async Task<bool> InsertPhotos(Message message)
        {
            if (message?.Photo == null || !message.Photo.Any()) return false;
            foreach (var photo in message.Photo)
            {
                try
                {
                    var command = new OleDbCommand($"INSERT INTO telegram_photos (messageid, file_id, width, height, file_size) VALUES ({message.MessageId}, '{photo.FileId?.Replace("\'", "\'\'")}', {photo.Width}, {photo.Height},{photo.FileSize});".Replace("\'\'", "NULL").Replace("NULLNULL", "\'\'\'\'"), Connection);
                    await command.ExecuteNonQueryAsync();
                }
                catch (Exception e)
                {
                    Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "InsertPhotos: " + e.Message));
#if DEBUG
                    throw;
#endif
                }
            }
            return true;
        }

        private async void InsertSticker(Sticker sticker)
        {
            if (sticker == null) return;
            try
            {
                var command = new OleDbCommand($"INSERT INTO telegram_stickers ( file_id, width, height, file_size) VALUES ('{sticker.FileId?.Replace("\'", "\'\'")}', {sticker.Width}, {sticker.Height},{sticker.FileSize});".Replace("\'\'", "NULL").Replace("NULLNULL", "\'\'\'\'"), Connection);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "InsertSticker: " + e.Message));
#if DEBUG
                throw;
#endif
            }
        }

        private async Task<Guid?> InsertVenue(Venue venue)
        {
            if (venue == null) return null;
            try
            {
                var id = Guid.NewGuid();
                var command = new OleDbCommand($"INSERT INTO telegram_venues ( id, latitude, longitude, title, address, foursquare_id) VALUES ('{id}', {venue.Location.Latitude.ToString().Replace(',', '.')}, {venue.Location.Longitude.ToString().Replace(',', '.')}, '{venue.Title?.Replace("\'", "\'\'")}', '{venue.Address?.Replace("\'", "\'\'")}', '{venue.FoursquareId?.Replace("\'", "\'\'")}');".Replace("\'\'", "NULL").Replace("NULLNULL", "\'\'\'\'"), Connection);
                await command.ExecuteNonQueryAsync();
                return id;
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "InsertVenue: " + e.Message));
#if DEBUG
                throw;
#endif
                return null;
            }
        }

        private async void InsertVideo(Video video)
        {
            if (video == null) return;

            var command = new OleDbCommand($"INSERT INTO telegram_videos ( file_id, width, height, duration, mime_type, file_size) VALUES ( '{video.FileId?.Replace("\'", "\'\'")}', {video.Width}, {video.Height}, {video.Duration}, '{video.MimeType?.Replace("\'", "\'\'")}', {video.FileSize});".Replace("\'\'", "NULL").Replace("NULLNULL", "\'\'\'\'"), Connection);
            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "InsertVideo: " + e.Message));
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
                var command = new OleDbCommand($"INSERT INTO telegram_voices ( file_id, duration, mime_type, file_size) VALUES ( '{voice.FileId?.Replace("\'", "\'\'")}', {voice.Duration}, '{voice.MimeType?.Replace("\'", "\'\'")}', {voice.FileSize});".Replace("\'\'", "NULL").Replace("NULLNULL", "\'\'\'\'"), Connection);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "InsertVoice: " + e.Message));
#if DEBUG
                throw;
#endif
            }
        }

        #endregion


        public async Task DeleteMessageAsync(int messageId)
        {
            try
            {
                var command = new OleDbCommand($"DELETE FROM telegram_messages WHERE message_id={messageId};", Connection);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "DeleteMessage: " + e.Message));
#if DEBUG
                throw;
#endif
            }
        }

        //TODO убрать превью из стикера
        public async void InsertMessage(Message message, bool? isSended = null, bool? isReply = null, bool? isPinned = null)
        {
            if (message == null) return;

            try
            {
                InsertAudio(message.Audio);
                InsertDocument(message.Document);
                var locationId = await InsertLocation(message.Location);
                bool haveME = await InsertMessageEntities(message);
                bool havePhotos = await InsertPhotos(message);
                InsertSticker(message.Sticker);
                var venueId = await InsertVenue(message.Venue);
                InsertVideo(message.Video);
                InsertVoice(message.Voice);
                InsertContact(message.Contact);

                if (message.ForwardFrom != null) InsertOrUpdateUser(message.ForwardFrom);
                if (message.ReplyToMessage != null) InsertMessage(message.ReplyToMessage, isReply: true);
                if (message.PinnedMessage != null) InsertMessage(message.PinnedMessage, isPinned: true);
                var command = new OleDbCommand($"INSERT INTO  telegram_messages (message_id, from_id, date, chat_id, text, forward_from, forward_date, reply_to_message_id, pinned_message_id, document_id, caption, audio_id, video_id, voice_id, venue_id, location_id, have_photos, have_message_entities, phone, sticker_id, isreply, ispinned, issended) VALUES({message.MessageId}, {message.From?.Id.ToString() ?? "NULL"}, {message.Date.ToUnixTime()}, {message.Chat.Id}, '{message.Text?.Replace("\'", "\'\'")}', {message.ForwardFrom?.Id.ToString() ?? "NULL"}, {message.ForwardDate?.ToUnixTime().ToString() ?? "NULL"}, {message.ReplyToMessage?.MessageId.ToString() ?? "NULL"}, {message.PinnedMessage?.MessageId.ToString() ?? "NULL"}, '{message.Document?.FileId?.Replace("\'", "\'\'")}', '{message.Caption?.Replace("\'", "\'\'")}', '{message.Audio?.FileId?.Replace("\'", "\'\'")}', '{message.Video?.FileId?.Replace("\'", "\'\'")}', '{message.Voice?.FileId?.Replace("\'", "\'\'")}','{venueId}','{locationId}',{(havePhotos ? 1 : 0)},{(haveME ? 1 : 0)},'{message?.Contact?.PhoneNumber?.Replace("\'", "\'\'")}', '{message.Sticker?.Thumb.FileId?.Replace("\'", "\'\'")}', {((isReply ?? false) ? 1 : 0)}, {((isPinned ?? false) ? 1 : 0)}, {((isSended ?? true) ? 1 : 0)});".Replace("\'\'", "NULL").Replace("NULLNULL", "\'\'\'\'"), Connection);

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
                messages.Load(await new OleDbCommand($"SELECT message_id, from_id, date, chat_id, text FROM telegram_messages where issended=0;", Connection).ExecuteReaderAsync());

                var command = new OleDbCommand("DELETE FROM telegram_messages WHERE message_id<0;", Connection);
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
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "GetUnsendedMessages: " + e.Message));
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
                var command = new OleDbCommand($"SELECT managerid, telegramchatid, token FROM telegram_managers where token='{token.Replace("\'", "\'\'")}';".Replace("\'\'", "NULL").Replace("NULLNULL", "\'\'\'\'"), Connection);
                var reader = await command.ExecuteReaderAsync();
                if (!reader.Read()) return false;
                var updcommand = new OleDbCommand($"UPDATE telegram_managers SET telegramchatid ={chat.Id}, token=null where token='{token?.Replace("\'", "\'\'")}'; ".Replace("\'\'", "NULL").Replace("NULLNULL", "\'\'\'\'"), Connection);
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
                var command = new OleDbCommand($"UPDATE telegram_managers SET telegramchatid = NULL, token ='{token?.Replace("\'", "\'\'")}' WHERE managerid={managerId}; ".Replace("\'\'", "NULL").Replace("NULLNULL", "\'\'\'\'"), Connection);
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
                var reader = await new OleDbCommand($"SELECT id, first_name, last_name, username FROM telegram_users WHERE id={id};", Connection).ExecuteReaderAsync();
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

        public List<string> GetTables()
        {
            //            var tables = new DataTable();
            //            try
            //            {
            //                tables.Load(await new OleDbCommand("SELECT table_name FROM information_schema.tables where table_schema=\'public\' ORDER BY table_name;", Connection).ExecuteReaderAsync());
            //            }
            //            catch (Exception e)
            //            {
            //                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "GetTablesAsync: " + e.Message));
            //#if DEBUG
            //                throw;
            //#endif
            //            }
            //            return Enumerable.Select(tables.AsEnumerable(), row => row[0].ToString()).ToList();
            return new List<string>
                            {
                             "opportunities",
                             "telegram_audios",
                             "telegram_chats",
                             "telegram_clients",
                             "telegram_contacts",
                             "telegram_documents",
                             "telegram_locations",
                             "telegram_managers",
                             "telegram_message_entities",
                             "telegram_messages",
                             "telegram_offices",
                             "telegram_photos",
                             "telegram_stickers",
                             "telegram_users",
                             "telegram_venues",
                             "telegram_videos",
                             "telegram_voices",
                            };
        }

        public async Task<DataTable> GetTableByNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            var offices = new DataTable();
            try
            {
                offices.Load(await new OleDbCommand($"SELECT * FROM {name};", Connection).ExecuteReaderAsync());
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
                managerIds.Load(await new OleDbCommand($"SELECT managerid FROM telegram_managers;", Connection).ExecuteReaderAsync());
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
                chatids.Load(await new OleDbCommand($"SELECT telegramchatid FROM telegram_managers where telegramchatid IS NOT NULL;", Connection).ExecuteReaderAsync());

                foreach (var chatid in chatids.AsEnumerable())
                {
                    var chats = new DataTable();
                    chats.Load(await new OleDbCommand($"SELECT id, type, title, username, first_name, last_name FROM telegram_chats where id={chatid[0]};", Connection).ExecuteReaderAsync());

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

        public async Task<Document> GetDocumentAsync(string fileid)
        {
            if (string.IsNullOrWhiteSpace(fileid)) return null;
            try
            {
                var docrow =
                    await new OleDbCommand(
                            $"SELECT file_name, mime_type, file_size FROM telegram_documents where file_id='{fileid}';", Connection)
                        .ExecuteReaderAsync();

                return docrow.Read() ? new Document
                {
                    FileId = fileid,
                    FileName = docrow[0].ToString(),
                    MimeType = docrow[1].ToString(),
                    FileSize = string.IsNullOrWhiteSpace(docrow[2].ToString()) ? 0 : int.Parse(docrow[2].ToString())
                } : null;
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "GetOfficesAsync: " + e.Message));
#if DEBUG
                throw;
#endif
                return null;
            }
        }

        public async Task<List<Message>> GetMessagesFromChatAsync(Chat chat)
        {
            if (chat == null) return new List<Message>(0);
            var messages = new DataTable();


            try
            {
                messages.Load(await new OleDbCommand($@"SELECT telegram_messages.message_id
      ,telegram_messages.from_id
	  ,telegram_users.first_name as user_first_name
	  ,telegram_users.last_name as user_last_name
	  ,telegram_users.username as user_username

      ,telegram_messages.date
      ,telegram_messages.text
      ,telegram_messages.forward_from
      ,telegram_messages.forward_date
      ,telegram_messages.reply_to_message_id
      ,telegram_messages.pinned_message_id
      ,telegram_messages.caption

      ,telegram_messages.have_photos
      ,telegram_messages.have_message_entities


      ,telegram_messages.audio_id as aud_fileid
	  ,telegram_audios.mime_type as aud_mime_type
	  ,telegram_audios.performer as aud_performer
	  ,telegram_audios.title as aud_title
	  ,telegram_audios.duration as aud_duration
	  ,telegram_audios.file_size as aud_file_size

	  ,telegram_messages.document_id as doc_fileid
	  ,telegram_documents.file_name as doc_file_name
	  ,telegram_documents.file_size as doc_file_size
	  ,telegram_documents.mime_type as doc_mime_type

	  ,telegram_messages.phone as contact_phone
	  ,telegram_contacts.first_name as contact_first_name
	  ,telegram_contacts.last_name as contact_last_name
	  ,telegram_contacts.user_id as contact_user_id

	  ,telegram_messages.video_id as video_fileid
	  ,telegram_videos.duration as video_duration
	  ,telegram_videos.file_size as video_file_size
	  ,telegram_videos.height as video_height
	  ,telegram_videos.width as video_width
	  ,telegram_videos.mime_type as video_mime_type

      ,telegram_messages.voice_id as voice_fileid
	  ,telegram_voices.duration as voice_duration
	  ,telegram_voices.file_size as voice_file_size
	  ,telegram_voices.mime_type as voice_mime_type


	  ,telegram_messages.venue_id as venue
      ,telegram_venues.address as venue_address
	  ,telegram_venues.latitude as venue_latitude
	  ,telegram_venues.longitude as venue_longitude
	  ,telegram_venues.title as venue_title
	  ,telegram_venues.foursquare_id as venue_foursquare_id

	  ,telegram_messages.location_id as location
      ,telegram_locations.latitude as location_latitude
	  ,telegram_locations.longitude as location_longitude
	  
      ,telegram_messages.sticker_id as sticker_fileid
	  ,telegram_stickers.file_size as sticker_file_size
	  ,telegram_stickers.height as sticker_height
	  ,telegram_stickers.width as sticker_width


  FROM telegram_messages
  left join telegram_users on telegram_messages.from_id=telegram_users.id
  left join telegram_audios on telegram_messages.audio_id=telegram_audios.file_id
  left join telegram_contacts on telegram_messages.phone=telegram_contacts.phone_number
  left join telegram_documents on telegram_messages.document_id=telegram_documents.file_id
  left join telegram_locations on telegram_messages.location_id=telegram_locations.id
  left join telegram_stickers on telegram_messages.sticker_id=telegram_stickers.file_id
  left join telegram_venues on telegram_messages.venue_id=telegram_venues.id
  left join telegram_videos on telegram_messages.video_id=telegram_videos.file_id
  left join telegram_voices on telegram_messages.voice_id=telegram_voices.file_id
  where chat_id={chat.Id} and isreply=0"
                    , Connection).ExecuteReaderAsync());
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "GetMessagesFromChatAsync: " + e.Message));
#if DEBUG
                throw;
#endif
            }
            var startDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

            var messagesList = new List<Message>(messages.Rows.Count);
            //TODO
            //,telegram_messages.forward_from
            //,telegram_messages.reply_to_message_id
            //,telegram_messages.pinned_message_id
            foreach (var row in messages.AsEnumerable().AsParallel())
            {
                var mes = new Message
                {
                    MessageId = int.Parse(row["message_id"].ToString()),
                    From = row.IsNull("from_id")
                        ? null
                        : new User
                        {
                            Id = int.Parse(row["from_id"].ToString()),
                            FirstName = row["user_first_name"]?.ToString(),
                            LastName = row["user_last_name"]?.ToString(),
                            Username = row["user_username"]?.ToString()
                        },
                    Date = startDateTime.AddSeconds(int.Parse(row["date"]?.ToString() ?? "0")),
                    Chat = chat,
                    Text = row.IsNull("text") ? null : row["text"].ToString(),
                    ForwardDate = row.IsNull("forward_date") ? (DateTime?)null : startDateTime.AddSeconds(int.Parse(row["forward_date"]?.ToString() ?? "0")),

                    Caption = row.IsNull("caption") ? null : row["caption"].ToString(),
                    Audio = row.IsNull("aud_fileid")
                        ? null
                        : new Audio
                        {
                            FileId = row["aud_fileid"].ToString(),
                            Duration = int.Parse(row["aud_duration"]?.ToString() ?? "0"),
                            MimeType = row["aud_mime_type"]?.ToString(),
                            Performer = row["aud_performer"]?.ToString(),
                            Title = row["aud_title"]?.ToString(),
                            FileSize = int.Parse(row["aud_file_size"]?.ToString() ?? "0")
                        },
                    Document = row.IsNull("doc_fileid")
                        ? null
                        : new Document
                        {
                            FileId = row["doc_fileid"].ToString(),
                            FileName = row["doc_file_name"]?.ToString(),
                            MimeType = row["doc_mime_type"]?.ToString(),
                            FileSize = int.Parse(row["doc_file_size"]?.ToString() ?? "0"),
                        },
                    Contact = row.IsNull("contact_phone")
                        ? null
                        : new Contact
                        {
                            PhoneNumber = row["contact_phone"].ToString(),
                            FirstName = row["contact_first_name"]?.ToString(),
                            LastName = row["contact_last_name"]?.ToString(),
                            UserId = int.Parse(row["contact_user_id"]?.ToString() ?? "0")
                        },
                    Video = row.IsNull("video_fileid")
                        ? null
                        : new Video
                        {
                            FileId = row["video_fileid"].ToString(),
                            Duration = int.Parse(row["video_duration"]?.ToString() ?? "0"),
                            MimeType = row["video_mime_type"]?.ToString(),
                            FileSize = int.Parse(row["video_file_size"]?.ToString() ?? "0"),
                            Height = row["video_height"]?.ToString(),
                            Width = row["video_width"]?.ToString(),
                        },
                    Voice = row.IsNull("voice_fileid")
                    ? null
                    : new Voice
                    {
                        FileId = row["voice_fileid"].ToString(),
                        Duration = int.Parse(row["voice_duration"]?.ToString() ?? "0"),
                        MimeType = row["voice_mime_type"]?.ToString(),
                        FileSize = int.Parse(row["voice_file_size"]?.ToString() ?? "0"),
                    },
                    Venue = row.IsNull("venue")
                        ? null
                        : new Venue
                        {
                            Address = row["venue_address"]?.ToString(),
                            Location = new Location
                            {
                                Latitude = float.Parse(row["venue_latitude"]?.ToString()?.Replace('.', ',') ?? "0"),
                                Longitude = float.Parse(row["venue_longitude"]?.ToString()?.Replace('.', ',') ?? "0"),
                            },
                            Title = row["venue_title"]?.ToString(),
                            FoursquareId = row["venue_foursquare_id"]?.ToString()
                        },
                    Location = row.IsNull("location")
                        ? null
                        : new Location
                        {
                            Latitude = float.Parse(row["location_latitude"]?.ToString()?.Replace('.', ',') ?? "0"),
                            Longitude = float.Parse(row["location_longitude"]?.ToString()?.Replace('.', ',') ?? "0"),
                        },
                    Sticker = row.IsNull("sticker_fileid")
                        ? null
                        : new Sticker
                        {
                            FileId = row["sticker_fileid"].ToString(),
                            FileSize = int.Parse(row["sticker_file_size"]?.ToString() ?? "0"),
                            Height = row["sticker_height"]?.ToString(),
                            Width = row["sticker_width"]?.ToString()
                        }
                };
                if (row["have_photos"].ToString() == "1")
                {
                    var photos = new DataTable();
                    photos.Load(await new OleDbCommand($"select file_id, width, height, file_size FROM telegram_photos where messageid={row["message_id"]}", Connection).ExecuteReaderAsync());
                    mes.Photo=Enumerable.Select(photos.AsEnumerable(), prow=>new PhotoSize
                    {
                        FileId = prow["file_id"]?.ToString(),
                        Width = int.Parse(prow["width"]?.ToString()??"0"),
                        Height = int.Parse(prow["height"]?.ToString() ?? "0"),
                        FileSize = int.Parse(prow["file_size"]?.ToString() ?? "0"),
                    }).ToArray();
                }
                if (row["have_message_entities"].ToString() == "1")
                {
                    var entities = new DataTable();
                    entities.Load(await new OleDbCommand($"select type, offset, length, url FROM telegram_message_entities where messageid={row["message_id"]}", Connection).ExecuteReaderAsync());
                    mes.Entities = Enumerable.Select(entities.AsEnumerable(), erow => new MessageEntity
                    {
                        Type = (MessageEntityType)Enum.Parse(typeof(MessageEntityType),erow["type"]?.ToString()??MessageEntityType.Pre.ToString()),
                        Offset = int.Parse(erow["offset"]?.ToString() ?? "0"),
                        Length = int.Parse(erow["length"]?.ToString() ?? "0"),
                        Url = erow["url"]?.ToString(),
                    }).ToList();
                }
                messagesList.Add(mes);
            }
            return messagesList.OrderBy(x => x.Date).ToList();
        }

        public async Task<List<Chat>> GetOpenedDialogChatsAsync()
        {

            var chats = new DataTable();
            try
            {
                chats.Load(await new OleDbCommand("SELECT id, type, title, username, first_name, last_name FROM telegram_chats where is_dialog_opened=1 and is_closed=0;", Connection).ExecuteReaderAsync());

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
                chats.Load(await new OleDbCommand("SELECT id, type, title, username, first_name, last_name FROM telegram_chats where is_closed=0;", Connection).ExecuteReaderAsync());

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
                var reader = await new OleDbCommand($"SELECT file_id, width, height, file_size FROM telegram_photos where messageid={message.MessageId} and width>=320;", Connection).ExecuteReaderAsync();
                if (!reader.Read()) return null;
                return new PhotoSize
                {
                    FileId = reader[0].ToString(),
                    Width = int.Parse(reader[1].ToString()),
                    Height = int.Parse(reader[2].ToString()),
                    FileSize = string.IsNullOrWhiteSpace(reader[3].ToString()) ? 0 : int.Parse(reader[3].ToString())
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
