using HM.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HM.BLL.Models
{
    public class WishListDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public UserDto User { get; set; } = new UserDto();
        public List<ProductDto> Products { get; set; } = new List<ProductDto>();
    }
}
