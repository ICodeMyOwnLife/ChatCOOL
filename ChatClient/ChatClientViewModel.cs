using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ChatCommon;
using Microsoft.AspNet.SignalR.Client;


namespace ChatClient
{
    public class ChatClientViewModel: ChatViewModelBase
    {
        #region Fields
        private ChatAccount[] _accounts;
        private string _clientId;
        private ICommand _connectServerAsyncCommand;
        private string _dialog;
        private ICommand _disconnectServerCommand;
        private HubConnection _hubConnection;
        private IHubProxy _hubProxy;
        private string _message;
        private ChatAccount _selectedAccount;
        private ICommand _sendMessageAsyncCommand;
        private string _userName;
        #endregion


        #region  Properties & Indexers
        public ChatAccount[] Accounts
        {
            get { return _accounts; }
            private set { SetProperty(ref _accounts, value); }
        }

        public override bool CanConnect
            => !string.IsNullOrEmpty(UserName) && ChatConfig.IsValidServerUri(ServerUri) &&
               ChatConnectionState == ChatConnectionState.Closed;

        public override bool CanDisconnect => ChatConnectionState == ChatConnectionState.Connected;

        public bool CanSendMessage
            =>
                !string.IsNullOrEmpty(Message) && SelectedAccount != null &&
                ChatConnectionState == ChatConnectionState.Connected;

        public ICommand ConnectServerAsyncCommand
            => GetCommand(ref _connectServerAsyncCommand, async _ => await ConnectServerAsync(), _ => CanConnect);

        public string Dialog
        {
            get { return _dialog; }
            private set { SetProperty(ref _dialog, value); }
        }

        public ICommand DisconnectServerCommand
            => GetCommand(ref _disconnectServerCommand, _ => DisconnectServer(), _ => CanDisconnect);

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

        public ChatAccount SelectedAccount
        {
            get { return _selectedAccount; }
            set { SetProperty(ref _selectedAccount, value); }
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
            if (ChatConnectionState != ChatConnectionState.Closed)
            {
                Log($"Cannot connect whie this client is {ChatConnectionState.ToString().ToLower()}.");
                return;
            }

            ChatConnectionState = ChatConnectionState.Connecting;
            InitializeConnection();

            try
            {
                await _hubConnection.Start();
                await _hubProxy.Invoke("LogIn", UserName);
                ChatConnectionState = ChatConnectionState.Connected;
            }
            catch (Exception exception)
            {
                Log(exception.Message);
                ChatConnectionState = ChatConnectionState.Closed;
            }
        }

        public void DisconnectServer()
        {
            if (ChatConnectionState != ChatConnectionState.Connected)
            {
                Log($"Cannot disconnect when the client is {ChatConnectionState.ToString().ToLower()}.");
                return;
            }

            _hubConnection.Stop();
            ChatConnectionState = ChatConnectionState.Closed;
            UpdateEnabilities();
        }

        public async Task SendMessageAsync()
        {
            if (!CanSendMessage) return;

            await _hubProxy.Invoke("SendMessage", SelectedAccount.Id, Message);
            Message = "";
            UpdateEnabilities();
        }
        #endregion


        #region Override
        protected override void UpdateEnabilities()
        {
            base.UpdateEnabilities();
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


        #region Implementation
        private void AddDialog(string userName, string message)
        {
            var s = $"{userName}: {message}";
            Dialog = string.IsNullOrEmpty(Dialog) ? s : Dialog + Environment.NewLine + s;
        }

        private void InitializeConnection()
        {
            _hubConnection = new HubConnection(ServerUri);
            _hubConnection.StateChanged += HubConnection_StateChanged;
            _hubConnection.Error += HubConnection_Error;
            _hubProxy = _hubConnection.CreateHubProxy("ChatHub");
            _hubProxy.On<string, string>("ShowMessage", AddDialog);
            _hubProxy.On<string>("ReceiveId", id =>
            {
                _clientId = id;
                SetAccounts(Accounts, id);
            });
            _hubProxy.On<IEnumerable<ChatAccount>>("ReceiveAccounts",
                accounts => SetAccounts(accounts, _clientId));
        }

        private void SetAccounts(IEnumerable<ChatAccount> accounts, string clientId)
        {
            Accounts = string.IsNullOrEmpty(clientId) || accounts == null
                           ? null : accounts.Where(a => a.Id != clientId).ToArray();
        }
        #endregion
    }
}


// TODO: Test CanConnectServer, CanSendMessage
// TODO: Test EnterToClick
// TODO: Test Log Connection success, Connection closed
// TODO: Test Chat with specific user?
// TODO: Test Delete Message after sending
// TODO: Test Remove account when disconnected/closed
// TODO: Test Account.Except(thisAccount)
// TODO: Disconnect when Closed