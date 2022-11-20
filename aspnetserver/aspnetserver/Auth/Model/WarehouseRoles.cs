namespace aspnetserver.Auth.Model;

public static class WarehouseRoles
{
    public const string Admin = nameof(Admin);
    public const string Worker = nameof(Worker);
    public const string Manager = nameof(Manager);

    public static readonly IReadOnlyCollection<string> All = new[] { Admin, Worker, Manager };
}