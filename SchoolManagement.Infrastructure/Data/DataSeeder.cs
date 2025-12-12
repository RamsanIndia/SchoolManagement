// Infrastructure/Data/DataSeeder.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Domain.ValueObjects;
using SchoolManagement.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(SchoolManagementDbContext context, ILogger logger)
        {
            try
            {
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
                rolePermissions.Add(rolePermission);
            }

            await context.RolePermissions.AddRangeAsync(rolePermissions);
            await context.SaveChangesAsync();

            logger.LogInformation($"✅ Seeded {rolePermissions.Count} role permissions");
        }

        private static async Task SeedAdminUserAsync(SchoolManagementDbContext context, ILogger logger)
        {
            if (await context.Users.AnyAsync())
            {
                logger.LogInformation("Users already seeded");
                return;
            }

            var superAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");

            if (superAdminRole == null)
            {
                logger.LogWarning("⚠️ SuperAdmin role not found");
                return;
            }

            var adminUser = User.Create(
                username: "admin",
                email: new Email("admin@sms.com"),
                fullName: new FullName("System", "Administrator"),
                passwordHash: BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                userType: UserType.Staff,
                createdBy: "System",
                createdIp: "127.0.0.1",
                correlationId: Guid.NewGuid().ToString()
            );

            adminUser.Activate("System");
            adminUser.UpdatePhoneNumber(new PhoneNumber("+919999999999"), "System");

            // Clear domain events to skip outbox processing during seeding
            adminUser.ClearDomainEvents();

            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync();

            var userRole = SchoolManagement.Domain.Entities.UserRole.Create(
                adminUser.Id,
                superAdminRole.Id,
                "System"
            );
            userRole.SetCreated("System", "127.0.0.1");
            userRole.ClearDomainEvents();  // Clear events from UserRole too

            await context.UserRoles.AddAsync(userRole);
            await context.SaveChangesAsync();

            logger.LogInformation("✅ Seeded admin user: admin@sms.com / Admin@123");
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
                roleMenuPermissions.Add(roleMenuPermission);
            }

            await context.RoleMenuPermissions.AddRangeAsync(roleMenuPermissions);
            await context.SaveChangesAsync();

            logger.LogInformation($"✅ Seeded {roleMenuPermissions.Count} role menu permissions");
        }
    }
}
