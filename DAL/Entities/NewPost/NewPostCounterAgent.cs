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
        public string Ref { get; set; } = null!; // Identifier (REF) of the contact person
        public string Description { get; set; } = null!; // Description of the counterparty in Ukrainian (max 50 characters)
        public string FirstName { get; set; } = null!; // First name (max 36 characters)
        public string MiddleName { get; set; } = null!; // Middle name (max 36 characters)
        public string LastName { get; set; } = null!; // Last name (max 36 characters)
        public string CounterpartyId { get; set; } = null!; // Identifier of the counterparty
        public string OwnershipFormId { get; set; } = null!; // Identifier of the ownership form
        public string OwnershipFormDescription { get; set; } = null!; // Description of the ownership form (max 36 characters)
        public string EDRPOU { get; set; } = null!; // EDRPOU code (max 36 characters)
        public string CounterpartyType { get; set; } = null!; // Type of counterparty (max 36 characters)

        // Navigation property for ContactPerson
        public ICollection<NewPostContactPerson> ContactPersons { get; set; } = null!;
    }
}
