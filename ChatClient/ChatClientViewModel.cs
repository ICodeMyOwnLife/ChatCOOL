using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ChatCommon;
using Microsoft.AspNet.SignalR.Client;


namespace ChatClient
{
    public class ChatClientViewModel: ChatViewModelBase
    {
        #region Fields
        private ICommand _connectServerAsyncCommand;
        private ICommand _disconnectServerCommand;
        private HubConnection _hubConnection;
        private IHubProxy _hubProxy;
        private string _message;
        private ICommand _sendMessageAsyncCommand;
        private string _userName;
        #endregion


        #region  Properties & Indexers
        public bool CanConnectServer
            => !string.IsNullOrEmpty(UserName) && ChatConfig.IsValidServerUri(ServerUri) && !IsConnected;

        public bool CanDisconnectServer => IsConnected;

        public bool CanSendMessage
            => !string.IsNullOrEmpty(Message) && IsConnected;

        public ICommand ConnectServerAsyncCommand
            => GetCommand(ref _connectServerAsyncCommand, async _ => await ConnectServerAsync(), _ => CanConnectServer);

        public ICommand DisconnectServerCommand
            => GetCommand(ref _disconnectServerCommand, _ => DisconnectServer(), _ => CanDisconnectServer);

        public string Message
        {
            get { return _message; }
            set
            {
                if (SetProperty(ref _message, value))
                {
                    UpdateEnabilities();
                }
            }
        }

        public ICommand SendMessageAsyncCommand
            => GetCommand(ref _sendMessageAsyncCommand, async _ => await SendMessageAsync(), _ => CanSendMessage);

        public string UserName
        {
            get { return _userName; }
            set
            {
                if (SetProperty(ref _userName, value))
                {
                    UpdateEnabilities();
                }
            }
        }
        #endregion


        #region Methods
        public async Task ConnectServerAsync()
        {
            if (IsConnected)
            {
                Log("This client is connected to server.");
                return;
            }

            _hubConnection = new HubConnection(ServerUri);
            _hubConnection.StateChanged += HubConnection_StateChanged;
            _hubConnection.Error += HubConnection_Error;
            _hubProxy = _hubConnection.CreateHubProxy("ChatHub");
            _hubProxy.On<string, string>("ShowMessage", (userName, message) => Log($"{userName}: {message}"));

            try
            {
                await _hubConnection.Start();
                IsConnected = true;
                UpdateEnabilities();
            }
            catch (Exception exception)
            {
                Log(exception.Message);
            }
        }

        public void DisconnectServer()
        {
            if (!IsConnected)
            {
                Log("Client is not connected to server.");
                return;
            }

            _hubConnection.Stop();
            IsConnected = false;
            UpdateEnabilities();
        }

        public async Task SendMessageAsync()
        {
            if (!CanSendMessage) return;

            await _hubProxy.Invoke("SendMessage", UserName, Message);
            Message = "";
            UpdateEnabilities();
        }
        #endregion


        #region Override
        protected override void UpdateEnabilities()
        {
            NotifyPropertyChanged(nameof(CanConnectServer));
            NotifyPropertyChanged(nameof(CanDisconnectServer));
            NotifyPropertyChanged(nameof(CanSendMessage));
        }
        #endregion


        #region Event Handlers
        private void HubConnection_Error(Exception exception)
        {
            Log(exception.Message);
        }

        private void HubConnection_StateChanged(StateChange stateChange)
        {
            string msg;
            switch (stateChange.NewState)
            {
                case ConnectionState.Connecting:
                    msg = "Connecting...";
                    break;
                case ConnectionState.Connected:
                    msg = $"Connected to {ServerUri}";
                    break;
                case ConnectionState.Reconnecting:
                    msg = "Reconnecting...";
                    break;
                case ConnectionState.Disconnected:
                    msg = "Disconnected";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Log(msg);
        }
        #endregion
    }
}


// TODO: Test CanConnectServer, CanSendMessage
// TODO: EnterToClick
// TODO: Log Connection success, Connection closed
// TODO: Chat with specific user?
// TODO: Delete Message after sending