using System.ComponentModel;

namespace Manga.Interfaces
{
    /// <summary>
    /// The interface to support multiple Server for the program
    /// </summary>
    public interface IServer : INotifyPropertyChanged
    {
        bool IsRunning { get; set; }
        void Download(object obj, bool State);
        void FetchList(object obj, System.Threading.CancellationToken Token);
        void GetInfoExtended(object obj, bool State);
        void GetInfoMinimal(IMangaModel obj, bool State);
        void FindImages(IReaderModel obj, bool State);
        void FindImages(IChapterModel obj, bool State);
        void CheckUpdates(IMangaModel obj, System.Threading.CancellationToken Token);
    }
}
