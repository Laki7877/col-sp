namespace Colsp.Model.Requests
{
    public class VideoLinkRequest
    {
        public string Url { get; set; }
        public int VideoId { get; set; }

        public VideoLinkRequest()
        {
            Url = string.Empty;
            VideoId = 0;
        }
    }
}
