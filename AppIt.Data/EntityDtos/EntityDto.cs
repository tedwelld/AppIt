using System;
using System.Collections.Generic;
using System.Text;

namespace AppIt.Data.EntityDtos
{
    public class EntityDto<T>
    {
        public T Id { get; set; } = default!;
    }
}
