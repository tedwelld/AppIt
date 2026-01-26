using System;
using System.Collections.Generic;
using System.Text;

namespace AppIt.Core.DTOs
{
    public  class FeatureAttributeDto(params string[] featureIds)
    {
        public IEnumerable<string> FeatureIds { get; } = featureIds;
    }
}
