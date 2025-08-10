using Microsoft.EntityFrameworkCore;
using RecipeShare.Models;

namespace RecipeShare.Data
{
    public class RecipeContext : DbContext
    {
        public RecipeContext(DbContextOptions<RecipeContext> options)
            : base(options) { }

        public DbSet<Recipe> Recipe { get; set; }
    }
}
