namespace ChatCommon
{
    public class ChatAccount
    {
        #region  Properties & Indexers
        public string Id { get; set; }
        public string Name { get; set; }
        #endregion


        #region Override
        public override string ToString()
        {
            return $"Id: {Id} - Name: {Name}";
        }
        #endregion
    }
}