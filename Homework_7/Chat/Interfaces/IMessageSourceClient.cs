using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IMessageSourceClient
    {
        public string? Name { get; }
        public Task Run();
    }
}
