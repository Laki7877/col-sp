using System.Collections.Generic;

namespace Colsp.Model.Requests
{
    public class TagRequest
    {
        public List<ProductStageRequest> Products { get; set; }
        public List<string> Tags { get; set; }


        public int TagId { get; set; }
        public string TagName { get; set; }

        public TagRequest()
        {
            TagId = 0;
            TagName = string.Empty;

            Products = new List<ProductStageRequest>();
            Tags = new List<string>();
        }
    }
}
