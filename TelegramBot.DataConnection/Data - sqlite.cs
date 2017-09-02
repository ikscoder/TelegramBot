using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
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
                var factory = (SQLiteFactory)DbProviderFactories.GetFactory("System.Data.SQLite");
                var connection = (SQLiteConnection)factory.CreateConnection();
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

        private SQLiteConnection Connection { get; }

        private Data(string connectionString)
        {
            var factory = (SQLiteFactory)DbProviderFactories.GetFactory("System.Data.SQLite");
            Connection = (SQLiteConnection)factory.CreateConnection();
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
                await new SQLiteCommand($"DELETE FROM [telegram_currentbot];", Connection).ExecuteNonQueryAsync();
                var command = new SQLiteCommand("INSERT INTO [telegram_currentbot] ([id]) VALUES (@ID);", Connection);
                command.Parameters.Add("@ID", DbType.Int32);
                command.Parameters["@ID"].Value = bot.Id;
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
                var test = new SQLiteCommand($"select * from [telegram_chats] where id={chat.Id}", Connection);
                bool isUpdate = test.ExecuteReader().Read();
                var command = new SQLiteCommand(Connection);
                if (isUpdate)
                {
                    command.CommandText =
                        "UPDATE [telegram_chats] SET [type] = @type, [title] = @title, [username] = @username, [first_name] = @first_name, [last_name] = @last_name";
                    if (isClosed.HasValue)
                        command.CommandText += ", [is_closed] = @is_closed";
                    if (isDialogOpened.HasValue)
                        command.CommandText += ", [is_dialog_opened] = @is_dialog_opened";
                    command.CommandText += " WHERE id=@id;";
                }
                else
                {
                    command.CommandText = "INSERT INTO [telegram_chats] ([id] ,[type] ,[title] ,[username] ,[first_name] ,[last_name] ,[is_closed], [is_dialog_opened]) VALUES (@id, @type, @title, @username, @first_name, @last_name, @is_closed, @is_dialog_opened);";
                }
                command.Parameters.Add("@type", DbType.String);
                command.Parameters["@type"].Value = chat.Type;
                command.Parameters.Add("@title", DbType.String);
                command.Parameters["@title"].Value = chat.Title;
                command.Parameters.Add("@username", DbType.String);
                command.Parameters["@username"].Value = chat.Username;
                command.Parameters.Add("@first_name", DbType.String);
                command.Parameters["@first_name"].Value = chat.FirstName;
                command.Parameters.Add("@last_name", DbType.String);
                command.Parameters["@last_name"].Value = chat.LastName;
                if (!isUpdate || isClosed.HasValue)
                {
                    command.Parameters.Add("@is_closed", DbType.Boolean);
                    command.Parameters["@is_closed"].Value = isClosed ?? false;
                }
                if (!isUpdate || isDialogOpened.HasValue)
                {
                    command.Parameters.Add("@is_dialog_opened", DbType.Boolean);
                    command.Parameters["@is_dialog_opened"].Value = isDialogOpened ?? false;
                }
                command.Parameters.Add("@id", DbType.Int64);
                command.Parameters["@id"].Value = chat.Id;
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
                var test = new SQLiteCommand($"select * from [telegram_users] where [id]={user.Id}", Connection);
                var command = new SQLiteCommand(Connection)
                {
                    CommandText =
                        test.ExecuteReader().Read()
                            ? "UPDATE [telegram_users] SET [first_name] = @first_name, [last_name] = @last_name, [username] = @username WHERE [id] = @id;"
                            : "INSERT INTO [telegram_users] ([id],[first_name],[last_name],[username]) VALUES(@id, @first_name, @last_name, @username);"
                };
                command.Parameters.Add("@id", DbType.Int64);
                command.Parameters["@id"].Value = user.Id;
                command.Parameters.Add("@first_name", DbType.String);
                command.Parameters["@first_name"].Value = user.FirstName;
                command.Parameters.Add("@last_name", DbType.String);
                command.Parameters["@last_name"].Value = user.LastName;
                command.Parameters.Add("@username", DbType.String);
                command.Parameters["@username"].Value = user.Username;
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Log.Add(new Log.LogMessage(Log.MessageType.ERROR, "InsertOrUpdateUser: " + e.Message));
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

                var test = new SQLiteCommand($"select * from [telegram_clients] where [telegramuserid]={user.Id}", Connection);
                var command = new SQLiteCommand(Connection)
                {
                    CommandText =
                        (test.ExecuteReader().Read() && contact != null)
                            ? "UPDATE [telegram_clients] SET [phone] = @phone WHERE [telegramuserid] = @telegramuserid;"
                            : "INSERT INTO [telegram_clients] ([phone],[telegramuserid]) VALUES(@phone, @telegramuserid);"
                };
                command.Parameters.Add("@phone", DbType.String);
                command.Parameters["@phone"].Value = contact?.PhoneNumber;
                command.Parameters.Add("@telegramuserid", DbType.Int64);
                command.Parameters["@telegramuserid"].Value = user.Id;
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
                var reader = await new SQLiteCommand($"SELECT [id] FROM [telegram_clients] where [telegramuserid]={user.Id};", Connection).ExecuteReaderAsync();
                if (!reader.Read()) return;
                var command = new SQLiteCommand("INSERT INTO [opportunities] ([clientid],[description]) VALUES(@clientid, @description);", Connection);
                command.Parameters.Add("@clientid", DbType.Int64);
                command.Parameters["@clientid"].Value = reader.GetInt32(0);
                command.Parameters.Add("@description", DbType.String);
                command.Parameters["@description"].Value = description;
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
                offices.Load(await new SQLiteCommand("SELECT [latitude], [longitude], [title], [address], [foursquare_id] FROM [telegram_offices];", Connection).ExecuteReaderAsync());
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
                opportunities.Load(await new SQLiteCommand("SELECT [id], [clientid], [description] FROM [opportunities];", Connection).ExecuteReaderAsync());
                var result = new List<Opportunity>(opportunities.AsEnumerable().Count());
                foreach (var row in opportunities.AsEnumerable())
                {
                    var o = new Opportunity
                    {
                        Description = row[2].ToString(),
                        Contact = new Contact()
                    };
                    var command = new SQLiteCommand($"SELECT [phone], [telegramuserid] FROM [telegram_clients] where [id]={row[1]} limit 1;", Connection);
                    var reader = await command.ExecuteReaderAsync();
                    if (reader.Read())
                    {

                        o.Contact.PhoneNumber = reader.GetString(0);
                        var userc = new SQLiteCommand($"SELECT [first_name], [last_name] FROM [telegram_users] where [id]={reader[1]} limit 1;", Connection);
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
                var command = new SQLiteCommand($"SELECT [is_closed] FROM [telegram_chats] where [id]={chat.Id};", Connection);
                var reader = await command.ExecuteReaderAsync();
                return !reader.Read() || bool.Parse(reader[0].ToString());
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
                var command = new SQLiteCommand($"SELECT [is_dialog_opened] FROM [telegram_chats] where [id]={chat.Id};", Connection);
                var reader = await command.ExecuteReaderAsync();
                return reader.Read() && bool.Parse(reader[0].ToString());
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
                var command = new SQLiteCommand($"SELECT * FROM [telegram_managers] where [telegramchatid]={chat.Id};", Connection);
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
                var command = new SQLiteCommand("INSERT INTO [telegram_audios] ([file_id], [duration], [performer], [title], [mime_type], [file_size]) VALUES (@file_id, @duration, @performer, @title, @mime_type, @file_size);", Connection);
                command.Parameters.Add("@file_id", DbType.String);
                command.Parameters["@file_id"].Value = audio.FileId;
                command.Parameters.Add("@duration", DbType.Int32);
                command.Parameters["@duration"].Value = audio.Duration;
                command.Parameters.Add("@performer", DbType.String);
                command.Parameters["@performer"].Value = audio.Performer;
                command.Parameters.Add("@title", DbType.String);
                command.Parameters["@title"].Value = audio.Title;
                command.Parameters.Add("@mime_type", DbType.String);
                command.Parameters["@mime_type"].Value = audio.MimeType;
                command.Parameters.Add("@file_size", DbType.Int32);
                command.Parameters["@file_size"].Value = audio.FileSize;
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
                var command = new SQLiteCommand("INSERT INTO [telegram_documents] ([file_id], [file_name], [mime_type], [file_size]) VALUES (@file_id, @file_name, @mime_type, @file_size);", Connection);
                command.Parameters.Add("@file_id", DbType.String);
                command.Parameters["@file_id"].Value = document.FileId;
                command.Parameters.Add("@file_name", DbType.String);
                command.Parameters["@file_name"].Value = document.FileName;
                command.Parameters.Add("@mime_type", DbType.String);
                command.Parameters["@mime_type"].Value = document.MimeType;
                command.Parameters.Add("@file_size", DbType.Int32);
                command.Parameters["@file_size"].Value = document.FileSize;
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
                var command = new SQLiteCommand("INSERT INTO [telegram_locations] ([message_id], [latitude], [longitude]) VALUES (@message_id, @latitude, @longitude);", Connection);
                command.Parameters.Add("@message_id", DbType.Int32);
                command.Parameters["@message_id"].Value = message.MessageId;
                command.Parameters.Add("@latitude", DbType.Double);
                command.Parameters["@latitude"].Value = message.Location.Latitude;
                command.Parameters.Add("@longitude", DbType.Double);
                command.Parameters["@longitude"].Value = message.Location.Longitude;
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
                    var command = new SQLiteCommand("INSERT INTO [telegram_messageentities] ([message_id], [type], [offset], [length], [url]) VALUES (@message_id, @type, @offset, @length,@url);", Connection);
                    command.Parameters.Add("@message_id", DbType.Int32);
                    command.Parameters["@message_id"].Value = message.MessageId;
                    command.Parameters.Add("@type", DbType.String);
                    command.Parameters["@type"].Value = entity.Type;
                    command.Parameters.Add("@offset", DbType.Int32);
                    command.Parameters["@offset"].Value = entity.Offset;
                    command.Parameters.Add("@length", DbType.Int32);
                    command.Parameters["@length"].Value = entity.Length;
                    command.Parameters.Add("@url", DbType.String);
                    command.Parameters["@url"].Value = entity.Url;
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
                    var command = new SQLiteCommand("INSERT INTO [telegram_photos] ([messageid], [file_id], [width], [height], [file_size]) VALUES (@message_id, @file_id, @width, @height,@file_size);", Connection);
                    command.Parameters.Add("@message_id", DbType.Int32);
                    command.Parameters["@message_id"].Value = message.MessageId;
                    command.Parameters.Add("@file_id", DbType.String);
                    command.Parameters["@file_id"].Value = photo.FileId;
                    command.Parameters.Add("@width", DbType.Int32);
                    command.Parameters["@width"].Value = photo.Width;
                    command.Parameters.Add("@height", DbType.Int32);
                    command.Parameters["@height"].Value = photo.Height;
                    command.Parameters.Add("@file_size", DbType.Int32);
                    command.Parameters["@file_size"].Value = photo.FileSize;
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
                var command = new SQLiteCommand("INSERT INTO [telegram_stickers] ( [file_id], [width], [height], [file_size]) VALUES (@file_id, @width, @height,@file_size);", Connection);
                command.Parameters.Add("@file_id", DbType.String);
                command.Parameters["@file_id"].Value = sticker.FileId;
                command.Parameters.Add("@width", DbType.Int32);
                command.Parameters["@width"].Value = sticker.Width;
                command.Parameters.Add("@height", DbType.Int32);
                command.Parameters["@height"].Value = sticker.Height;
                command.Parameters.Add("@file_size", DbType.Int32);
                command.Parameters["@file_size"].Value = sticker.FileSize;
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
                var command = new SQLiteCommand("INSERT INTO [telegram_venues] ( [message_id], [latitude], [longitude], [title], [address], [foursquare_id]) VALUES (@message_id, @latitude, @longitude, @title, @address, @foursquare_id);", Connection);
                command.Parameters.Add("@message_id", DbType.Int32);
                command.Parameters["@message_id"].Value = message.MessageId;
                command.Parameters.Add("@latitude", DbType.Double);
                command.Parameters["@latitude"].Value = message.Venue.Location.Latitude;
                command.Parameters.Add("@longitude", DbType.Double);
                command.Parameters["@longitude"].Value = message.Venue.Location.Longitude;
                command.Parameters.Add("@title", DbType.String);
                command.Parameters["@title"].Value = message.Venue.Title;
                command.Parameters.Add("@address", DbType.String);
                command.Parameters["@address"].Value = message.Venue.Address;
                command.Parameters.Add("@foursquare_id", DbType.String);
                command.Parameters["@foursquare_id"].Value = message.Venue.FoursquareId;
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

            var command = new SQLiteCommand("INSERT INTO [telegram_videos] ( [file_id], [width], [height], [duration], [mime_type], [file_size]) VALUES ( @file_id, @width, @height, @duration, @mime_type, @file_size);", Connection);
            try
            {
                command.Parameters.Add("@file_id", DbType.String);
                command.Parameters["@file_id"].Value = video.FileId;
                command.Parameters.Add("@width", DbType.Int32);
                command.Parameters["@width"].Value = video.Width;
                command.Parameters.Add("@height", DbType.Int32);
                command.Parameters["@height"].Value = video.Height;
                command.Parameters.Add("@duration", DbType.Int32);
                command.Parameters["@duration"].Value = video.Duration;
                command.Parameters.Add("@mime_type", DbType.String);
                command.Parameters["@mime_type"].Value = video.MimeType;
                command.Parameters.Add("@file_size", DbType.Int32);
                command.Parameters["@file_size"].Value = video.FileSize;
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
                var command = new SQLiteCommand("INSERT INTO [telegram_voices] ( [file_id], [duration], [mime_type], [file_size]) VALUES ( @file_id, @duration, @mime_type, @file_size);", Connection);
                command.Parameters.Add("@file_id", DbType.String);
                command.Parameters["@file_id"].Value = voice.FileId;
                command.Parameters.Add("@duration", DbType.Int32);
                command.Parameters["@duration"].Value = voice.Duration;
                command.Parameters.Add("@mime_type", DbType.String);
                command.Parameters["@mime_type"].Value = voice.MimeType;
                command.Parameters.Add("@file_size", DbType.Int32);
                command.Parameters["@file_size"].Value = voice.FileSize;
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
                var command = new SQLiteCommand("INSERT INTO  [telegram_messages] ([message_id], [from_id], [date], [chat_id], [text], [forward_from], [forward_date], [reply_to_message_id], [pinned_message_id], [document_id], [caption], [audio_id], [video_id], [voice_id], [sticker_id], [issended]) VALUES(@message_id, @from_id, @date, @chat_id, @text, @forward_from, @forward_date, @reply_to_message_id, @pinned_message_id, @document_id, @caption, @audio_id, @video_id, @voice_id, @sticker_id, @issended);", Connection);
                command.Parameters.Add("@message_id", DbType.Int32);
                command.Parameters["@message_id"].Value = message.MessageId;
                command.Parameters.Add("@from_id", DbType.Int32);
                command.Parameters["@from_id"].Value = message.From.Id;
                command.Parameters.Add("@date", DbType.Int32);
                command.Parameters["@date"].Value = message.Date.ToUnixTime();
                command.Parameters.Add("@chat_id", DbType.Int64);
                command.Parameters["@chat_id"].Value = message.Chat.Id;
                command.Parameters.Add("@text", DbType.String);
                command.Parameters["@text"].Value = message.Text;
                command.Parameters.Add("@forward_from", DbType.Int64);
                command.Parameters["@forward_from"].Value = message.ForwardFrom?.Id;
                command.Parameters.Add("@forward_date", DbType.Int64);
                command.Parameters["@forward_date"].Value = message.ForwardDate?.ToUnixTime();     
                command.Parameters.Add("@reply_to_message_id", DbType.Int64);
                command.Parameters["@reply_to_message_id"].Value = message.ReplyToMessage?.MessageId;
                command.Parameters.Add("@pinned_message_id", DbType.Int64);
                command.Parameters["@pinned_message_id"].Value = message.PinnedMessage?.MessageId;
                command.Parameters.Add("@document_id", DbType.String);
                command.Parameters["@document_id"].Value = message.Document?.FileId;
                command.Parameters.Add("@caption", DbType.String);
                command.Parameters["@caption"].Value = message.Caption;
                command.Parameters.Add("@audio_id", DbType.String);
                command.Parameters["@audio_id"].Value = message.Audio?.FileId;
                command.Parameters.Add("@video_id", DbType.String);
                command.Parameters["@video_id"].Value = message.Video?.FileId;
                command.Parameters.Add("@voice_id", DbType.String);
                command.Parameters["@voice_id"].Value = message.Voice?.FileId;
                command.Parameters.Add("@sticker_id", DbType.String);
                command.Parameters["@sticker_id"].Value = message.Sticker?.FileId;
                command.Parameters.Add("@issended", DbType.Boolean);
                command.Parameters["@issended"].Value = isSended ?? true;
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
                messages.Load(await new SQLiteCommand($"SELECT [message_id], [from_id], [date], [chat_id], [text] FROM [telegram_messages] where not [issended];", Connection).ExecuteReaderAsync());

                var command = new SQLiteCommand("DELETE FROM [telegram_messages] WHERE [message_id] < 0;", Connection);
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
                var command = new SQLiteCommand("SELECT [managerid], [telegramchatid], [token] FROM [telegram_managers] where [token]=@token;", Connection);
                command.Parameters.Add("@token", DbType.String);
                command.Parameters["@token"].Value = token;
                var reader = await command.ExecuteReaderAsync();
                if (!reader.Read()) return false;
                var updcommand = new SQLiteCommand("UPDATE [telegram_managers] SET [telegramchatid] =@telegramchatid, [token] is null where [token]=@token; ", Connection);
                updcommand.Parameters.Add("@token", DbType.String);
                updcommand.Parameters["@token"].Value = token;
                updcommand.Parameters.Add("@telegramchatid", DbType.Int64);
                updcommand.Parameters["@telegramchatid"].Value = chat.Id;
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
                var command = new SQLiteCommand("UPDATE [telegram_managers] SET [telegramchatid] = NULL, [token] =@token WHERE [managerid]=@managerId; ", Connection);
                command.Parameters.Add("@token", DbType.String);
                command.Parameters["@token"].Value = token;
                command.Parameters.Add("@managerid", DbType.Int64);
                command.Parameters["@managerid"].Value = managerId;
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
                var reader = await new SQLiteCommand($"SELECT [id], [first_name], [last_name], [username] FROM [telegram_users] WHERE [id]={id};", Connection).ExecuteReaderAsync();
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
                var reader = await new SQLiteCommand("SELECT [id] FROM [telegram_currentbot];", Connection).ExecuteReaderAsync();
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
                tables.Load(await new SQLiteCommand($"SELECT name FROM sqlite_master WHERE type='table' and not name='sqlite_sequence';", Connection).ExecuteReaderAsync());
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
                offices.Load(await new SQLiteCommand($"SELECT * FROM {name};", Connection).ExecuteReaderAsync());
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
                managerIds.Load(await new SQLiteCommand($"SELECT [managerid] FROM [telegram_managers];", Connection).ExecuteReaderAsync());
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
                chatids.Load(await new SQLiteCommand($"SELECT [telegramchatid] FROM [telegram_managers] where [telegramchatid] IS NOT NULL;", Connection).ExecuteReaderAsync());

                foreach (var chatid in chatids.AsEnumerable())
                {
                    var chats = new DataTable();
                    chats.Load(await new SQLiteCommand($"SELECT [id], [type], [title], [username], [first_name], [last_name] FROM [telegram_chats] where [id]={chatid[0]};", Connection).ExecuteReaderAsync());

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
                messages.Load(await new SQLiteCommand($"SELECT [message_id], [from_id], [date], [text] FROM [telegram_messages] where [chat_id]={chat.Id};", Connection).ExecuteReaderAsync());
                users.Load(await new SQLiteCommand("SELECT [id], [first_name], [last_name], [username] FROM [telegram_users];", Connection).ExecuteReaderAsync());

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
                Text = row[3].ToString()
            }).ToList();
        }

        public async Task<List<Chat>> GetOpenedDialogChatsAsync()
        {

            var chats = new DataTable();
            try
            {
                chats.Load(await new SQLiteCommand("SELECT [id], [type], [title], [username], [first_name], [last_name] FROM [telegram_chats] where [is_dialog_opened] and not [is_closed];", Connection).ExecuteReaderAsync());

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
                chats.Load(await new SQLiteCommand("SELECT [id], [type], [title], [username], [first_name], [last_name] FROM [telegram_chats] where not [is_closed];", Connection).ExecuteReaderAsync());

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

        #endregion
    }
}
