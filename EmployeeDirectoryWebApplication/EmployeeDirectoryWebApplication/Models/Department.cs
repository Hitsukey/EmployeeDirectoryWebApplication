using System.ComponentModel.DataAnnotations;

namespace EmployeeDirectoryWebApplication.Models
{
    public class Department
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Название отдела")]
        public string Name { get; set; }
    }
}
