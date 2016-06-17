namespace Colsp.Model.Requests
{
    public class AttributeValueRequest
    {
        public int AttributeValueId { get; set; }
        public string AttributeValueEn { get; set; }
        public string AttributeValueTh { get; set; }
        public bool CheckboxValue { get; set; }
        public string Status { get; set; }
        public int Position { get; set; }
        public ImageRequest Image { get; set; }
		public string ImageUrl { get; set; }


		public AttributeValueRequest()
        {
            AttributeValueId = 0;
            AttributeValueEn = string.Empty;
            AttributeValueTh = string.Empty;
            Status = string.Empty;
            Position = 0;
			ImageUrl = string.Empty;
			Image = new ImageRequest();
        }
    }
}
