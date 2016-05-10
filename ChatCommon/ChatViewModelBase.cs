using System;
using CB.Model.Common;


namespace ChatCommon
{
    public class ChatViewModelBase: ViewModelBase
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


        #region Implementation
        protected void Log(string log) => State = State == null ? log : State + Environment.NewLine + log;
        #endregion
    }
}