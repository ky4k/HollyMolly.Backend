using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HM.DAL.Entities.NewPost
{
    public class NewPostContactPerson
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!; 
        public string LastName { get; set; } = null!;
        public string? MiddleName { get; set; } 
        public string? Email { get; set; } 
        public string? Phone { get; set; } 
        public int CounterpartyRef { get; set; } 
        public NewPostCounterAgent? CounterAgent { get; set; }
    }
}
