using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace EmployeeDirectoryWebApplication.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Имя")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Отчество")]
        public string Patronymic { get; set; }

        [Required]
        [Display(Name = "Отдел")]
        public int DepartmentId { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        [Display(Name = "Отдел")]
        public Department Department { get; set; }

        [Display(Name = "Телефон")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Фото")]
        public byte[] ProfilePhoto { get; set; }

        [NotMapped]
        [Display(Name = "Фото")]
        public IFormFile ProfilePhotoFile { get; set; }

        [NotMapped]
        [Display(Name = "Фото")]
        public string ProfilePhotoBase64 => ProfilePhoto != null ?
        $"data:image/jpeg;base64,{Convert.ToBase64String(ProfilePhoto)}" :
        "/images/defaultUser.png";

        [NotMapped]
        [Display(Name = "ФИО")]
        public string FullName => $"{LastName} {FirstName} {Patronymic}".Trim();
    }
}
