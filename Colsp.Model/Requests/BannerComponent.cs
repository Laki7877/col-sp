using System.Collections.Generic;

namespace Colsp.Model.Requests
{
    public class BannerComponent
    {
        public List<ImageRequest> Images { get; set; }
        public bool AutoPlay { get; set; }

        public BannerComponent()
        {
            Images = new List<ImageRequest>();
            AutoPlay = false;
        }
    }
}
