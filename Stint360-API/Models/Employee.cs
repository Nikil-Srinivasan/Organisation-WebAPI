using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Organisation_WebAPI.Models
{
    public class Employee
    {
       
        public Employee()
        {
            EmployeeTasks = new List<EmployeeTask>();
        }
        
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] 
        public int EmployeeID { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Designation { get; set; }
        public string? Address { get; set; }
        public string ?EmployeeName { get; set; }
        public int EmployeeSalary { get; set; }
        public int EmployeeAge { get; set; }
        [ForeignKey("User")]
        public int? UserID { get; set; }

        [ForeignKey("Manager")]
        public int? ManagerID { get; set; }
        public Manager? Manager { get; set; }
        public User? User { get; set; }

        [JsonIgnore]
        public ICollection<EmployeeTask> EmployeeTasks { get; set; }


    }
}