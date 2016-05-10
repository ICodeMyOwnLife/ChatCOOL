using System.Threading.Tasks;
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
    }
}


// TODO: Log OnConnected & OnDisconnected...