using SchoolManagement.Domain.Common;
using SchoolManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Entities
{

    public class Tenant : BaseEntity
    {
        public string Name { get; private set; }
        public string Code { get; private set; }
        public string ConnectionString { get; private set; }
        public TenantPlan Plan { get; private set; }
        public DateTime? SubscriptionExpiryDate { get; private set; }
        public int MaxSchools { get; private set; }
        public int MaxUsers { get; private set; }

        // Navigation properties
        private readonly List<School> _schools = new();
        public IReadOnlyCollection<School> Schools => _schools.AsReadOnly();

        // Computed properties
        public int CurrentSchoolCount => _schools.Count;
        public bool IsSubscriptionActive => !SubscriptionExpiryDate.HasValue ||
            SubscriptionExpiryDate.Value > DateTime.UtcNow;

        private Tenant() : base()
        {
            // TenantId and SchoolId remain null for Tenant entity
        }

        public Tenant(string name, string code, TenantPlan plan = TenantPlan.Basic) : base()
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Tenant name is required", nameof(name));
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Tenant code is required", nameof(code));

            Name = name;
            Code = code.ToUpperInvariant();
            Plan = plan;
            SetPlanLimits(plan);
        }

        private void SetPlanLimits(TenantPlan plan)
        {
            switch (plan)
            {
                case TenantPlan.Basic:
                    MaxSchools = 1;
                    MaxUsers = 50;
                    break;
                case TenantPlan.Standard:
                    MaxSchools = 5;
                    MaxUsers = 200;
                    break;
                case TenantPlan.Premium:
                    MaxSchools = 20;
                    MaxUsers = 1000;
                    break;
                case TenantPlan.Enterprise:
                    MaxSchools = -1; // Unlimited
                    MaxUsers = -1;   // Unlimited
                    break;
            }
        }

        public void UpdateName(string name, string userName = null, string ipAddress = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Tenant name cannot be empty", nameof(name));

            Name = name;
            SetUpdated(userName, ipAddress);
        }

        public void UpdateSubscription(TenantPlan plan, DateTime expiryDate, string userName = null, string ipAddress = null)
        {
            Plan = plan;
            SubscriptionExpiryDate = expiryDate;
            SetPlanLimits(plan);
            SetUpdated(userName, ipAddress);
        }

        public void SetConnectionString(string connectionString, string userName = null, string ipAddress = null)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be empty", nameof(connectionString));

            ConnectionString = connectionString;
            SetUpdated(userName, ipAddress);
        }

        public void UpdateCode(string code, string userName = null, string ipAddress = null)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Tenant code cannot be empty", nameof(code));

            Code = code.ToUpperInvariant();
            SetUpdated(userName, ipAddress);
        }

        public bool CanAddSchool()
        {
            return MaxSchools == -1 || CurrentSchoolCount < MaxSchools;
        }
    }


}
