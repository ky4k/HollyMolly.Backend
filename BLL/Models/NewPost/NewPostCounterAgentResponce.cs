using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HM.BLL.Models.NewPost
{
    public class NewPostCounterAgentResponce
    {
        public class CounterAgent
        {
            [JsonPropertyName("Ref")]
            public string Ref { get; set; } = null!;

            [JsonPropertyName("Description")]
            public string Description { get; set; } = null!;

            [JsonPropertyName("FirstName")]
            public string FirstName { get; set; } = null!;

            [JsonPropertyName("MiddleName")]
            public string MiddleName { get; set; } = null!;

            [JsonPropertyName("LastName")]
            public string LastName { get; set; } = null!;

            [JsonPropertyName("Counterparty")]
            public string CounterpartyRef { get; set; } = null!;

            [JsonPropertyName("OwnershipForm")]
            public string OwnershipForm { get; set; } = null!;

            [JsonPropertyName("OwnershipFormDescription")]
            public string OwnershipFormDescription { get; set; } = null!;

            [JsonPropertyName("EDRPOU")]
            public string EDRPOU { get; set; } = null!;

            [JsonPropertyName("CounterpartyType")]
            public string CounterpartyType { get; set; } = null!;

            [JsonPropertyName("ContactPerson")]
            public List<ContactPerson> ContactPersons { get; set; } = new List<ContactPerson>();
        }

        public class ContactPerson
        {
            [JsonPropertyName("Description")]
            public string Description { get; set; } = null!;

            [JsonPropertyName("Ref")]
            public string Ref { get; set; } = null!;

            [JsonPropertyName("LastName")]
            public string LastName { get; set; } = null!;

            [JsonPropertyName("FirstName")]
            public string FirstName { get; set; } = null!;

            [JsonPropertyName("MiddleName")]
            public string MiddleName { get; set; } = null!;
        }
    }
}
