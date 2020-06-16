using System;

namespace Game.Core
{
    /// <summary>
    /// Enumeration describing the different value states an either instance can
    /// be in
    /// </summary>
    public enum ValuePresence
    {
        Left,
        Right
    }

    /// <summary>
    /// Class containing either an instance of type L, or an instance of type R
    /// </summary>
    /// <typeparam name="L"></typeparam>
    /// <typeparam name="R"></typeparam>
    public class Either<L, R>
    {
        private object _value;

        private ValuePresence _presence;

        /// <summary>
        /// Whether the left alternative is present
        /// </summary>
        public bool HasLeft => this._presence == ValuePresence.Left;

        /// <summary>
        /// Whether the right alternative is present
        /// </summary>
        public bool HasRight => this._presence == ValuePresence.Right;

        /// <summary>
        /// Current value presence state
        /// </summary>
        public ValuePresence Presence => this._presence;

        /// <summary>
        /// Retrieve value of left alternative, if present
        /// </summary>
        public L Left
        {
            get
            {
                if (!this.HasLeft)
                    throw new InvalidOperationException("Left value is not present");

                return (L) this._value;
            }
        }

        /// <summary>
        /// Retrieve value of right alternative, if present
        /// </summary>
        public R Right
        {
            get
            {
                if (!this.HasRight)
                    throw new InvalidOperationException("Right value is not present");

                return (R) this._value;
            }
        }

        /// <summary>
        /// Construct either instance with left alternative value present
        /// </summary>
        public Either(L value)
        {
            this._value = value;
            this._presence = ValuePresence.Left;
        }

        /// <summary>
        /// Construct either instance with right alternative value present
        /// </summary>
        public Either(R value)
        {
            this._value = value;
            this._presence = ValuePresence.Right;
        }

        /// <summary>
        /// Set either to contain given value for left alternative
        /// </summary>
        public void SetValue(L value)
        {
            this._value = value;
            this._presence = ValuePresence.Left;
        }

        /// <summary>
        /// Set either to contain given value for right alternative
        /// </summary>
        public void SetValue(R value)
        {
            this._value = value;
            this._presence = ValuePresence.Right;
        }

        /// <summary>
        /// Create new either value with left value present
        /// </summary>
        public static Either<L, R> WithLeft(L value)
        {
            return new Either<L, R>(value);
        }
        
        /// <summary>
        /// Create new either value with right value present
        /// </summary>
        public static Either<L, R> WithRight(R value)
        {
            return new Either<L, R>(value);
        }

        /// <summary>
        /// Unpack either instance using given transformation functions.
        /// </summary>
        public T Unpack<T>(Func<L, T> left, Func<R, T> right)
        {
            return this._presence switch
            {
                ValuePresence.Left => left.Invoke(this.Left),
                ValuePresence.Right => right.Invoke(this.Right)
            };
        }
    }
}