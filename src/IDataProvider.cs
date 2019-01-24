namespace WebSocketService
{
  using System;
  using System.Threading.Tasks;

  public interface IDataProvider<TDataEventArgs>
  {
    event EventHandler<TDataEventArgs> DataChanged;

    bool IsActive { get; set; }

    Task StartFetchingAsync();

    void StopFetching();
  }
}
