using System.Text;
using Yandex.Music.Api.Models.Artist;
using Yandex.Music.Api.Models.Track;

namespace MusicMigrationService.WebHost.Models.Interfaces;

public interface IPlaylist
{
    string Id { get; }
    string Title { get; }
    IEnumerable<ITrack> Tracks { get; }
}