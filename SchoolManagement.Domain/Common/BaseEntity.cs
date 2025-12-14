using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagement.Domain.Common
{
    public abstract class BaseEntity
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        public Guid Id { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }
        public string CreatedBy { get; protected set; }
        public string? UpdatedBy { get; protected set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; protected set; }
        public string CreatedIP { get; set; }

        // PostgreSQL xmin concurrency token
        [Timestamp]
        public uint RowVersion { get; set; }  // Changed from private set to set

        [NotMapped]
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

        public void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        protected BaseEntity()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            IsDeleted = false;
        }

        public void MarkAsDeleted(string user = "SYSTEM")
        {
            IsDeleted = true;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = user;
        }

        public void SetCreated(string user, string ipAddress)
        {
            CreatedAt = DateTime.UtcNow;
            CreatedBy = user;
            CreatedIP = ipAddress;
        }

        public void SetUpdated(string user, string ipAddress)
        {
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = user;
            // Note: Consider adding UpdatedIP property if you want to track this
            CreatedIP = ipAddress;
        }
    }
}