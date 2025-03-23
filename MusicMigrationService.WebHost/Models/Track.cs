using System.Text;
using MusicMigrationService.WebHost.Models.Interfaces;
using SpotifyAPI.Web;
using Yandex.Music.Api.Models.Artist;
using Yandex.Music.Api.Models.Track;

namespace MusicMigrationService.WebHost.Models;

public class Track : ITrack
{
    public string Id { get; init; }
    public string Title { get; init; }
    public string Artist { get; init; }
    public uint Duration { get; init; }

    public Track(YTrack yandexTrack)
    {
        Id = yandexTrack.Id;
        Title = yandexTrack.Title;
        Artist = MakeArtistString(yandexTrack.Artists);
        Duration = Convert.ToUInt32(yandexTrack.DurationMs / 1000);
    }

    public Track(FullTrack spotyPlayableItem)
    {
        Id = spotyPlayableItem.Id;
        Title = spotyPlayableItem.Name;
        Artist = MakeArtistString(spotyPlayableItem.Artists);
        Duration = Convert.ToUInt32(spotyPlayableItem.DurationMs / 1000);
    }

    private string MakeArtistString(IEnumerable<YArtist> artists)
    {
        var sb = new StringBuilder();
        foreach (YArtist yArtist in artists)
        {
            sb.Append(yArtist.Name).Append(", ");
        }
        
        return sb.ToString().TrimEnd(',');
    }

    private string MakeArtistString(List<SimpleArtist> artists)
    {
        var sb = new StringBuilder();
        foreach (SimpleArtist simpleArtist in artists)
        {
            sb.Append(simpleArtist.Name).Append(", ");
        }
        
        return sb.ToString().TrimEnd(',');
    }
    
    public bool LooksLike(ITrack track)
    {
        bool isTitleSame = string.Compare(Title, track.Title, StringComparison.InvariantCultureIgnoreCase) == 0;
        bool isArtistSame = string.Compare(Artist, track.Artist, StringComparison.InvariantCultureIgnoreCase) == 0;
        
        return isTitleSame && isArtistSame;
    }
}