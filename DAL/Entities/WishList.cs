﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HM.DAL.Entities
{
    public class WishList
    {
        public int Id { get; set; }
        public string UserId { get; set; } 
        public User User { get; set; }
        public List<Product> Products { get; set; } = new List<Product>();
    }
}
