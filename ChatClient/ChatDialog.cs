using System;
using CB.Model.Common;
using ChatCommon;


namespace ChatClient
{
    public class ChatDialog: ObservableObject
    {
        #region Fields
        private string _dialog;
        #endregion


        #region  Constructors & Destructor
        public ChatDialog(ChatAccount account)
        {
            Account = account;
        }
        #endregion


        #region  Properties & Indexers
        public ChatAccount Account { get; }

        public string Dialog
        {
            get { return _dialog; }
            private set { SetProperty(ref _dialog, value); }
        }
        #endregion


        #region Methods
        public void AddMessage(string message)
        {
            string s = $"{Account.Name}: {message}";
            Dialog = string.IsNullOrEmpty(Dialog) ? s : Dialog + Environment.NewLine + s;
        }
        #endregion
    }
}