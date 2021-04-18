using VkNet.Model.Attachments;

namespace fullvk.Methods
{
	public class PopularMusic
	{
		public Response response { get; set; }

		public class Response
		{
			public Item[] items { get; set; }
			public string next_from { get; set; }
			public Profile[] profiles { get; set; }
			public Group[] groups { get; set; }

			public Block block { get; set; }
		}

		public class Item
		{
			public string title { get; set; }
			public string subtitle { get; set; }
			public string type { get; set; }
			public int count { get; set; }
			public string source { get; set; }
			public string id { get; set; }
			public string next_from { get; set; }
			public Audio[] audios { get; set; }
			public Thumb1[] thumbs { get; set; }
			public Playlist[] playlists { get; set; }
			public string url { get; set; }
		}

		public class Block : Item
		{
			public Item[] items { get; set; }
		}

		public class Audio
		{
			public string artist { get; set; }
			public long? id { get; set; }
			public long? owner_id { get; set; }
			public string title { get; set; }
			public int duration { get; set; }
			public string access_key { get; set; }
			public Ads ads { get; set; }
			public bool is_licensed { get; set; }
			public string track_code { get; set; }
			public string url { get; set; }
			public int date { get; set; }
			public Album album { get; set; }
			public Main_Artists[] main_artists { get; set; }
			public int no_search { get; set; }
			public Featured_Artists[] featured_artists { get; set; }
			public string subtitle { get; set; }
			public bool is_explicit { get; set; }
			public int content_restricted { get; set; }
			public bool is_focus_track { get; set; }
			public int genre_id { get; set; }
			public int lyrics_id { get; set; }
		}

		public class Ads
		{
			public string content_id { get; set; }
			public string duration { get; set; }
			public string account_age_type { get; set; }
			public string puid1 { get; set; }
			public string puid22 { get; set; }
		}

		public class Album
		{
			public int id { get; set; }
			public string title { get; set; }
			public int owner_id { get; set; }
			public string access_key { get; set; }
			public Thumb thumb { get; set; }
		}

		public class Thumb
		{
			public int width { get; set; }
			public int height { get; set; }
			public string photo_34 { get; set; }
			public string photo_68 { get; set; }
			public string photo_135 { get; set; }
			public string photo_270 { get; set; }
			public string photo_300 { get; set; }
			public string photo_600 { get; set; }
		}

		public class Main_Artists
		{
			public string name { get; set; }
			public bool is_followed { get; set; }
			public bool can_follow { get; set; }
			public string domain { get; set; }
			public string id { get; set; }
		}

		public class Featured_Artists
		{
			public string name { get; set; }
			public bool is_followed { get; set; }
			public bool can_follow { get; set; }
			public string domain { get; set; }
			public string id { get; set; }
		}

		public class Thumb1
		{
			public int width { get; set; }
			public int height { get; set; }
			public string photo_34 { get; set; }
			public string photo_68 { get; set; }
			public string photo_135 { get; set; }
			public string photo_270 { get; set; }
			public string photo_300 { get; set; }
			public string photo_600 { get; set; }
		}

		public class Playlist
		{
			public long? id { get; set; }
			public long? owner_id { get; set; }
			public int type { get; set; }
			public string title { get; set; }
			public string description { get; set; }
			public int count { get; set; }
			public int followers { get; set; }
			public int plays { get; set; }
			public int create_time { get; set; }
			public int update_time { get; set; }
			public Genre[] genres { get; set; }
			public int year { get; set; }
			public Original original { get; set; }
			public Photo photo { get; set; }
			public string access_key { get; set; }
			public bool is_explicit { get; set; }
			public Main_Artists1[] main_artists { get; set; }
			public string album_type { get; set; }
			public bool is_blocked { get; set; }
			public Meta meta { get; set; }
			public Restriction restriction { get; set; }
		}

		public class Original
		{
			public int playlist_id { get; set; }
			public int owner_id { get; set; }
			public string access_key { get; set; }
		}

		public class Photo
		{
			public int width { get; set; }
			public int height { get; set; }
			public string photo_34 { get; set; }
			public string photo_68 { get; set; }
			public string photo_135 { get; set; }
			public string photo_270 { get; set; }
			public string photo_300 { get; set; }
			public string photo_600 { get; set; }
		}

		public class Meta
		{
			public string view { get; set; }
		}

		public class Restriction
		{
			public string title { get; set; }
			public Button button { get; set; }
			public Icon[] icons { get; set; }
			public string text { get; set; }
		}

		public class Button
		{
			public Action action { get; set; }
			public string title { get; set; }
		}

		public class Action
		{
			public string target { get; set; }
			public string type { get; set; }
			public string url { get; set; }
		}

		public class Icon
		{
			public int height { get; set; }
			public string url { get; set; }
			public int width { get; set; }
		}

		public class Genre
		{
			public int id { get; set; }
			public string name { get; set; }
		}

		public class Main_Artists1
		{
			public string name { get; set; }
			public bool is_followed { get; set; }
			public bool can_follow { get; set; }
			public string domain { get; set; }
			public string id { get; set; }
		}

		public class Profile
		{
			public int id { get; set; }
			public string first_name { get; set; }
			public string last_name { get; set; }
			public bool is_closed { get; set; }
			public bool can_access_closed { get; set; }
			public string photo_50 { get; set; }
			public string photo_100 { get; set; }
			public string photo_200 { get; set; }
			public string first_name_gen { get; set; }
			public string last_name_gen { get; set; }
			public string deactivated { get; set; }
		}

		public class Group
		{
			public long? id { get; set; }
			public string name { get; set; }
			public string screen_name { get; set; }
			public int is_closed { get; set; }
			public string type { get; set; }
			public int is_admin { get; set; }
			public int is_member { get; set; }
			public int is_advertiser { get; set; }
			public string photo_50 { get; set; }
			public string photo_100 { get; set; }
			public string photo_200 { get; set; }
		}
	}


	public class DailyRec
	{
		public Response response { get; set; }

		public class Response
		{
			public Audio[] audios { get; set; }
		}

	}
}