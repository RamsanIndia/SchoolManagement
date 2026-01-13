// Persistence/TenantQueryFilter.cs - MULTI-TENANT HIERARCHICAL (TenantId + SchoolId)
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Common;
using System;
using System.Linq.Expressions;

namespace SchoolManagement.Persistence
{
    /// <summary>
    /// Hierarchical multi-tenant query filter (TenantId → SchoolId)
    /// 1. Always filters TenantId
    /// 2. SchoolId optional (null = tenant-wide)
    /// </summary>
    public class TenantQueryFilter
    {
        private readonly ITenantService _tenantService;

        public TenantQueryFilter(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        /// <summary>
        /// Creates composite TenantId + SchoolId filter
        /// </summary>
        public LambdaExpression CreateFilter(Type entityType, bool includeSchoolFilter = true)
        {
            var param = Expression.Parameter(entityType, "e");

            // ✅ ALWAYS: TenantId filter
            var tenantIdProp = Expression.Property(param, nameof(BaseEntity.TenantId));
            var currentTenantId = Expression.Constant(_tenantService.TenantId, typeof(Guid));
            var tenantFilter = Expression.Equal(tenantIdProp, currentTenantId);

            Expression filter = tenantFilter;

            // ✅ OPTIONAL: SchoolId filter
            if (includeSchoolFilter && _tenantService.IsSchoolSet)
            {
                var schoolIdProp = Expression.Property(param, nameof(BaseEntity.SchoolId));
                var currentSchoolId = Expression.Constant(_tenantService.SchoolId!.Value, typeof(Guid));
                var schoolFilter = Expression.Equal(schoolIdProp, currentSchoolId);

                filter = Expression.AndAlso(tenantFilter, schoolFilter);
            }

            return Expression.Lambda(filter, param);
        }

        /// <summary>
        /// TenantId ONLY (admin/tenant-wide queries)
        /// </summary>
        public LambdaExpression CreateTenantFilterOnly(Type entityType) =>
            CreateFilter(entityType, includeSchoolFilter: false);

        /// <summary>
        /// Full hierarchy (normal operations)
        /// </summary>
        public LambdaExpression CreateFullFilter(Type entityType) =>
            CreateFilter(entityType, includeSchoolFilter: true);
    }
}
