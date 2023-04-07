using System;

namespace Game.Data
{
    /// <summary>
    /// Represents a type of product that can be stored and processed in sites
    /// </summary>
    public class ProductType : ITypeClass, IEquatable<ProductType>
    {
        /// <summary>
        /// Unique identifier of this type
        /// </summary>
        public string Identifier { get; set; }
        
        /// <summary>
        /// Resource name that is displayed to the player
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// A short, descriptive text about the product
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Whether this is a raw, primary product, like iron ore, wood or grain
        /// </summary>
        public bool IsPrimary { get; set; }
        
        #region Equality Members
        public bool Equals(ProductType other)
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
            return Equals((ProductType) obj);
        }

        public override int GetHashCode()
        {
            return (Identifier != null ? Identifier.GetHashCode() : 0);
        }

        public static bool operator ==(ProductType left, ProductType right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ProductType left, ProductType right)
        {
            return !Equals(left, right);
        }
        #endregion
    }
}