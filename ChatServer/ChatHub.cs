using System.Threading.Tasks;
using System.Windows;
using ChatCommon;
using Microsoft.AspNet.SignalR;


namespace ChatServer
{
    public class ChatHub: Hub
    {
        #region Methods
        public void SendMessage(string userName, string message)
        {
            Clients.All.ShowMessage(userName, message);
        }
        #endregion

        private ILog GetLogger()
            => Application.Current.MainWindow.DataContext as ILog;
    }
}


// TODO: Log OnConnected & OnDisconnected...