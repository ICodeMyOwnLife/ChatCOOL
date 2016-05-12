using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ChatCommon;
using Microsoft.Owin.Hosting;


namespace ChatServer
{
    public class ChatServerViewModel: ChatViewModelBase
    {
        #region Fields
        private IDisposable _signalR;
        private ICommand _startServerAsyncCommand;
        private ICommand _stopServerCommand;
        #endregion


        #region  Properties & Indexers
        public override bool CanConnect
            => ChatConnectionState == ChatConnectionState.Closed && ChatConfig.IsValidServerUri(ServerUri);

        public override bool CanDisconnect => ChatConnectionState == ChatConnectionState.Connected;

        public ICommand StartServerAsyncCommand
            => GetCommand(ref _startServerAsyncCommand, async _ => await StartServerAsync(), _ => CanConnect);

        public ICommand StopServerCommand => GetCommand(ref _stopServerCommand, _ => StopServer(), _ => CanDisconnect);
        #endregion


        #region Methods
        public async Task StartServerAsync()
        {
            if (ChatConnectionState != ChatConnectionState.Closed)
            {
                Log($"Server is {ChatConnectionState.ToString().ToLower()}");
                return;
            }

            try
            {
                Log("Starting server...");
                ChatConnectionState = ChatConnectionState.Connecting;
                await Task.Run(() => _signalR = WebApp.Start(ServerUri));
                ChatConnectionState = ChatConnectionState.Connected;
                Log($"Server is started at {ServerUri}.");
                UpdateEnabilities();
            }
            catch (Exception exception)
            {
                Log(exception.Message);
            }
        }

        public void StopServer()
        {
            if (ChatConnectionState != ChatConnectionState.Connected)
            {
                Log($"Server is {ChatConnectionState.ToString().ToLower()}.");
                return;
            }

            _signalR.Dispose();
            _signalR = null;
            ChatConnectionState = ChatConnectionState.Closed;
            UpdateEnabilities();
            Log("Server is stopped.");
        }
        #endregion
    }
}


// TODO: Validate ServerUri, CanStartServer = false if valilation failed