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
        public List<T> Data { get; set; } = null!;
        public List<string> Errors { get; set; } = null!;
        public List<string> TranslatedErrors { get; set; } = null!;
        public List<string> Warnings { get; set; } = null!;
        public List<string> Info { get; set; } = null!;
        public List<string> MessageCodes { get; set; } = null!;
        public List<string> ErrorCodes { get; set; } = null!;
        public List<string> WarningCodes { get; set; } = null!;
        public List<string> InfoCodes { get; set; } = null!;
    }
}
