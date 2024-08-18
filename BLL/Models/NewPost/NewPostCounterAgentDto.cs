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
        public string? Description { get; set; }
        public string FirstName { get; set; } = null!;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = null!;
        public string? CounterpartyId { get; set; }
        public string? OwnershipFormId { get; set; }
        public string? OwnershipFormDescription { get; set; }
        public string? EDRPOU { get; set; }
        public string? CounterpartyType { get; set; }
        public List<NewPostContactPersonDto> ContactPersons { get; set; } = new List<NewPostContactPersonDto>();
    }
}
