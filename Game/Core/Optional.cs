using System;
using System.Reflection.Metadata.Ecma335;

namespace Game.Core
{
    /// <summary>
    /// Represents an optional value that can either be present or missing
    /// </summary>
    public struct Optional<T>
    {
        /// <summary>
        /// Whether the optional value is currently present
        /// </summary>
        public bool HasValue { get; private set; }
        
        /// <summary>
        /// The stored value.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the optional value is not present</exception>
        public T Value
        {
            get
            {
                if (this.HasValue)
                    return this._value;
                else
                    throw new InvalidOperationException("Optional value was not present");
            }
        }
        
        /// <summary>
        /// Empty optional instance without a value present
        /// </summary>
        public static Optional<T> Empty => new Optional<T>();

        /// <summary>
        /// The stored value if present; null otherwise
        /// </summary>
        private T _value;

        /// <summary>
        /// Construct new optional instance with given value present
        /// </summary>
        public Optional(T value)
        {
            this.HasValue = true;
            this._value = value;
        }

        /// <summary>
        /// Remove contained value, if present
        /// </summary>
        public void Reset()
        {
            if (this.HasValue)
            {
                this.HasValue = false;
                this._value = default;
            }
        }

        /// <summary>
        /// Set the value in the optional instance to given value
        /// </summary>
        public void Set(T value)
        {
            this.HasValue = true;
            this._value = value;
        }

        /// <summary>
        /// If value is present, execute given action, with the stored value given as parameter.
        /// </summary>
        public Optional<T> Some(Action<T> action)
        {
            if (this.HasValue)
                action(this._value);

            return this;
        }

        /// <summary>
        /// If value is not present, execute given action.
        /// </summary>
        public Optional<T> None(Action action)
        {
            if (!this.HasValue)
                action();

            return this;
        }

        /// <summary>
        /// Create optional value with given stored value
        /// </summary>
        public static Optional<T> Of(T value)
        {
            return new Optional<T>(value);
        }
        
        /// <summary>
        /// Allow explicit conversion from optional value to contained value.
        /// </summary>
        public static explicit operator T(Optional<T> optional)
        {
            return optional.Value;
        }
        
        /// <summary>
        /// Allow implicit conversion from value to optional instance containing that value
        /// </summary>
        public static implicit operator Optional<T>(T value)
        {
            return new Optional<T>(value);
        }

        #region Equality Checks
        public override bool Equals(object obj)
        {
            if (obj is Optional<T>)
                return this.Equals((Optional<T>)obj);
            else
                return false;
        }
        public bool Equals(Optional<T> other)
        {
            if (HasValue && other.HasValue)
                return object.Equals(this._value, other._value);
            else
                return HasValue == other.HasValue;
        }
        #endregion
    }
}