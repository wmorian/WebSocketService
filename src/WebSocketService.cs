namespace WebSocketService
{
  using System;
  using System.Net.WebSockets;
  using System.Text;
  using System.Threading;
  using System.Threading.Tasks;
  using Newtonsoft.Json;

  public class WebSocketService
  {
    private readonly WebSocket webSocket;
    private readonly IDataProvider<DataEventArgs> dataProvider;

    public WebSocketService(WebSocket webSocket, IDataProvider<DataEventArgs> dataProvider)
    {
      this.webSocket = webSocket;
      this.dataProvider = dataProvider;
      this.dataProvider.DataChanged += this.SendNewData;
    }

    public async Task StartAsync()
    {
      var receiving = this.Receiving();
      var fetching = this.Fetching();

      await Task.WhenAll(receiving, fetching);
    }

    private async Task Fetching()
    {
      this.dataProvider.IsActive = true;
      while (this.dataProvider.IsActive)
      {
        await this.dataProvider.StartFetchingAsync();
        await Task.Delay(500);
      }

      this.dataProvider.StopFetching();
    }

    private async Task Receiving()
    {
      var buffer = new byte[1024 * 4];

      while (this.webSocket.State == WebSocketState.Open)
      {
        var result = await this.webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        if (result.MessageType == WebSocketMessageType.Close)
        {
          this.dataProvider.IsActive = false;
          await this.webSocket?.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Server has closed the WebSocket", CancellationToken.None);
          break;
        }
      }
    }

    private async void SendNewData(object sender, DataEventArgs args)
    {
      if (this.webSocket.State == WebSocketState.Open)
      {
        var json = JsonConvert.SerializeObject(args.Data);
        await this.SendMessageAsync(json);
      }
    }

    private async Task SendMessageAsync(string message)
    {
      if (this.webSocket.State == WebSocketState.Open)
      {
        await this.webSocket?.SendAsync(
          buffer: new ArraySegment<byte>(
            array: Encoding.ASCII.GetBytes(message),
            offset: 0,
            count: message.Length),
          messageType: WebSocketMessageType.Text,
          endOfMessage: true,
          cancellationToken: CancellationToken.None);
      }
    }
  }
}
