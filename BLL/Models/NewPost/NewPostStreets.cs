using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HM.BLL.Models.NewPost
{
    public  class NewPostStreets
    {
        public string Ref {  get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string StreetsTypeRef { get; set; } = string.Empty;
        public string StreetsType { get; set; } = string.Empty;
    }
}
