using System.Collections.Concurrent;
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
        private static readonly ConcurrentDictionary<string, string> _idNameDictionary =
            new ConcurrentDictionary<string, string>();

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
            Clients.Client(clientId).SetAccounts(
                _idNameDictionary.Select(p => new ChatAccount { Id = p.Key, Name = p.Value }));

            var newAccount = new ChatAccount { Id = clientId, Name = userName };
            Clients.Others.AddAccount(newAccount);
            AddAccount(newAccount);
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
            _idNameDictionary.AddOrUpdate(newAccount.Id, id => newAccount.Name, (id, name) => newAccount.Name);
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
            string userName;
            _idNameDictionary.TryRemove(clientId, out userName);
        }
        #endregion
    }
}


// TODO: Test Log OnConnected & OnDisconnected...