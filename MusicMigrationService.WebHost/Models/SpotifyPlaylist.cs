using MusicMigrationService.WebHost.Models.Interfaces;
using SpotifyAPI.Web;

namespace MusicMigrationService.WebHost.Models;

public class SpotifyPlaylist : IPlaylist
{
    public string Id { get; }
    public string Title { get; }
    public IEnumerable<ITrack> Tracks { get; }

    public SpotifyPlaylist(FullPlaylist spotyPlaylist)
    {
        Id = spotyPlaylist.Id      ?? string.Empty;
        Title = spotyPlaylist.Name ?? string.Empty;
        
        if (spotyPlaylist.Tracks is null)
            Tracks = Enumerable.Empty<ITrack>();
        else if (spotyPlaylist.Tracks.Items is null)
            Tracks = Enumerable.Empty<ITrack>();
        else
            Tracks = GetTrackList(spotyPlaylist.Tracks.Items);
    }

    private IEnumerable<ITrack> GetTrackList(List<PlaylistTrack<IPlayableItem>> tracksItems)
    {
        var trackList = new List<ITrack>();
        foreach (PlaylistTrack<IPlayableItem> trackItem in tracksItems)
        {
            if (trackItem.Track is FullTrack fullTrack)
                trackList.Add(new Track(fullTrack));
        }
        return trackList;
    }
}