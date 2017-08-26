--CRM imitation
CREATE TABLE opportunities (
  id INTEGER NOT NULL
, clientid bigint NOT NULL
, description varchar(4096) NOT NULL
, CONSTRAINT sqlite_master_PK_opportunity PRIMARY KEY (id)
);
--Bot tables

CREATE TABLE telegram_currentbot (
  id bigint NOT NULL
);

CREATE TABLE telegram_offices (
  latitude real NOT NULL
, longitude real NOT NULL
, title varchar NOT NULL
, address varchar NOT NULL
, foursquare_id varchar NULL
);

CREATE TABLE telegram_clients (
  id INTEGER NOT NULL
, phone varchar NULL
, telegramuserid bigint NULL
, CONSTRAINT sqlite_master_PK_client PRIMARY KEY (id)
);

CREATE TABLE telegram_managers (
  managerid bigint NOT NULL
, telegramchatid bigint NULL
, token varchar NULL
);

--Telegram entities
CREATE TABLE telegram_users (
  id bigint NULL
, first_name varchar NULL
, last_name varchar NULL
, username varchar NULL
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
, document_id character varying NULL
, caption character varying NULL
, audio_id character varying NULL
, video_id character varying NULL
, voice_id character varying NULL
, sticker_id character varying NULL

, issended boolean DEFAULT true NOT NULL
);

CREATE TABLE telegram_chats (
  id bigint NOT NULL
, type character varying NOT NULL
, title character varying NULL
, username character varying NULL
, first_name character varying NULL
, last_name character varying NULL
, is_closed boolean DEFAULT false NOT NULL
, is_dialog_opened boolean DEFAULT true NOT NULL
);

CREATE TABLE telegram_messageentities (
  message_id bigint NOT NULL
, type character varying NOT NULL
, "offset" integer NOT NULL
, length integer NOT NULL
, url character varying NULL
);

CREATE TABLE telegram_venues (
  message_id bigint NOT NULL
, latitude real NOT NULL
, longitude real NOT NULL
, title varchar NOT NULL
, address varchar NOT NULL
, foursquare_id varchar NULL
);

CREATE TABLE telegram_locations (
  message_id bigint NOT NULL
, latitude real NOT NULL
, longitude real NOT NULL
);

CREATE TABLE telegram_documents (
	file_id character varying NOT NULL
, file_name character varying NULL
, mime_type character varying NULL
, file_size bigint NULL
);

CREATE TABLE telegram_photos (
  messageid bigint NOT NULL
, file_id character varying NOT NULL
, width bigint NOT NULL
, height bigint NOT NULL
, file_size bigint NULL
);

CREATE TABLE telegram_audios (
  file_id character varying NOT NULL
, duration bigint NOT NULL
, performer character varying NULL
, title character varying NULL
, mime_type character varying NULL
, file_size bigint NULL
);

CREATE TABLE telegram_videos (
  file_id character varying NOT NULL
, width bigint NOT NULL
, height bigint NOT NULL
, duration bigint NOT NULL
, mime_type character varying NULL
, file_size bigint NULL
);

CREATE TABLE telegram_voices (
  file_id character varying NOT NULL
, duration bigint NOT NULL
, mime_type character varying NULL
, file_size bigint NULL
);

CREATE TABLE telegram_stickers (
  file_id character varying NOT NULL
, width bigint NOT NULL
, height bigint NOT NULL
, file_size bigint NULL
);