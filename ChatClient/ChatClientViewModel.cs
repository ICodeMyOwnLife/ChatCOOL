using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ChatCommon;
using Microsoft.AspNet.SignalR.Client;


namespace ChatClient
{
    public class ChatClientViewModel: ChatViewModelBase
    {
        #region Fields

        //private string _clientId;
        private const string TITLE = "Chat Client";
        private ICommand _connectServerAsyncCommand;
        private readonly ObservableCollection<ChatDialog> _dialogs = new ObservableCollection<ChatDialog>();
        private ICommand _disconnectServerCommand;
        private HubConnection _hubConnection;
        private IHubProxy _hubProxy;
        private string _message;
        private ChatDialog _selectedDialog;
        private ICommand _sendMessageAsyncCommand;
        private readonly SynchronizationContext _syncContext = SynchronizationContext.Current;
        private string _title = TITLE;
        private string _userName;
        #endregion


        #region  Properties & Indexers
        public override bool CanConnect
            => !string.IsNullOrEmpty(UserName) && ChatConfig.IsValidServerUri(ServerUri) &&
               ChatConnectionState == ChatConnectionState.Closed;

        public override bool CanDisconnect => ChatConnectionState == ChatConnectionState.Connected;

        public bool CanSendMessage
            => !string.IsNullOrEmpty(Message) && SelectedDialog != null &&
               ChatConnectionState == ChatConnectionState.Connected;

        public ICommand ConnectServerAsyncCommand
            => GetCommand(ref _connectServerAsyncCommand, async _ => await ConnectServerAsync(), _ => CanConnect);

        public IEnumerable<ChatDialog> Dialogs => _dialogs;

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

        public ChatDialog SelectedDialog
        {
            get { return _selectedDialog; }
            private set { SetProperty(ref _selectedDialog, value); }
        }

        public ICommand SendMessageAsyncCommand
            => GetCommand(ref _sendMessageAsyncCommand, async _ => await SendMessageAsync(), _ => CanSendMessage);

        public string Title
        {
            get { return _title; }
            private set { SetProperty(ref _title, value); }
        }

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
                Title = $"{TITLE} - {UserName}";
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
            ClearDialogs();
            Title = TITLE;
            ChatConnectionState = ChatConnectionState.Closed;
        }

        public async Task SendMessageAsync()
        {
            if (!CanSendMessage) return;

            await _hubProxy.Invoke("SendMessage", SelectedDialog.Account.Id, Message);
            Message = "";
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
        private void AddDialog(ChatAccount account)
        {
            _dialogs.Add(new ChatDialog(account));
        }

        private void AddDialogOnUiThread(ChatAccount account)
        {
            _syncContext.Send(_ => AddDialog(account), null);
        }

        private void ClearDialogs()
        {
            _dialogs.Clear();
        }

        private void FillDialogs(IEnumerable<ChatAccount> accounts)
        {
            if (_dialogs.Any()) return;

            foreach (var account in accounts)
            {
                _dialogs.Add(new ChatDialog(account));
            }
        }

        private void FillDialogsOnUiThread(IEnumerable<ChatAccount> accounts)
        {
            _syncContext.Send(_ => FillDialogs(accounts), null);
        }

        private void InitializeConnection()
        {
            _hubConnection = new HubConnection(ServerUri);
            _hubConnection.StateChanged += HubConnection_StateChanged;
            _hubConnection.Error += HubConnection_Error;
            _hubProxy = _hubConnection.CreateHubProxy("ChatHub");
            _hubProxy.On<string, string>("ShowMessage", ShowDialog);
            /*_hubProxy.On<string>("ReceiveId", id =>
            {
                _clientId = id;
                UpdateDialogs();
            });*/
            /*_hubProxy.On<IEnumerable<ChatAccount>>("ReceiveAccounts", UpdateDialogs);*/
            _hubProxy.On<ChatAccount>("AddAccount", AddDialogOnUiThread);
            _hubProxy.On<string>("RemoveAccount", RemoveDialogOnUiThread);
            _hubProxy.On<IEnumerable<ChatAccount>>("SetAccounts", FillDialogsOnUiThread);
        }

        private void RemoveDialog(string accountId)
        {
            _dialogs.Remove(_dialogs.FirstOrDefault(d => d.Account.Id == accountId));
        }

        private void RemoveDialogOnUiThread(string accountId)
        {
            _syncContext.Send(_ => RemoveDialog(accountId), null);
        }

        private void ShowDialog(string senderId, string message)
        {
            SelectedDialog = Dialogs.FirstOrDefault(d => d.Account.Id == senderId);
            SelectedDialog?.AddMessage(message);
        }
        #endregion


        /*private void UpdateDialogs(IEnumerable<ChatAccount> accounts)
        {
            var accountList = accounts as List<ChatAccount> ?? accounts.ToList();

            Log($"SetAccounts for {_clientId}:\r\n\t{string.Join("\r\n\t", accountList)}");

            accountList.ForEach(a => { if (Dialogs.All(d => d.Account.Id != a.Id))  });
        }*/
    }
}


// TODO: Test CanConnectServer, CanSendMessage
// TODO: Test EnterToClick
// TODO: Test Log Connection success, Connection closed
// TODO: Test Chat with specific user?
// TODO: Test Delete Message after sending
// TODO: Test Remove account when disconnected/closed and announce to client
// TODO: Test Account.Except(thisAccount)
// TODO: Test Disconnect when Closed
// TODO: Test Reset Accounts when disconnected
// TODO: Test Multi Dialogs
// TODO: Show self conversation