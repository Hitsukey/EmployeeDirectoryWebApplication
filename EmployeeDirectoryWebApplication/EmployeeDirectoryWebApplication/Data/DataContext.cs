using EmployeeDirectoryWebApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeDirectoryWebApplication.Data
{
    public class DataContext : DbContext
    {
        public DbSet<Department> Department { get; set; }
        public DbSet<Employee> Employee { get; set; }

        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }
    }
}
