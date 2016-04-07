namespace Colsp.Model.Requests
{
    public class ImageRequest
    {
        //public string tmpPath { get; set; }
        public string Url { get; set; }
        public int Position { get; set; }
        //public string ImageName { get; set; }
        public int ImageId;
        //public string EnTh { get; set; }
        public decimal SlideDuration { get; set; }

        public ImageRequest()
        {
            //tmpPath = string.Empty;
            Url = string.Empty;
            Position = 0;
            //ImageName = string.Empty;
            ImageId = 0;
            SlideDuration = 0;
            //EnTh = string.Empty;
        }
    }
}
