using System;
using System.Collections.Generic;
using System.Text;
using AppIt.Data.Enums;

namespace AppIt.Data.EntityModels
{
    public class FeatureAttributes : Attribute
    {
        public IEnumerable<FeatureId> FeatureIds { get; }
        public FeatureAttributes(params FeatureId[] featureIds)
        {
            FeatureIds = featureIds;
        }

    }
}
