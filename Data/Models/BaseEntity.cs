namespace Data.Models;

using Data.Contracts;

public class BaseEntity : IEntity
{
    public Guid Id { get; set; }
    public long Created { get; set; }
    public long LastModified { get; set; }
}