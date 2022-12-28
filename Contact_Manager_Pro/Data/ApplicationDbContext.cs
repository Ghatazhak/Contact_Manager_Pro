using Contact_Manager_Pro.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Contact_Manager_Pro.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser> // Need to add custom model for user here. Remember to do this. <AppUser> in this application.
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }
    }
}