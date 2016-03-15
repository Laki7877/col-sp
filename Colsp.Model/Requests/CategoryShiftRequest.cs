namespace Colsp.Model.Requests
{
    public class CategoryShiftRequest
    {
        public int Parent { get; set; }
        public int Sibling { get; set; }
        public int Child { get; set; }

        public CategoryShiftRequest()
        {
            Parent = 0;
            Sibling = 0;
            Child = 0;
        }
    }
}
