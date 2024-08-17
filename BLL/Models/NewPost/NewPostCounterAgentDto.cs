using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HM.BLL.Models.NewPost
{
    public class NewPostCounterAgentDto
    {
        public string Ref { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string MiddleName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string CounterpartyId { get; set; } = null!;
        public string OwnershipFormId { get; set; } = null!;
        public string OwnershipFormDescription { get; set; } = null!;
        public string EDRPOU { get; set; } = null!;
        public string CounterpartyType { get; set; } = null!;
        public List<NewPostContactPersonDto> ContactPersons { get; set; } = new();
    }
}
