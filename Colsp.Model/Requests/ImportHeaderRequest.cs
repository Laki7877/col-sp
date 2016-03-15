namespace Colsp.Model.Requests
{
    public class ImportHeaderRequest
    {
        public int    ImportHeaderId    { get; set; }
        public string HeaderName        { get; set; }
        public string Description       { get; set; }
        public string AcceptedValue     { get; set; }
        public string Example           { get; set; }
        public string Note              { get; set; }
        public string GroupName         { get; set; }
        public object AttributeValue    { get; set; }
        public bool   IsAttribute       { get; set; } 
        public string AttributeType     { get; set; }
        public bool?  IsVariant         { get; set; }

        public ImportHeaderRequest()
        {
            ImportHeaderId = 0;
            HeaderName     = string.Empty;
            Description    = string.Empty;
            AcceptedValue  = string.Empty;
            Example        = string.Empty;
            Note           = string.Empty;
            GroupName      = string.Empty;
            IsAttribute    = false;
            AttributeType  = string.Empty;
            IsVariant      = false;
            AttributeValue = new object();
        }
    }
}
