using AppIt.Data.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppIt.Core.DTOs
{
    public  class FeatureAttributeDto(params FeatureId[] featureIds)
    {
        public IEnumerable<FeatureId> FeatureIds { get; } = featureIds;
    }
}
