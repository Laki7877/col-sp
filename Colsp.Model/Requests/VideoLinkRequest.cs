namespace Colsp.Model.Requests
{
    public class VideoLinkRequest
    {
        public string Url { get; set; }
        public long VideoId { get; set; }
        public string Description { get; set; }
        public string Thumbnail { get; set; }
        public int Position { get; set; }


        public VideoLinkRequest()
        {
            Url = string.Empty;
            VideoId = 0;
            Description = string.Empty;
            Thumbnail = string.Empty;
        }
    }
}
