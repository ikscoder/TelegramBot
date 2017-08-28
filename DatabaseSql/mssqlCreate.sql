--CRM imitation
CREATE TABLE opportunities (
  id INTEGER IDENTITY(1,1) PRIMARY KEY
, clientid bigint NOT NULL
, description varchar(4096) NOT NULL
);
--Bot tables

CREATE TABLE telegram_currentbot (
  id bigint NOT NULL
);

CREATE TABLE telegram_offices (
  latitude real NOT NULL
, longitude real NOT NULL
, title varchar(1024) NOT NULL
, address varchar(1024) NOT NULL
, foursquare_id varchar(1024) NULL
);

CREATE TABLE telegram_clients (
  id INTEGER IDENTITY(1,1) PRIMARY KEY
, phone varchar(1024) NULL
, telegramuserid bigint NULL
);

CREATE TABLE telegram_managers (
  managerid bigint NOT NULL
, telegramchatid bigint NULL
, token varchar(1024) NULL
);

--Telegram entities
CREATE TABLE telegram_users (
  id bigint NULL
, first_name varchar(1024) NULL
, last_name varchar(1024) NULL
, username varchar(1024) NULL
);

CREATE TABLE telegram_messages (
  message_id bigint NOT NULL
, from_id bigint NOT NULL
, date bigint NOT NULL
, chat_id bigint NOT NULL
, text varchar(4096) NULL
, forward_from bigint NULL
, forward_date bigint NULL
, reply_to_message_id bigint NULL
, pinned_message_id bigint NULL
, document_id varchar(1024) NULL
, caption varchar(1024) NULL
, audio_id varchar(1024) NULL
, video_id varchar(1024) NULL
, voice_id varchar(1024) NULL
, sticker_id varchar(1024) NULL
, isreply bit default 0 not null
, ispinned bit default 0 not null
, issended bit DEFAULT 1 NOT NULL
);

CREATE TABLE telegram_chats (
  id bigint NOT NULL
, type varchar(1024) NOT NULL
, title varchar(1024) NULL
, username varchar(1024) NULL
, first_name varchar(1024) NULL
, last_name varchar(1024) NULL
, is_closed bit DEFAULT 0 NOT NULL
, is_dialog_opened bit DEFAULT 1 NOT NULL
);

CREATE TABLE telegram_messageentities (
  message_id bigint NOT NULL
, type varchar(1024) NOT NULL
, "offset" integer NOT NULL
, length integer NOT NULL
, url varchar(1024) NULL
);

CREATE TABLE telegram_venues (
  message_id bigint NOT NULL
, latitude real NOT NULL
, longitude real NOT NULL
, title varchar(1024) NOT NULL
, address varchar(1024) NOT NULL
, foursquare_id varchar(1024) NULL
);

CREATE TABLE telegram_locations (
  message_id bigint NOT NULL
, latitude real NOT NULL
, longitude real NOT NULL
);

CREATE TABLE telegram_documents (
	file_id varchar(1024) NOT NULL
, file_name varchar(1024) NULL
, mime_type varchar(1024) NULL
, file_size bigint NULL
);

CREATE TABLE telegram_photos (
  messageid bigint NOT NULL
, file_id varchar(1024) NOT NULL
, width bigint NOT NULL
, height bigint NOT NULL
, file_size bigint NULL
);

CREATE TABLE telegram_audios (
  file_id varchar(1024) NOT NULL
, duration bigint NOT NULL
, performer varchar(1024) NULL
, title varchar(1024) NULL
, mime_type varchar(1024) NULL
, file_size bigint NULL
);

CREATE TABLE telegram_videos (
  file_id varchar(1024) NOT NULL
, width bigint NOT NULL
, height bigint NOT NULL
, duration bigint NOT NULL
, mime_type varchar(1024) NULL
, file_size bigint NULL
);

CREATE TABLE telegram_voices (
  file_id varchar(1024) NOT NULL
, duration bigint NOT NULL
, mime_type varchar(1024) NULL
, file_size bigint NULL
);

CREATE TABLE telegram_stickers (
  file_id varchar(1024) NOT NULL
, width bigint NOT NULL
, height bigint NOT NULL
, file_size bigint NULL
);