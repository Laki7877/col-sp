namespace Colsp.Model.Requests
{
    public class Layout
    {
        public int CollectionId { get; set; }
        public string CollectionName { get; set; }
        public int Position { get; set; }
        public bool DisplayCountTime { get; set; }

        public Layout()
        {
            CollectionName = string.Empty;
        }
    }
}
