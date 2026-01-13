// Infrastructure/Data/DataSeeder.cs - COMPLETE CORRECTED VERSION
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Domain.ValueObjects;
using SchoolManagement.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.Data
{
    public static class DataSeeder
    {
        // Fixed GUIDs matching your PostgreSQL inserts
        public static readonly Guid GlobalTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public static readonly Guid GreenValleySchoolId = Guid.Parse("a1b2c3d4-5678-90ab-cdef-111111111111");
        public static readonly Guid EliteAcademySchoolId = Guid.Parse("a1b2c3d4-5678-90ab-cdef-222222222222");

        public static async Task SeedAsync(SchoolManagementDbContext context, ILogger logger)
        {
            try
            {
                await SeedTenantsAsync(context, logger);
                await SeedSchoolsAsync(context, logger);
                await SeedRolesAsync(context, logger);
                await SeedPermissionsAsync(context, logger);
                await SeedRolePermissionsAsync(context, logger);
                await SeedMenusAsync(context, logger);
                await SeedRoleMenuPermissionsAsync(context, logger);
                await SeedAdminUserAsync(context, logger);

                logger.LogInformation("✅ All data seeded successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error seeding data");
                throw;
            }
        }

        // Reflection helper to set protected properties
        private static void SetProtectedProperty(object entity, string propertyName, object value)
        {
            var property = entity.GetType()
                .GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (property != null && property.CanWrite)
            {
                property.SetValue(entity, value);
            }
        }

        private static async Task SeedTenantsAsync(SchoolManagementDbContext context, ILogger logger)
        {
            var exists = await context.Tenants
                .IgnoreQueryFilters()
                .AnyAsync(t => t.Code == "GLOBAL");

            if (exists)
            {
                logger.LogInformation("GLOBAL tenant already exists");
                return;
            }

            // ✅ FIXED: Include ALL required columns (TenantId + all BaseEntity fields)
            var sql = @"
        INSERT INTO public.""Tenants"" 
        (""Id"", ""TenantId"", ""Code"", ""Name"", ""IsActive"", ""CreatedAt"", ""CreatedBy"", 
         ""CreatedIP"", ""UpdatedAt"", ""UpdatedBy"", ""UpdatedIP"", ""IsDeleted"", ""DeletedAt"", ""DeletedBy"")
        VALUES 
        ('11111111-1111-1111-1111-111111111111', 
         '11111111-1111-1111-1111-111111111111',  -- ✅ TenantId (self-reference)
         'GLOBAL', 
         'Global Tenant', 
         true, 
         NOW(), 
         'SYSTEM SEED', 
         '127.0.0.1', 
         NULL, NULL, NULL, 
         false, NULL, NULL);";

            await context.Database.ExecuteSqlRawAsync(sql);
            logger.LogInformation("✅ Seeded GLOBAL tenant (full columns)");
        }

        private static async Task SeedSchoolsAsync(SchoolManagementDbContext context, ILogger logger)
        {
            if (await context.Schools.AnyAsync(s => !s.IsDeleted))
            {
                logger.LogInformation("Schools already seeded");
                return;
            }

            // Use raw SQL to bypass protected setters
            var sql = @"
                INSERT INTO school.""Schools"" 
                (""Id"", ""Name"", ""Code"", ""Type"", ""Address"", ""ContactPhone"", ""ContactEmail"", 
                 ""MaxStudentCapacity"", ""Status"", ""FoundedDate"", ""IsActive"", ""TenantId"", ""SchoolId"", 
                 ""CreatedAt"", ""CreatedBy"", ""CreatedIP"", ""UpdatedAt"", ""UpdatedBy"", ""UpdatedIP"", 
                 ""IsDeleted"", ""DeletedAt"", ""DeletedBy"")
                VALUES 
                ('a1b2c3d4-5678-90ab-cdef-111111111111', 'Green Valley Public School', 'GVPS', 1, 
                 '123 Education St, Dadri, UP 203207', '0120-1234567', 'contact@gvps.edu.in', 1200, 1, 
                 '1985-03-15 00:00:00+00', true, '11111111-1111-1111-1111-111111111111', 
                 'a1b2c3d4-5678-90ab-cdef-111111111111', NOW(), 'SYSTEM SEED', '127.0.0.1', 
                 NULL, NULL, NULL, false, NULL, NULL),
                
                ('a1b2c3d4-5678-90ab-cdef-222222222222', 'Elite International Academy', 'EIA', 3, 
                 '456 Learning Ave, Greater Noida, UP 201310', '0120-2345678', 'info@eia.ac.in', 800, 1, 
                 '1992-07-22 00:00:00+00', true, '11111111-1111-1111-1111-111111111111', 
                 'a1b2c3d4-5678-90ab-cdef-222222222222', NOW(), 'SYSTEM SEED', '127.0.0.1', 
                 NULL, NULL, NULL, false, NULL, NULL)
                ON CONFLICT (""Code"") DO NOTHING;";

            await context.Database.ExecuteSqlRawAsync(sql);
            logger.LogInformation("✅ Seeded 2 schools via SQL");
        }

        private static async Task SeedRolesAsync(SchoolManagementDbContext context, ILogger logger)
        {
            if (await context.Roles.AnyAsync())
            {
                logger.LogInformation("Roles already seeded");
                return;
            }

            var roles = new List<Role>
            {
                new Role("SuperAdmin", "Super Administrator", "Full system access with all privileges", true, 1),
                new Role("Admin", "Administrator", "Administrative access to manage school operations", true, 2),
                new Role("Principal", "Principal", "Principal with high-level access", true, 3),
                new Role("Teacher", "Teacher", "Teacher with academic management access", true, 4),
                new Role("Student", "Student", "Student with learning access", true, 5),
                new Role("Parent", "Parent", "Parent with student monitoring access", true, 6),
                new Role("Staff", "Staff", "Staff with operational access", true, 7)
            };

            foreach (var role in roles)
            {
                role.SetCreated("System", "127.0.0.1");

                // SuperAdmin is global, others are school-specific
                if (role.Name == "SuperAdmin")
                {
                    SetProtectedProperty(role, "TenantId", GlobalTenantId);
                    SetProtectedProperty(role, "SchoolId", Guid.Empty);
                }
                else
                {
                    SetProtectedProperty(role, "TenantId", GlobalTenantId);
                    SetProtectedProperty(role, "SchoolId", GreenValleySchoolId);
                }
            }

            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();

            logger.LogInformation($"✅ Seeded {roles.Count} roles");
        }

        private static async Task SeedPermissionsAsync(SchoolManagementDbContext context, ILogger logger)
        {
            if (await context.Permissions.AnyAsync())
            {
                logger.LogInformation("Permissions already seeded");
                return;
            }

            var permissions = new List<Permission>
            {
                // User Management
                new Permission("users.view", "View Users", "UserManagement", "View", "Users", "View user list and details"),
                new Permission("users.create", "Create User", "UserManagement", "Create", "Users", "Create new users"),
                new Permission("users.edit", "Edit User", "UserManagement", "Edit", "Users", "Edit existing users"),
                new Permission("users.delete", "Delete User", "UserManagement", "Delete", "Users", "Delete users"),

                // Role Management
                new Permission("roles.view", "View Roles", "RoleManagement", "View", "Roles", "View role list and details"),
                new Permission("roles.create", "Create Role", "RoleManagement", "Create", "Roles", "Create new roles"),
                new Permission("roles.edit", "Edit Role", "RoleManagement", "Edit", "Roles", "Edit existing roles"),
                new Permission("roles.delete", "Delete Role", "RoleManagement", "Delete", "Roles", "Delete roles"),

                // Permission Management
                new Permission("permissions.view", "View Permissions", "PermissionManagement", "View", "Permissions", "View permissions"),
                new Permission("permissions.assign", "Assign Permissions", "PermissionManagement", "Assign", "Permissions", "Assign permissions to roles"),

                // Student Management
                new Permission("students.view", "View Students", "StudentManagement", "View", "Students", "View student list and details"),
                new Permission("students.create", "Create Student", "StudentManagement", "Create", "Students", "Create new students"),
                new Permission("students.edit", "Edit Student", "StudentManagement", "Edit", "Students", "Edit student information"),
                new Permission("students.delete", "Delete Student", "StudentManagement", "Delete", "Students", "Delete students"),

                // Teacher Management
                new Permission("teachers.view", "View Teachers", "TeacherManagement", "View", "Teachers", "View teacher list and details"),
                new Permission("teachers.create", "Create Teacher", "TeacherManagement", "Create", "Teachers", "Create new teachers"),
                new Permission("teachers.edit", "Edit Teacher", "TeacherManagement", "Edit", "Teachers", "Edit teacher information"),
                new Permission("teachers.delete", "Delete Teacher", "TeacherManagement", "Delete", "Teachers", "Delete teachers"),

                // Class Management
                new Permission("classes.view", "View Classes", "ClassManagement", "View", "Classes", "View class information"),
                new Permission("classes.manage", "Manage Classes", "ClassManagement", "Manage", "Classes", "Create and manage classes"),

                // Attendance
                new Permission("attendance.view", "View Attendance", "Attendance", "View", "Attendance", "View attendance records"),
                new Permission("attendance.mark", "Mark Attendance", "Attendance", "Create", "Attendance", "Mark student attendance"),
                new Permission("attendance.edit", "Edit Attendance", "Attendance", "Edit", "Attendance", "Edit attendance records"),

                // Grades
                new Permission("grades.view", "View Grades", "Grades", "View", "Grades", "View student grades"),
                new Permission("grades.manage", "Manage Grades", "Grades", "Manage", "Grades", "Create and manage grades"),

                // Reports
                new Permission("reports.view", "View Reports", "Reports", "View", "Reports", "View system reports"),
                new Permission("reports.export", "Export Reports", "Reports", "Export", "Reports", "Export reports"),

                // System Settings
                new Permission("settings.view", "View Settings", "System", "View", "Settings", "View system settings"),
                new Permission("settings.manage", "Manage Settings", "System", "Manage", "Settings", "Manage system settings")
            };

            foreach (var permission in permissions)
            {
                permission.SetCreated("System", "127.0.0.1");
                permission.MarkAsSystemPermission();

                // All permissions are global
                SetProtectedProperty(permission, "TenantId", GlobalTenantId);
                SetProtectedProperty(permission, "SchoolId", Guid.Empty);
            }

            await context.Permissions.AddRangeAsync(permissions);
            await context.SaveChangesAsync();

            logger.LogInformation($"✅ Seeded {permissions.Count} permissions");
        }

        private static async Task SeedRolePermissionsAsync(SchoolManagementDbContext context, ILogger logger)
        {
            if (await context.RolePermissions.AnyAsync())
            {
                logger.LogInformation("Role permissions already seeded");
                return;
            }

            var superAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
            var allPermissions = await context.Permissions.ToListAsync();

            if (superAdminRole == null || !allPermissions.Any())
            {
                logger.LogWarning("⚠️ Roles or permissions not found");
                return;
            }

            var rolePermissions = new List<RolePermission>();

            foreach (var permission in allPermissions)
            {
                var rolePermission = new RolePermission(superAdminRole.Id, permission.Id, true);
                rolePermission.SetCreated("System", "127.0.0.1");

                SetProtectedProperty(rolePermission, "TenantId", GlobalTenantId);
                SetProtectedProperty(rolePermission, "SchoolId", Guid.Empty);

                rolePermissions.Add(rolePermission);
            }

            await context.RolePermissions.AddRangeAsync(rolePermissions);
            await context.SaveChangesAsync();

            logger.LogInformation($"✅ Seeded {rolePermissions.Count} role permissions");
        }

        // FIXED SeedAdminUserAsync - Use GreenValleySchoolId for admin (domain requires SchoolId != Empty)
        private static async Task SeedAdminUserAsync(SchoolManagementDbContext context, ILogger logger)
        {
            if (await context.Users.AnyAsync())
            {
                logger.LogInformation("Users already seeded");
                return;
            }

            var superAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");

            // ✅ FIXED: Bypass query filter + use known GUID directly
            var greenValleySchoolId = Guid.Parse("a1b2c3d4-5678-90ab-cdef-111111111111");

            // Domain requires SchoolId != Empty - use GVPS directly
            var adminUser = User.Create(
                tenantId: GlobalTenantId,
                schoolId: greenValleySchoolId,  // ✅ Direct GUID - no query needed
                username: "admin",
                email: new Email("admin@sms.com"),
                fullName: new FullName("System", "Administrator"),
                passwordHash: BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                userType: UserType.SuperAdmin,
                createdBy: "System",
                createdIp: "127.0.0.1"
            );

            adminUser.Activate("System");
            adminUser.UpdatePhoneNumber(new PhoneNumber("+919999999999"), "System");
            adminUser.ClearDomainEvents();

            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync();

            var userRole = SchoolManagement.Domain.Entities.UserRole.Create(
                adminUser.Id,
                superAdminRole!.Id,
                "System"
            );
            userRole.SetCreated("System", "127.0.0.1");
            userRole.ClearDomainEvents();

            await context.UserRoles.AddAsync(userRole);
            await context.SaveChangesAsync();

            logger.LogInformation("✅ Seeded SuperAdmin: admin@sms.com / Admin@123 (GVPS)");
        }



        private static async Task SeedMenusAsync(SchoolManagementDbContext context, ILogger logger)
        {
            if (await context.Menus.AnyAsync())
            {
                logger.LogInformation("Menus already seeded");
                return;
            }

            var menus = new List<Menu>
            {
                new Menu("Dashboard", "Dashboard", "/dashboard", MenuType.Menu, "dashboard", "Main dashboard", null),
                new Menu("UserManagement", "User Management", null, MenuType.Module, "users", "Manage system users", null),
                new Menu("StudentManagement", "Student Management", null, MenuType.Module, "students", "Manage students", null),
                new Menu("TeacherManagement", "Teacher Management", null, MenuType.Module, "teachers", "Manage teachers", null),
                new Menu("Academic", "Academic", null, MenuType.Module, "academic", "Academic management", null),
                new Menu("Reports", "Reports", "/reports", MenuType.Report, "reports", "System reports", null),
                new Menu("Settings", "Settings", "/settings", MenuType.Menu, "settings", "System settings", null)
            };

            for (int i = 0; i < menus.Count; i++)
            {
                menus[i].SetSortOrder(i + 1);
                menus[i].SetCreated("System", "127.0.0.1");

                SetProtectedProperty(menus[i], "TenantId", GlobalTenantId);
                SetProtectedProperty(menus[i], "SchoolId", Guid.Empty);
            }

            await context.Menus.AddRangeAsync(menus);
            await context.SaveChangesAsync();

            var userMgmt = menus.First(m => m.Name == "UserManagement");
            var studentMgmt = menus.First(m => m.Name == "StudentManagement");
            var teacherMgmt = menus.First(m => m.Name == "TeacherManagement");
            var academic = menus.First(m => m.Name == "Academic");

            var subMenus = new List<Menu>
            {
                new Menu("Users", "Users", "/users", MenuType.SubMenu, "user-list", "User list", userMgmt.Id),
                new Menu("Roles", "Roles", "/roles", MenuType.SubMenu, "role-list", "Role management", userMgmt.Id),
                new Menu("Permissions", "Permissions", "/permissions", MenuType.SubMenu, "permission-list", "Permission management", userMgmt.Id),
                new Menu("Students", "Students", "/students", MenuType.SubMenu, "student-list", "Student list", studentMgmt.Id),
                new Menu("Admissions", "Admissions", "/admissions", MenuType.SubMenu, "admission", "Student admissions", studentMgmt.Id),
                new Menu("Teachers", "Teachers", "/teachers", MenuType.SubMenu, "teacher-list", "Teacher list", teacherMgmt.Id),
                new Menu("Classes", "Classes", "/classes", MenuType.SubMenu, "class", "Class management", academic.Id),
                new Menu("Attendance", "Attendance", "/attendance", MenuType.SubMenu, "attendance", "Attendance management", academic.Id),
                new Menu("Grades", "Grades", "/grades", MenuType.SubMenu, "grades", "Grade management", academic.Id)
            };

            for (int i = 0; i < subMenus.Count; i++)
            {
                subMenus[i].SetSortOrder(i + 1);
                subMenus[i].SetCreated("System", "127.0.0.1");

                SetProtectedProperty(subMenus[i], "TenantId", GlobalTenantId);
                SetProtectedProperty(subMenus[i], "SchoolId", Guid.Empty);
            }

            await context.Menus.AddRangeAsync(subMenus);
            await context.SaveChangesAsync();

            logger.LogInformation($"✅ Seeded {menus.Count + subMenus.Count} menus");
        }

        private static async Task SeedRoleMenuPermissionsAsync(SchoolManagementDbContext context, ILogger logger)
        {
            if (await context.RoleMenuPermissions.AnyAsync())
            {
                logger.LogInformation("Role menu permissions already seeded");
                return;
            }

            var superAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
            var allMenus = await context.Menus.ToListAsync();

            if (superAdminRole == null || !allMenus.Any())
            {
                logger.LogWarning("⚠️ Roles or menus not found");
                return;
            }

            var roleMenuPermissions = new List<RoleMenuPermission>();

            var fullPermissions = new MenuPermissions
            {
                CanView = true,
                CanAdd = true,
                CanEdit = true,
                CanDelete = true,
                CanExport = true,
                CanPrint = true,
                CanApprove = true,
                CanReject = true
            };

            foreach (var menu in allMenus)
            {
                var roleMenuPermission = new RoleMenuPermission(superAdminRole.Id, menu.Id, fullPermissions);
                roleMenuPermission.SetCreated("System", "127.0.0.1");

                SetProtectedProperty(roleMenuPermission, "TenantId", GlobalTenantId);
                SetProtectedProperty(roleMenuPermission, "SchoolId", Guid.Empty);

                roleMenuPermissions.Add(roleMenuPermission);
            }

            await context.RoleMenuPermissions.AddRangeAsync(roleMenuPermissions);
            await context.SaveChangesAsync();

            logger.LogInformation($"✅ Seeded {roleMenuPermissions.Count} role menu permissions");
        }
    }
}