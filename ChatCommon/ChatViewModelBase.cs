using System;
using CB.Model.Common;


namespace ChatCommon
{
    public class ChatViewModelBase: ViewModelBase, ILog
    {
        #region Fields
        private string _serverUri = ChatConfig.ChatServerUri;
        #endregion


        #region  Properties & Indexers
        public string ServerUri
        {
            get { return _serverUri; }
            set { SetProperty(ref _serverUri, value); }
        }
        #endregion


        #region Methods
        public void Log(string log) => State = State == null ? log : State + Environment.NewLine + log;
        #endregion
    }
}