namespace Country.Domain.Common
{
    public class Entity
    {
        public Guid Id { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }

        protected Entity()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }

        public Entity(Guid id)
        {
            Id= id;
            CreatedAt= DateTime.UtcNow;
        }

    }
}
