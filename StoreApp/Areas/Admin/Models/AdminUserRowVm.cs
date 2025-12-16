namespace StoreApp.Areas.Admin.Models
{
    public class AdminUserRowVm
    {
        public string Id { get; set; } = "";
        public string UserName { get; set; } = "";
        public List<string> Roles { get; set; } = new();
    }
}
