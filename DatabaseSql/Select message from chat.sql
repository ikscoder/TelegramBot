USE [TelegramCRMBot]
GO

SELECT telegram_messages.message_id

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
  where chat_id=178515919 and isreply=0
GO


