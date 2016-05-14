using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ChatCommon;
using Microsoft.AspNet.SignalR;


namespace ChatServer
{
    public class ChatHub: Hub
    {
        #region Fields
        private static readonly IList<ChatAccount> _accounts = new List<ChatAccount>();
        private ILog _logger;
        #endregion


        #region  Constructors & Destructor
        public ChatHub()
        {
            Application.Current.Dispatcher?.Invoke(() => _logger = Application.Current.MainWindow?.DataContext as ILog);
        }
        #endregion


        #region Methods
        public void LogIn(string userName)
        {
            var clientId = Context.ConnectionId;
            Clients.Client(clientId).SetAccounts(_accounts);

            var newAccount = new ChatAccount { Id = clientId, Name = userName };
            Clients.AllExcept(clientId).AddAccount(newAccount);
            //Clients.Client(clientId).ReceiveId(clientId);
            AddAccount(newAccount);

            //SendAccounts();
        }

        public void SendMessage(string receiverId, string message)
        {
            var senderId = Context.ConnectionId;
            Clients.Client(receiverId).ShowMessage(senderId, message);
        }
        #endregion


        #region Override
        public override Task OnConnected()
        {
            LogConnection(Context.ConnectionId);
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var clientId = Context.ConnectionId;
            LogDisconnection(clientId);
            Clients.AllExcept(clientId).RemoveAccount(clientId);
            RemoveAccount(Context.ConnectionId);

            //SendAccounts();
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            _logger?.Log($"Client reconnected: {Context.ConnectionId}");
            return base.OnReconnected();
        }
        #endregion


        #region Implementation
        private static void AddAccount(ChatAccount newAccount)
        {
            var account = _accounts.SingleOrDefault(a => a.Id == newAccount.Id);
            if (account == null)
            {
                _accounts.Add(newAccount);
            }
            else
            {
                account.Name = newAccount.Name;
            }
        }

        private void LogConnection(string clientId)
        {
            _logger?.Log($"Client connected: {clientId}");
        }

        private void LogDisconnection(string clientId)
        {
            _logger?.Log($"Client disconnected: {clientId}");
        }

        private static void RemoveAccount(string clientId)
        {
            _accounts.Remove(_accounts.FirstOrDefault(a => a.Id == clientId));
        }
        #endregion


        /*private void SendAccounts()
        {
            Clients.All.ReceiveAccounts(_accounts);
        }*/
    }
}


// TODO: Test Log OnConnected & OnDisconnected...