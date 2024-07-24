using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HM.BLL.Models.NewPost
{
    public class NewPostResponse<T> where T : class
    {
        public bool Success { get; set; }
        public List<NewPostWarehouse> Data { get; set; } = null!;
        public List<string> Errors { get; set; } = null!;
    }
}
