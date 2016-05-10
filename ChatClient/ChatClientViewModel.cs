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
        private bool _canConnectServer = true;
        private bool _canSendMessage = true;
        private ICommand _connectServerAsyncCommand;
        private HubConnection _hubConnection;
        private IHubProxy _hubProxy;
        private string _message;
        private ICommand _sendMessageAsyncCommand;
        private string _userName;
        #endregion


        #region  Properties & Indexers
        public bool CanConnectServer
        {
            get { return _canConnectServer; }
            private set { SetProperty(ref _canConnectServer, value); }
        }

        public bool CanSendMessage
        {
            get { return _canSendMessage; }
            private set { SetProperty(ref _canSendMessage, value); }
        }

        public ICommand ConnectServerAsyncCommand
            => GetCommand(ref _connectServerAsyncCommand, async _ => await ConnectServerAsync(), _ => CanConnectServer);

        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        public ICommand SendMessageAsyncCommand
            => GetCommand(ref _sendMessageAsyncCommand, async _ => await SendMessageAsync(), _ => CanSendMessage);

        public string UserName
        {
            get { return _userName; }
            set { SetProperty(ref _userName, value); }
        }
        #endregion


        #region Methods
        public async Task ConnectServerAsync()
        {
            _hubConnection = new HubConnection(ServerUri);
            _hubConnection.Closed += HubConnection_Closed;
            _hubProxy = _hubConnection.CreateHubProxy("ChatHub");
            _hubProxy.On<string, string>("ShowMessage", (userName, message) => Log($"{userName}: {message}"));

            try
            {
                await _hubConnection.Start();
            }
            catch (Exception exception)
            {
                Log(exception.Message);
            }
        }

        public async Task SendMessageAsync()
        {
            await _hubProxy.Invoke("SendMessage", UserName, Message);
        }
        #endregion


        #region Event Handlers
        private void HubConnection_Closed()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

// TODO: Implement CanConnectServer, CanSendMessage
// TODO: Log Connection success, Connection closed
// TODO: Chat with specific user?
// TODO: Delete Message after sending