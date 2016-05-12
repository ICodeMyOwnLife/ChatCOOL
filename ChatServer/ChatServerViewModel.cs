using System;
using System.Data;
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
        public bool CanStartServer
            => ConnectionState == ConnectionState.Closed && ChatConfig.IsValidServerUri(ServerUri);

        public bool CanStopServer => ConnectionState == ConnectionState.Open;

        public ICommand StartServerAsyncCommand
            => GetCommand(ref _startServerAsyncCommand, async _ => await StartServerAsync(), _ => CanStartServer);

        public ICommand StopServerCommand => GetCommand(ref _stopServerCommand, _ => StopServer(), _ => CanStopServer);
        #endregion


        #region Methods
        public async Task StartServerAsync()
        {
            if (ConnectionState != ConnectionState.Closed)
            {
                Log($"Server is {ConnectionState.ToString().ToLower()}");
                return;
            }

            try
            {
                Log("Starting server...");
                ConnectionState = ConnectionState.Connecting;
                await Task.Run(() => _signalR = WebApp.Start(ServerUri));
                ConnectionState = ConnectionState.Open;
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
            if (ConnectionState != ConnectionState.Open)
            {
                Log($"Server is {ConnectionState.ToString().ToLower()}.");
                return;
            }

            _signalR.Dispose();
            _signalR = null;
            ConnectionState = ConnectionState.Closed;
            UpdateEnabilities();
            Log("Server is stopped.");
        }
        #endregion


        #region Override
        protected override void UpdateEnabilities()
        {
            NotifyPropertyChanged(nameof(CanStartServer));
            NotifyPropertyChanged(nameof(CanStopServer));
        }
        #endregion
    }
}


// TODO: Validate ServerUri, CanStartServer = false if valilation failed