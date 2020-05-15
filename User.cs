using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TestMiddleDB
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public int Identity { get; set; }
        public string FIO { get; set; }
        public string City { get; set; }
        public string Mail { get; set; }
        public string Number { get; set; }
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
    }
}
