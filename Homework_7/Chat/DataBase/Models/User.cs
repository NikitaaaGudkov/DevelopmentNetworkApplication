using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; } = null!;
        public virtual ICollection<Message> SendedMessages { get; set; } = null!;
        public virtual ICollection<Message> ReceivedMessages { get; set; } = null!;
    }
}
