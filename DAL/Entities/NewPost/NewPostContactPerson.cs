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
        public string Ref { get; set; } = null!;
        public string? Description { get; set; }
        public string LastName { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string? MiddleName { get; set; }
        public string Phones { get; set; } = null!;
        public string? Email { get; set; }
        public int CounterAgentId { get; set; } 
        public NewPostCounterAgent? CounterAgent { get; set; }
    }
}
