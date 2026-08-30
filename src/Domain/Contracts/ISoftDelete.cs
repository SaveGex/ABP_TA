namespace Domain.Contracts
{
    public interface ISoftDelete
    {
        DateTime? DeletedAt { get; }
        void Delete();
    }
}
