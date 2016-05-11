using System;
using CB.Model.Common;


namespace ChatCommon
{
    public abstract class ChatViewModelBase: ViewModelBase, ILog
    {
        #region Fields
        private bool _isConnected;
        private string _serverUri = ChatConfig.ChatServerUri;
        #endregion


        #region Abstract
        protected abstract void UpdateEnabilities();
        #endregion


        #region  Properties & Indexers
        public virtual bool IsConnected
        {
            get { return _isConnected; }
            protected set { SetProperty(ref _isConnected, value); }
        }

        public virtual string ServerUri
        {
            get { return _serverUri; }
            set { if (SetProperty(ref _serverUri, value)) UpdateEnabilities(); }
        }
        #endregion


        #region Methods
        public void Log(string log) => State = State == null ? log : State + Environment.NewLine + log;
        #endregion
    }
}