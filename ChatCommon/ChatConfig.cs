using System.Text.RegularExpressions;


namespace ChatCommon
{
    public class ChatConfig
    {
        #region  Properties & Indexers
        public static string ChatServerUri { get; } = "http://localhost:9494";
        #endregion


        #region Methods
        public static bool IsValidServerUri(string serverUri)
            => !string.IsNullOrEmpty(serverUri) &&
               Regex.IsMatch(serverUri, @"http://(localhost|\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\:\d{4}");
        #endregion
    }
}