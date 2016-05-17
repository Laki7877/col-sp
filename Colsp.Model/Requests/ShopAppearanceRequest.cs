using System.Collections.Generic;

namespace Colsp.Model.Requests
{
    public class ShopAppearanceRequest
    {
        public BannerComponent Banner { get; set; }
        public bool IsBanner { get; set; }
        public List<Layout> Layouts { get; set; }
        public bool IsLayout { get; set; }
        public List<VideoLinkRequest> Videos { get; set; }
        public bool IsVideo { get; set; }
        public int ThemeId { get; set; }
        public string Data { get; set; }


        public ShopAppearanceRequest()
        {
            Banner = new BannerComponent();
            Layouts = new List<Layout>();
            Videos = new List<VideoLinkRequest>();
            IsBanner = false;
            IsLayout = false;
            IsVideo = false;
            Data = string.Empty;
        }
    }
}
