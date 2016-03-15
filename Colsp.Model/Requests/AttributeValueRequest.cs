namespace Colsp.Model.Requests
{
    public class AttributeValueRequest
    {
        public int AttributeValueId { get; set; }
        public string AttributeValueEn { get; set; }
        public string AttributeValueTh { get; set; }
        public string Status { get; set; }
        public ImageRequest Image { get; set; }

        public AttributeValueRequest()
        {
            AttributeValueId = 0;
            AttributeValueEn = string.Empty;
            AttributeValueTh = string.Empty;
            Status = string.Empty;
            Image = new ImageRequest();
        }
    }
}
