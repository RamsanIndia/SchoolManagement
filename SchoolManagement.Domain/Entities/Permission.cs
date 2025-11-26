using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Entities
{
    public class Permission : BaseEntity
    {
        public string Name { get; private set; }
        public string DisplayName { get; private set; }
        public string Description { get; private set; }
        public string Module { get; private set; }
        public string Action { get; private set; }
        public string Resource { get; private set; }
        public bool IsSystemPermission { get; private set; }

        public bool IsDeleted { get; private set; }= false;

        // Navigation Properties
        public virtual ICollection<RolePermission> RolePermissions { get; private set; }

        private Permission()
        {
            RolePermissions = new List<RolePermission>();
        }

        public Permission(string name, string displayName, string module,
                         string action, string resource, string description = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            Module = module ?? throw new ArgumentNullException(nameof(module));
            Action = action ?? throw new ArgumentNullException(nameof(action));
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
            Description = description;
            IsSystemPermission = false;

            RolePermissions = new List<RolePermission>();
        }

        public void Update(string name, string displayName, string module,
                   string action, string resource, string description)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            Module = module ?? throw new ArgumentNullException(nameof(module));
            Action = action ?? throw new ArgumentNullException(nameof(action));
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
            Description = description;
        }

        public void MarkAsSystemPermission()
        {
            IsSystemPermission = true;
        }

        public void MarkAsDeleted()
        {
            IsDeleted = true;
        }
    }
}
