using System.Collections.Concurrent;
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
            var userId = Context.ConnectionId;
            _idNameDictionary[userId] = userName;

            //SendId(userId);
            SendAccounts();
        }

        public void SendMessage(string receiveId, string message)
        {
            var sendName = _idNameDictionary[Context.ConnectionId];
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
            //_idNameDictionary.Remove(_idNameDictionary.FirstOrDefault(a => a.Id == clientId));
            string userName;
            _idNameDictionary.TryRemove(clientId, out userName);
        }

        private void SendAccounts()
        {
            Clients.All.ReceiveAccounts(_idNameDictionary);
        }

        /*private void SendId(string clientId)
        {
            Clients.Client(clientId).ReceiveId(clientId);
        }*/
        #endregion
    }
}


// TODO: Test Log OnConnected & OnDisconnected...