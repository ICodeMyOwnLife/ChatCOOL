using System;
using CB.Model.Common;


namespace ChatCommon
{
    public abstract class ChatViewModelBase: ViewModelBase, ILog
    {
        #region Fields
        private ChatConnectionState _chatConnectionState = ChatConnectionState.Closed;
        private string _serverUri = ChatConfig.ChatServerUri;
        #endregion


        #region  Properties & Indexers
        public virtual bool CanConnect { get; protected set; }

        public virtual bool CanDisconnect { get; protected set; }

        public virtual ChatConnectionState ChatConnectionState
        {
            get { return _chatConnectionState; }
            protected set { if (SetProperty(ref _chatConnectionState, value)) UpdateEnabilities(); }
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


        #region Implementation
        protected virtual void UpdateEnabilities()
        {
            NotifyPropertyChanged(nameof(CanConnect));
            NotifyPropertyChanged(nameof(CanDisconnect));
        }
        #endregion
    }
}