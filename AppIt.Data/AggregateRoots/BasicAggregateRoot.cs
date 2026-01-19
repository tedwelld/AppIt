using System;
using System.Collections.Generic;
using System.Text;

namespace AppIt.Data.AggregateRoots
{
    public class BasicAggregateRoot<T>
    {
        public T Id { get; set; }
    }
}
