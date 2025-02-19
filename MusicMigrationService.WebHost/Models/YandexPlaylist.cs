using MusicMigrationService.WebHost.Models.Interfaces;
using Yandex.Music.Api.Models.Playlist;

namespace MusicMigrationService.WebHost.Models;

public class YandexPlaylist : IPlaylist
{
    public string Id { get; }
    public string Title { get; }
    public IEnumerable<ITrack> Tracks { get; }

    public YandexPlaylist(YPlaylist yaPlaylist)
    {
        Id = yaPlaylist.Uid;
        Title = yaPlaylist.Title;

        var yTracks = yaPlaylist.Tracks.Select(container => container.Track);
        Tracks = yTracks.Select(yTrack => new Track(yTrack));
    }
}