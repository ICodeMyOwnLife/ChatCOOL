using System;
using System.Data;
using CB.Model.Common;


namespace ChatCommon
{
    public abstract class ChatViewModelBase: ViewModelBase, ILog
    {
        #region Fields
        private bool _canConnect = true;
        private bool _canDisconnect;
        private ChatConnectionState _connectionState;
        private string _serverUri = ChatConfig.ChatServerUri;
        #endregion


        #region Abstract
        protected abstract void UpdateEnabilities();
        #endregion


        #region  Properties & Indexers
        public bool CanConnect
        {
            get { return _canConnect; }
            protected set { SetProperty(ref _canConnect, value); }
        }

        public bool CanDisconnect
        {
            get { return _canDisconnect; }
            protected set { SetProperty(ref _canDisconnect, value); }
        }

        public virtual ChatConnectionState ConnectionState
        {
            get { return _connectionState; }
            protected set { SetProperty(ref _connectionState, value); }
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