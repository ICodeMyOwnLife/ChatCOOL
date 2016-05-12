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
            var account = _accounts.SingleOrDefault(a => a.Id == clientId);
            if (account == null)
            {
                _accounts.Add(new ChatAccount { Id = clientId, UserName = userName });
            }
            else
            {
                account.UserName = userName;
            }
            SendId(clientId);
            SendAccounts();
        }

        public void SendMessage(string receiveId, string message)
        {
            var sendName = _accounts.SingleOrDefault(a => a.Id == Context.ConnectionId)?.UserName;
            Clients.Client(receiveId).ShowMessage(sendName, message);
        }
        #endregion


        #region Override
        public override Task OnConnected()
        {
            LogConnection();
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            LogDisconnection();
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
        private void LogConnection()
        {
            _logger?.Log($"Client connected: {Context.ConnectionId}");
        }

        private void LogDisconnection()
        {
            _logger?.Log($"Client disconnected: {Context.ConnectionId}");
        }

        private static void RemoveAccount(string clientId)
        {
            _accounts.Remove(_accounts.FirstOrDefault(a => a.Id == clientId));
        }

        private void SendAccounts()
        {
            Clients.All.ReceiveAccounts(_accounts);
        }

        private void SendId(string clientId)
        {
            Clients.Client(clientId).ReceiveId(clientId);
        }
        #endregion
    }
}


// TODO: Test Log OnConnected & OnDisconnected...