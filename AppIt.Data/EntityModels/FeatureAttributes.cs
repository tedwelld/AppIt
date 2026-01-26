using System;
using System.Collections.Generic;
using System.Text;
using AppIt.Data.Enums;

namespace AppIt.Data.EntityModels
{
    public class FeatureAttributes : Attribute
    {
        // Store feature identifiers as string constants (e.g., "Reservations" or "1")

        public IEnumerable<string> FeatureIds { get; }

        public FeatureAttributes(params string[] featureIds)
        {
            FeatureIds = featureIds;
        }

    }
}
