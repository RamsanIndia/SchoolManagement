using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagement.Domain.Common
{
    public abstract class BaseEntity
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        public Guid Id { get; protected set; }

        // Audit properties - CreatedAt is required, others are optional
        public DateTime CreatedAt { get; set; }  // NOT nullable - every entity must have creation date
        public string? CreatedBy { get; set; }
        public string? CreatedIP { get; set; }

        public DateTime? UpdatedAt { get; set; }  // Nullable - entity might not be updated yet
        public string? UpdatedBy { get; set; }
        public string? UpdatedIP { get; set; }

        // Soft delete support
        public bool IsDeleted { get; protected set; } = false;
        public DateTime? DeletedAt { get; set; }  // Nullable - entity might not be deleted
        public string? DeletedBy { get; set; }

        // Active status
        public bool IsActive { get; set; } = true;

        // PostgreSQL xmin concurrency token
        [Timestamp]
        public uint RowVersion { get; set; }

        // Domain events
        [NotMapped]
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected BaseEntity()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            IsDeleted = false;
            IsActive = true;
        }

        // Domain event management
        public void AddDomainEvent(IDomainEvent domainEvent)
        {
            if (domainEvent == null)
                throw new ArgumentNullException(nameof(domainEvent));

            _domainEvents.Add(domainEvent);
        }

        public void RemoveDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Remove(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        public void SetCreated(string userName, string ipAddress)
        {
            CreatedBy = userName ?? "System";
            CreatedAt = DateTime.UtcNow;
            CreatedIP = ipAddress ?? "Unknown";
        }

        public void SetUpdated(string userName, string ipAddress)
        {
            UpdatedBy = userName ?? "System";
            UpdatedAt = DateTime.UtcNow;
            UpdatedIP = ipAddress ?? "Unknown";
        }

        // Soft delete with full audit trail
        public void MarkAsDeleted(string userName = null)
        {
            IsDeleted = true;
            IsActive = false;
            DeletedAt = DateTime.UtcNow;
            DeletedBy = userName ?? "System";

            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = userName;
        }

        // Restore from soft delete
        public void Restore(string userName = null)
        {
            IsDeleted = false;
            IsActive = true;
            DeletedAt = null;
            DeletedBy = null;

            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = userName ?? "System";
        }

        // Deactivate without deleting
        public void Deactivate(string userName = null)
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = userName ?? "System";
        }

        // Activate
        public void Activate(string userName = null)
        {
            if (IsDeleted)
                throw new InvalidOperationException("Cannot activate a deleted entity. Use Restore() instead.");

            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = userName ?? "System";
        }
    }
}
