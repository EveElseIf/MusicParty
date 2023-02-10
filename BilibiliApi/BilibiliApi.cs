using System.Text.Json;
using System.Text.Json.Nodes;

namespace MusicParty.MusicApi.Bilibili;

public class BilibiliApi : IMusicApi
{
    private readonly string _sessdata;
    private readonly string _phoneNo;
    private readonly HttpClient _http = new();
    public string ServiceName => "Bilibili";

    public BilibiliApi(string sessdata, string phoneNo)
    {
        _sessdata = sessdata;
        _phoneNo = phoneNo;
    }

    public void Login()
    {
        Console.WriteLine("You are going to login your Bilibili Account...");
        if (!string.IsNullOrEmpty(_sessdata))
        {
            SESSDATALogin().Wait();
        }
        else
        {
            if (string.IsNullOrEmpty(_phoneNo))
                throw new Exception(
                    "You must set SESSDATA or phone number of your bilibili account in appsettings.json.");
            QRCodeLogin().Wait();
        }

        Console.WriteLine("Login success!");
    }

    private async Task SESSDATALogin()
    {
        _http.DefaultRequestHeaders.Add("Cookie", $"SESSDATA={_sessdata};");
        var resp = await _http.GetStringAsync("https://api.bilibili.com/nav");
        var j = JsonNode.Parse(resp)!;
        if (j["code"]!.GetValue<int>() != 0)
            throw new Exception($"Login failed, message: {resp}");
        var resp2 = await _http.GetAsync("https://www.bilibili.com");
        var cookies = resp2.Headers.GetValues("Set-Cookie");
        _http.DefaultRequestHeaders.Add("Cookie", cookies);
    }

    private async Task QRCodeLogin()
    {
        throw new NotImplementedException();
    }

    public async Task<Music> GetMusicByIdAsync(string id)
    {
        var resp = await _http.GetStringAsync($"https://api.bilibili.com/x/web-interface/view?bvid={id}");
        var j = JsonSerializer.Deserialize<BVQueryJson.RootObject>(resp);
        if (j is null || j.code != 0 || j.data is null)
            throw new Exception($"Unable to get playable music, message: {resp}");
        return new Music($"{j.data.bvid},{j.data.cid}", j.data.title, new[] { j.data.owner.name });
    }

    public async Task<IEnumerable<Music>> SearchMusicByNameAsync(string name)
    {
        throw new NotImplementedException();
    }

    public async Task<PlayableMusic> GetPlayableMusicAsync(Music music)
    {
        var ids = music.Id.Split(',');
        var resp = await _http.GetStringAsync(
            $"https://api.bilibili.com/x/player/playurl?bvid={ids[0]}&cid={ids[1]}&fnval=16");
        var j = JsonSerializer.Deserialize<PlayUrlJson.RootObject>(resp);
        if (j is null || j.code != 0 || j.data is null)
            throw new Exception($"Unable to get playable music, message: {resp}");

        return new PlayableMusic(music)
        {
            Url = $"/musicproxy?timestamp={DateTimeOffset.Now.ToUnixTimeSeconds()}",
            Length = j.data.dash.duration * 1000,
            NeedProxy = true, TargetUrl = j.data.dash.audio.OrderByDescending(x => x.id).First().baseUrl,
            Referer = "https://www.bilibili.com"
        };
    }

    public async Task<IEnumerable<MusicServiceUser>> SearchUserAsync(string keyword)
    {
        var resp = await _http.GetStringAsync(
            $"https://api.bilibili.com/x/web-interface/search/type?search_type=bili_user&keyword={keyword}");
        var j = JsonSerializer.Deserialize<SearchUserJson.RootObject>(resp);
        if (j is null || j.code != 0)
            throw new Exception($"Search user failed, message: {resp}");
        if (j.data?.result is null)
            return Array.Empty<MusicServiceUser>();
        return j.data.result.Select(x => new MusicServiceUser(x.mid.ToString(), x.uname));
    }

    public async Task<IEnumerable<PlayList>> GetUserPlayListAsync(string userIdentifier)
    {
        var resp = await _http.GetStringAsync(
            $"https://api.bilibili.com/x/v3/fav/folder/created/list-all?type=2&up_mid={userIdentifier}");
        var j = JsonSerializer.Deserialize<UserFavsJson.RootObject>(resp);
        if (j is null || j.code != 0)
            throw new Exception($"Unable to get user playlist, message: ${resp}");
        if (j.data?.list is null)
            return Array.Empty<PlayList>();
        return j.data.list.Select(x => new PlayList(x.id.ToString(), x.title));
    }

    public async Task<IEnumerable<Music>> GetMusicsByPlaylistAsync(string id, int offset = 0)
    {
        var resp = await _http.GetStringAsync(
            $"https://api.bilibili.com/x/v3/fav/resource/list?platform=web&media_id={id}&ps=10&pn={offset / 10 + 1}");
        var j = JsonSerializer.Deserialize<FavDetailJson.RootObject>(resp);
        if (j is null || j.code != 0)
            throw new Exception($"Unable to get playlist musics, message: {resp}");
        if (j.data?.medias is null)
            return Array.Empty<Music>();
        return j.data.medias.Where(x => x.title != "已失效视频" && x.type == 2)
            .Select(x => new Music(x.bvid, x.title, new[] { x.upper.name }));
    }

    #region JsonClasses

    private class SearchUserJson
    {
        public class RootObject
        {
            public int code { get; init; }
            public Data? data { get; init; }
        }

        public class Data
        {
            public Result[]? result { get; init; }
        }

        public class Result
        {
            public long mid { get; init; }
            public string uname { get; init; }
        }
    }

    private class UserFavsJson
    {
        public record RootObject(
            long code,
            string message,
            long ttl,
            Data? data
        );

        public record Data(
            long count,
            List[]? list,
            object season
        );

        public record List(
            long id,
            long fid,
            long mid,
            long attr,
            string title,
            long fav_state,
            long media_count
        );
    }

    private class FavDetailJson
    {
        public record RootObject(
            int code,
            string message,
            int ttl,
            Data? data
        );

        public record Data(
            Info info,
            Medias[]? medias,
            bool has_more
        );

        public record Info(
            int id,
            int fid,
            int mid,
            int attr,
            string title,
            string cover,
            Upper upper,
            int cover_type,
            Cnt_info cnt_info,
            int type,
            string intro,
            int ctime,
            int mtime,
            int state,
            int fav_state,
            int like_state,
            int media_count
        );

        public record Upper(
            int mid,
            string name,
            string face,
            bool followed,
            int vip_type,
            int vip_statue
        );

        public record Cnt_info(
            int collect,
            int play,
            int thumb_up,
            int share
        );

        public record Medias(
            int id,
            int type,
            string title,
            string cover,
            string intro,
            int page,
            int duration,
            Upper1 upper,
            int attr,
            Cnt_info1 cnt_info,
            string link,
            int ctime,
            int pubtime,
            int fav_time,
            string bv_id,
            string bvid,
            object season,
            object ogv,
            Ugc ugc
        );

        public record Upper1(
            int mid,
            string name,
            string face
        );

        public record Cnt_info1(
            int collect,
            int play,
            int danmaku
        );

        public record Ugc(
            int first_cid
        );
    }

    private class BVQueryJson
    {
        public record RootObject(
            int code,
            string message,
            int ttl,
            Data? data
        );

        public record Data(
            string bvid,
            int aid,
            int videos,
            int tid,
            string tname,
            int copyright,
            string pic,
            string title,
            int pubdate,
            int ctime,
            string desc,
            Desc_v2[] desc_v2,
            int state,
            int duration,
            Rights rights,
            Owner owner,
            Stat stat,
            string dynamic,
            int cid,
            Dimension dimension,
            object premiere,
            int teenage_mode,
            bool is_chargeable_season,
            bool is_story,
            bool no_cache,
            Pages[] pages,
            Subtitle subtitle,
            bool is_season_display,
            User_garb user_garb,
            Honor_reply honor_reply,
            string like_icon,
            bool need_jump_bv
        );

        public record Desc_v2(
            string raw_text,
            int type,
            int biz_id
        );

        public record Rights(
            int bp,
            int elec,
            int download,
            int movie,
            int pay,
            int hd5,
            int no_reprint,
            int autoplay,
            int ugc_pay,
            int is_cooperation,
            int ugc_pay_preview,
            int no_background,
            int clean_mode,
            int is_stein_gate,
            int is_360,
            int no_share,
            int arc_pay,
            int free_watch
        );

        public record Owner(
            int mid,
            string name,
            string face
        );

        public record Stat(
            int aid,
            int view,
            int danmaku,
            int reply,
            int favorite,
            int coin,
            int share,
            int now_rank,
            int his_rank,
            int like,
            int dislike,
            string evaluation,
            string argue_msg
        );

        public record Dimension(
            int width,
            int height,
            int rotate
        );

        public record Pages(
            int cid,
            int page,
            string from,
            string part,
            int duration,
            string vid,
            string weblink,
            Dimension1 dimension,
            string first_frame
        );

        public record Dimension1(
            int width,
            int height,
            int rotate
        );

        public record Subtitle(
            bool allow_submit,
            object[] list
        );

        public record User_garb(
            string url_image_ani_cut
        );

        public record Honor_reply(
        );
    }

    private class PlayUrlJson
    {
        public class RootObject
        {
            public int code { get; set; }
            public string message { get; set; }
            public int ttl { get; set; }
            public Data? data { get; set; }
        }

        public class Data
        {
            public string from { get; set; }
            public string result { get; set; }
            public string message { get; set; }
            public int quality { get; set; }
            public string format { get; set; }
            public int timelength { get; set; }
            public string accept_format { get; set; }
            public string[] accept_description { get; set; }
            public int[] accept_quality { get; set; }
            public int video_codecid { get; set; }
            public string seek_param { get; set; }
            public string seek_type { get; set; }
            public Dash dash { get; set; }
            public Support_formats[] support_formats { get; set; }
            public object high_format { get; set; }
            public int last_play_time { get; set; }
            public int last_play_cid { get; set; }
        }

        public class Dash
        {
            public int duration { get; set; }
            public double minBufferTime { get; set; }
            public double min_buffer_time { get; set; }
            public Video[] video { get; set; }
            public Audio[] audio { get; set; }
            public Dolby dolby { get; set; }
            public object flac { get; set; }
        }

        public class Video
        {
            public int id { get; set; }
            public string baseUrl { get; set; }
            public string base_url { get; set; }
            public string[] backupUrl { get; set; }
            public string[] backup_url { get; set; }
            public int bandwidth { get; set; }
            public string mimeType { get; set; }
            public string mime_type { get; set; }
            public string codecs { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public string frameRate { get; set; }
            public string frame_rate { get; set; }
            public string sar { get; set; }
            public int startWithSap { get; set; }
            public int start_with_sap { get; set; }
            public SegmentBase SegmentBase { get; set; }
            public Segment_base segment_base { get; set; }
            public int codecid { get; set; }
        }

        public class SegmentBase
        {
            public string Initialization { get; set; }
            public string indexRange { get; set; }
        }

        public class Segment_base
        {
            public string initialization { get; set; }
            public string index_range { get; set; }
        }

        public class Audio
        {
            public int id { get; set; }
            public string baseUrl { get; set; }
            public string base_url { get; set; }
            public string[] backupUrl { get; set; }
            public string[] backup_url { get; set; }
            public int bandwidth { get; set; }
            public string mimeType { get; set; }
            public string mime_type { get; set; }
            public string codecs { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public string frameRate { get; set; }
            public string frame_rate { get; set; }
            public string sar { get; set; }
            public int startWithSap { get; set; }
            public int start_with_sap { get; set; }
            public SegmentBase1 SegmentBase { get; set; }
            public Segment_base1 segment_base { get; set; }
            public int codecid { get; set; }
        }

        public class SegmentBase1
        {
            public string Initialization { get; set; }
            public string indexRange { get; set; }
        }

        public class Segment_base1
        {
            public string initialization { get; set; }
            public string index_range { get; set; }
        }

        public class Dolby
        {
            public int type { get; set; }
            public object audio { get; set; }
        }

        public class Support_formats
        {
            public int quality { get; set; }
            public string format { get; set; }
            public string new_description { get; set; }
            public string display_desc { get; set; }
            public string superscript { get; set; }
            public string[] codecs { get; set; }
        }
    }

    #endregion
}