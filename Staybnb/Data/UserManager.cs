using Microsoft.EntityFrameworkCore;
using Staybnb.Models;
namespace Staybnb.Data
{
    public class UserManager: DbContext
    {
        public UserManager(DbContextOptions<UserManager> options) : base(options) { }
    }
}
