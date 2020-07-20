using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Suda.Else
{
    public static class Global
    {
        public static Cache Cache { get; set; } = Cache.Read();

    }
}
