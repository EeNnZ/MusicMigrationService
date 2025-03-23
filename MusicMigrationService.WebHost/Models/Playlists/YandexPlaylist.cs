using MusicMigrationService.WebHost.Models.Interfaces;
using Yandex.Music.Api.Models.Playlist;
using Yandex.Music.Api.Models.Track;

namespace MusicMigrationService.WebHost.Models.Playlists;

public class YandexPlaylist : IPlaylist
{
    public string Id { get; }
    public string Title { get; }
    public IEnumerable<ITrack> Tracks { get; }

    public YandexPlaylist(YPlaylist yaPlaylist)
    {
        Id = yaPlaylist.Uid;
        Title = yaPlaylist.Title;

        IEnumerable<YTrack>? yTracks = yaPlaylist.Tracks.Select(container => container.Track);
        Tracks = yTracks.Select(yTrack => new Track(yTrack));
    }
}