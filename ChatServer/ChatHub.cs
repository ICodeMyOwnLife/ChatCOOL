using System.Threading.Tasks;
using System.Windows;
using ChatCommon;
using Microsoft.AspNet.SignalR;


namespace ChatServer
{
    public class ChatHub: Hub
    {
        #region Fields
        private ILog _logger;
        #endregion


        #region  Constructors & Destructor
        public ChatHub()
        {
            Application.Current.Dispatcher?.Invoke(() => _logger = Application.Current.MainWindow?.DataContext as ILog);
        }
        #endregion


        #region Methods
        public void SendMessage(string userName, string message)
        {
            Clients.All.ShowMessage(userName, message);
        }
        #endregion


        #region Override
        public override Task OnConnected()
        {
            _logger?.Log($"Client connected: {Context.ConnectionId}");
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            _logger?.Log($"Client disconnected: {Context.ConnectionId}");
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            _logger?.Log($"Client reconnected: {Context.ConnectionId}");
            return base.OnReconnected();
        }
        #endregion
    }
}


// TODO: Log OnConnected & OnDisconnected...