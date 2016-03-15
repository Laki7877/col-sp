namespace Colsp.Model.Requests
{
    public class PermissionRequest : PaginatedRequest
    {
        public int? PermissionId { get; set; }
        public string PermissionName { get; set; }
        public string PermissionGroup { get; set; }

        public PermissionRequest(string PermissionName, string PermissionGroup)
        {
            this.PermissionName = PermissionName;
            this.PermissionGroup = PermissionGroup;
        }
    }
}
