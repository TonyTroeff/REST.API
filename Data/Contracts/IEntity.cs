namespace Data.Contracts;

public interface IEntity
{
    Guid Id { get; set; }
    
    // NOTE: Tony Troeff, 28/09/2022 - If we were to implement authentication and authorization within this seminar, we should consider including more information here, e.g. Created by or Last modified by.
    long Created { get; set; }
    long LastModified { get; set; }
}