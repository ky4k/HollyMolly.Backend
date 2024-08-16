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
        public string FirstName { get; set; } = null!; // First name (max 36 characters)
        public string LastName { get; set; } = null!; // Last name (max 36 characters)
        public string MiddleName { get; set; } = null!; // Middle name (max 36 characters)
        public string Email { get; set; }= null!; // Email address
        public string Phone { get; set; } = null!; // Phone number
        public int CounterpartyRef { get; set; } // Foreign key to Counterparty

        // Navigation property back to Counterparty
        public NewPostCounterAgent CounterAgent { get; set; } = null!;
    }
}
