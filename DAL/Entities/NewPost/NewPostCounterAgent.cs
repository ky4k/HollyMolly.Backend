using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HM.DAL.Entities.NewPost
{
    public class NewPostCounterAgent
    {
        public int Id { get; set; }
        public string Ref { get; set; } = null!; 
        public string? Description { get; set; } 
        public string FirstName { get; set; } = null!;
        public string? MiddleName { get; set; } 
        public string LastName { get; set; } = null!; 
        public string? CounterpartyId { get; set; } 
        public string? OwnershipFormId { get; set; } 
        public string? OwnershipFormDescription { get; set; }
        public string? EDRPOU { get; set; }
        public string? CounterpartyType { get; set; } 
        public ICollection<NewPostContactPerson> ContactPersons { get; set; } = new List<NewPostContactPerson>();
    }
}
