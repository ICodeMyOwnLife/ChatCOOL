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
        private ICommand _startServerCommand;
        private ICommand _stopServerCommand;
        #endregion


        #region  Properties & Indexers
        public bool CanStartServer => !IsConnected && ChatConfig.IsValidServerUri(ServerUri);
        public bool CanStopServer => IsConnected;

        public ICommand StartServerAsyncCommand
            => GetCommand(ref _startServerAsyncCommand, async _ => await StartServerAsync(), _ => CanStartServer);

        public ICommand StartServerCommand
            => GetCommand(ref _startServerCommand, _ => StartServer(), _ => CanStartServer);

        public ICommand StopServerCommand => GetCommand(ref _stopServerCommand, _ => StopServer(), _ => CanStopServer);
        #endregion


        #region Methods
        public void StartServer()
        {
            if (IsConnected)
            {
                Log("Server is running");
                return;
            }

            try
            {
                Log("Starting server...");
                _signalR = WebApp.Start(ServerUri);
                Log($"Server is started at {ServerUri}.");
                IsConnected = true;
                UpdateEnabilities();
            }
            catch (Exception exception)
            {
                Log(exception.Message);
            }
        }

        public async Task StartServerAsync() => await Task.Run(() => StartServer());

        public void StopServer()
        {
            if (!IsConnected)
            {
                Log("Server is not running.");
                return;
            }

            _signalR.Dispose();
            _signalR = null;
            IsConnected = false;
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