namespace Colsp.Model.Requests
{
    public class ImageRequest
    {
        public string tmpPath { get; set; }
        public string url { get; set; }
        public int position { get; set; }
        public string ImageName { get; set; }
        public int ImageId;
        public string EnTh { get; set; }

        public ImageRequest()
        {
            tmpPath = string.Empty;
            url = string.Empty;
            position = 0;
            ImageName = string.Empty;
            ImageId = 0;
            EnTh = string.Empty;
        }
    }
}
