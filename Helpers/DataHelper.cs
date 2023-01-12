using Contact_Manager_Pro.Data;
using Microsoft.EntityFrameworkCore;

namespace Contact_Manager_Pro.Helpers
{
    public static class DataHelper
    {
        public static async Task ManageDataAsync(IServiceProvider svcProvider)
        {
            // get an instance of the db application context
            var dbContextSvc = svcProvider.GetRequiredService<ApplicationDbContext>();

            // migration: this is equivalent to update-database
            await dbContextSvc.Database.MigrateAsync();
        }
    }
}
