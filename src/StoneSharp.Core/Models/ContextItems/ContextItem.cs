using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneSharp.Core.Models.ContextItems
{
    public abstract class ContextItem
    {
        public abstract bool Equals(ContextItem other);
    }
}
