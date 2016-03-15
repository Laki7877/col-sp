namespace Colsp.Model.Requests
{
    public class TagRequest
    {
        public int TagId { get; set; }
        public string TagName { get; set; }

        public TagRequest()
        {
            TagId = 0;
            TagName = string.Empty;
        }
    }
}
