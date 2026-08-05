using Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Marketplace.Identity.Persistence;

public static class RbacSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("RbacSeeder");

        if (await context.Roles.AnyAsync())
        {
            logger.LogInformation("RBAC data already seeded.");
            return;
        }

        logger.LogInformation("Seeding Roles and Permissions...");

        // 1. Roles
        var superAdminRole = Role.Create("SuperAdmin", "System Administrator with full access");
        var financeRole = Role.Create("FinanceManager", "Finance Manager");
        var agencyRole = Role.Create("AgencyRep", "Regional Agency Representative");
        var vendorRole = Role.Create("Vendor", "Shop Vendor");
        var supportRole = Role.Create("SupportAgent", "Customer Support & Content Moderator");
        var deliveryRole = Role.Create("DeliveryAgent", "Delivery / Courier");
        var customerRole = Role.Create("Customer", "Registered Customer");
        var guestRole = Role.Create("Guest", "Guest User");

        var roles = new[] { superAdminRole, financeRole, agencyRole, vendorRole, supportRole, deliveryRole, customerRole, guestRole };
        await context.Roles.AddRangeAsync(roles);

        // 2. Permissions
        var permissionsData = new Dictionary<string, string>
        {
            { "Products.Create", "Create products" },
            { "Products.Read", "Read products" },
            { "Products.Update", "Update products" },
            { "Products.Delete", "Delete products" },
            { "Products.Moderate", "Moderate products" },
            { "Orders.Create", "Create orders" },
            { "Orders.ViewOwn", "View own orders" },
            { "Orders.ViewAll", "View all orders" },
            { "Orders.UpdateStatus", "Update order status" },
            { "Payments.View", "View payments" },
            { "Payments.Process", "Process payments" },
            { "Users.Manage", "Manage users" },
            { "Users.ViewProfile", "View own profile" },
            { "Vendors.Register", "Register as vendor" },
            { "Vendors.Approve", "Approve vendors" },
            { "Reports.Financial", "View financial reports" },
            { "Reports.Invoice", "View invoices" },
            { "Coupons.Create", "Create coupons" },
            { "Coupons.Apply", "Apply coupons" },
            { "Reviews.Create", "Create reviews" },
            { "Reviews.Read", "Read reviews" },
            { "Notifications.View", "View notifications" }
        };

        var permissionsList = new List<Permission>();
        foreach (var p in permissionsData)
        {
            var permission = Permission.Create(p.Key, p.Key, p.Value);
            permissionsList.Add(permission);
        }
        await context.Permissions.AddRangeAsync(permissionsList);
        await context.SaveChangesAsync();

        // 3. Assign Permissions to Roles
        var rolePermissions = new List<RolePermission>();
        
        // SuperAdmin gets all permissions
        foreach (var p in permissionsList)
        {
            rolePermissions.Add(RolePermission.Create(superAdminRole.Id, p.Id));
        }

        // Finance Manager
        rolePermissions.Add(RolePermission.Create(financeRole.Id, permissionsList.First(p => p.Code == "Products.Read").Id));
        rolePermissions.Add(RolePermission.Create(financeRole.Id, permissionsList.First(p => p.Code == "Orders.ViewAll").Id));
        rolePermissions.Add(RolePermission.Create(financeRole.Id, permissionsList.First(p => p.Code == "Payments.View").Id));
        rolePermissions.Add(RolePermission.Create(financeRole.Id, permissionsList.First(p => p.Code == "Payments.Process").Id));
        rolePermissions.Add(RolePermission.Create(financeRole.Id, permissionsList.First(p => p.Code == "Reports.Financial").Id));
        rolePermissions.Add(RolePermission.Create(financeRole.Id, permissionsList.First(p => p.Code == "Reports.Invoice").Id));
        rolePermissions.Add(RolePermission.Create(financeRole.Id, permissionsList.First(p => p.Code == "Reviews.Read").Id));
        rolePermissions.Add(RolePermission.Create(financeRole.Id, permissionsList.First(p => p.Code == "Notifications.View").Id));

        // Vendor
        rolePermissions.Add(RolePermission.Create(vendorRole.Id, permissionsList.First(p => p.Code == "Products.Create").Id));
        rolePermissions.Add(RolePermission.Create(vendorRole.Id, permissionsList.First(p => p.Code == "Products.Read").Id));
        rolePermissions.Add(RolePermission.Create(vendorRole.Id, permissionsList.First(p => p.Code == "Products.Update").Id));
        rolePermissions.Add(RolePermission.Create(vendorRole.Id, permissionsList.First(p => p.Code == "Orders.ViewOwn").Id));
        rolePermissions.Add(RolePermission.Create(vendorRole.Id, permissionsList.First(p => p.Code == "Orders.UpdateStatus").Id));
        rolePermissions.Add(RolePermission.Create(vendorRole.Id, permissionsList.First(p => p.Code == "Users.ViewProfile").Id));
        rolePermissions.Add(RolePermission.Create(vendorRole.Id, permissionsList.First(p => p.Code == "Reports.Invoice").Id));
        rolePermissions.Add(RolePermission.Create(vendorRole.Id, permissionsList.First(p => p.Code == "Coupons.Create").Id));
        rolePermissions.Add(RolePermission.Create(vendorRole.Id, permissionsList.First(p => p.Code == "Reviews.Read").Id));
        rolePermissions.Add(RolePermission.Create(vendorRole.Id, permissionsList.First(p => p.Code == "Notifications.View").Id));

        // Customer
        rolePermissions.Add(RolePermission.Create(customerRole.Id, permissionsList.First(p => p.Code == "Products.Read").Id));
        rolePermissions.Add(RolePermission.Create(customerRole.Id, permissionsList.First(p => p.Code == "Orders.Create").Id));
        rolePermissions.Add(RolePermission.Create(customerRole.Id, permissionsList.First(p => p.Code == "Orders.ViewOwn").Id));
        rolePermissions.Add(RolePermission.Create(customerRole.Id, permissionsList.First(p => p.Code == "Payments.Process").Id));
        rolePermissions.Add(RolePermission.Create(customerRole.Id, permissionsList.First(p => p.Code == "Users.ViewProfile").Id));
        rolePermissions.Add(RolePermission.Create(customerRole.Id, permissionsList.First(p => p.Code == "Vendors.Register").Id));
        rolePermissions.Add(RolePermission.Create(customerRole.Id, permissionsList.First(p => p.Code == "Reports.Invoice").Id));
        rolePermissions.Add(RolePermission.Create(customerRole.Id, permissionsList.First(p => p.Code == "Coupons.Apply").Id));
        rolePermissions.Add(RolePermission.Create(customerRole.Id, permissionsList.First(p => p.Code == "Reviews.Create").Id));
        rolePermissions.Add(RolePermission.Create(customerRole.Id, permissionsList.First(p => p.Code == "Reviews.Read").Id));
        rolePermissions.Add(RolePermission.Create(customerRole.Id, permissionsList.First(p => p.Code == "Notifications.View").Id));

        await context.RolePermissions.AddRangeAsync(rolePermissions);
        await context.SaveChangesAsync();

        // 4. Assign Roles to SuperAdmin User
        var adminUserId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        var userRoles = new List<UserRole>();
        foreach (var role in roles)
        {
            userRoles.Add(UserRole.Create(adminUserId, role.Id));
        }

        await context.UserRoles.AddRangeAsync(userRoles);
        await context.SaveChangesAsync();

        logger.LogInformation("RBAC data seeded successfully.");
    }
}
