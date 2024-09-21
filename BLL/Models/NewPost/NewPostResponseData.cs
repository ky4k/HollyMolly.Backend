using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HM.BLL.Models.NewPost
{
    public class NewPostResponseData<T> where T : class
    {
        [JsonPropertyName("data")]
        public List<T> Data { get; set; } = new List<T>();
    }
}
