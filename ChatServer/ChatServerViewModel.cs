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
        private bool _canStartServer = true;
        private bool _canStopServer;
        private IDisposable _signalR;
        private ICommand _startServerAsyncCommand;
        private ICommand _startServerCommand;
        private ICommand _stopServerCommand;
        #endregion


        #region  Properties & Indexers
        public bool CanStartServer
        {
            get { return _canStartServer; }
            private set { SetProperty(ref _canStartServer, value); }
        }

        public bool CanStopServer
        {
            get { return _canStopServer; }
            private set { SetProperty(ref _canStopServer, value); }
        }

        public ICommand StartServerAsyncCommand
            => GetCommand(ref _startServerAsyncCommand, async _ => await StartServerAsync(), _ => CanStartServer);

        public ICommand StartServerCommand
            => GetCommand(ref _startServerCommand, _ => StartServer(), _ => CanStartServer);

        public ICommand StopServerCommand => GetCommand(ref _stopServerCommand, _ => StopServer(), _ => CanStopServer);
        #endregion


        #region Methods
        public void StartServer()
        {
            if (_signalR != null)
            {
                Log("Server is running");
                return;
            }

            try
            {
                Log("Starting server...");
                _signalR = WebApp.Start(ServerUri);
                CanStartServer = false;
                CanStopServer = true;
                Log($"Server is started at {ServerUri}.");
            }
            catch (Exception exception)
            {
                Log(exception.Message);
            }
        }

        public async Task StartServerAsync() => await Task.Run(() => StartServer());

        public void StopServer()
        {
            if (_signalR == null)
            {
                Log("Server is not running.");
                return;
            }

            _signalR.Dispose();
            _signalR = null;
            CanStartServer = true;
            CanStopServer = false;
            Log("Server is stopped.");
        }
        #endregion
    }
}


// TODO: Validate ServerUri, CanStartServer = false if valilation failed