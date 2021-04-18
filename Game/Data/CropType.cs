using System;
using System.Collections.Generic;
using System.Text;
using Game.Core;

namespace Game.Data
{
    /// <summary>
    /// Describes a type of crop that can be planted by villages
    /// </summary>
    public class CropType : ITypeClass, IEquatable<CropType>
    {
        /// <summary>
        /// Unique identifier of this type
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Human-readable name for this crop
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Short description text for this crop
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Identifier of the <see cref="ProductType"/> that this crop generates
        /// when farmed
        /// </summary>
        public string Product { get; set; }

        /// <summary>
        /// Base crop yield, modified by fertility
        /// </summary>
        public double BaseYield { get; set; }

        /// <summary>
        /// Fertility factors for this type of crop
        /// </summary>
        public CropFertility FertilityFactors { get; set; }

        #region Equality Members
        public bool Equals(CropType other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Identifier == other.Identifier;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CropType)obj);
        }

        public override int GetHashCode()
        {
            return (Identifier != null ? Identifier.GetHashCode() : 0);
        }

        public static bool operator ==(CropType left, CropType right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CropType left, CropType right)
        {
            return !Equals(left, right);
        }
        #endregion
    }
}
