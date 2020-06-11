using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Game.Core
{
    /// <summary>
    /// An entry in a weighted collection. Contains a value and its weight.
    /// </summary>
    /// <typeparam name="T">Type of the stored value</typeparam>
    public class WeightedEntry<T>
    {
        /// <summary>
        /// The weight of this entry in the collection. Determines how likely it is to get picked.
        /// </summary>
        /// <remarks>
        /// Note that this is not a probability; Any number of any scale can be used here. It's how the
        /// different weights are in relation to each other that determines the actual probability.
        /// </remarks>
        public double Weight { get; }

        /// <summary>
        /// The actual value this entry is associated with.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Construct new entry from given value and weight
        /// </summary>
        public WeightedEntry(double weight, T value)
        {
            this.Weight = weight;
            this.Value = value;
        }
    }

    /// <summary>
    /// A collection of pairs, each consisting of a value and its weight. Allows the user to pick random elements from the
    /// collection, with probabilities based on the elements weights.
    /// </summary>
    public class WeightedCollection<T> : IEnumerable<WeightedEntry<T>>
    {
        /// <summary>
        /// All entries registered with this distribution, in their non-normalized state. Note that this collection
        /// is not used for the actual algorithm, only for re-normalization purposes when new entries
        /// are added after-the-fact.
        /// </summary>
        protected List<WeightedEntry<T>> Entries { get; } = new List<WeightedEntry<T>>();

        /// <summary>
        /// The current sum of all weights in <see cref="Entries"/>.
        /// </summary>
        protected double Sum { get; set; }

        /// <summary>
        /// Construct empty weighted collection
        /// </summary>
        public WeightedCollection()
        {
            
        }

        /// <summary>
        /// Construct weighted collection with given values, each having the exact same weight of 1.0.
        /// </summary>
        public WeightedCollection(params T[] values)
        {
            foreach (var value in values)
            {
                this.Add(1.0, value);
            }
        }

        /// <summary>
        /// Add new entry. This is supplied to allow simpler collection initialization.
        /// </summary>
        public void Add(double probability, T value)
        {
            this.Add(new WeightedEntry<T>(probability, value));
        }

        /// <summary>
        /// Add new entry. This will cause a re-normalization to happen when the next
        /// value is requested by the user.
        /// </summary>
        public void Add(WeightedEntry<T> entry)
        {
            this.Entries.Add(entry);
            this.Sum += entry.Weight;
        }

        /// <summary>
        /// Retrieve enumerator to the stored entries. Note that this references the non-normalized
        /// entries.
        /// </summary>
        public IEnumerator<WeightedEntry<T>> GetEnumerator()
        {
            return this.Entries.GetEnumerator();
        }

        /// <summary>
        /// Retrieve enumerator to the stored entries. Note that this references the non-normalized
        /// entries.
        /// </summary>
        /// <returns>Enumerator</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Append given weighted collection to this instance.
        /// </summary>
        public WeightedCollection<T> Append(WeightedCollection<T> other)
        {
            this.Entries.AddRange(other);
            this.Sum += other.Sum;
            return this;
        }

        /// <summary>
        /// Pick next random value.
        /// </summary>
        /// <param name="rng">Random number generator instance to use</param>
        /// <returns>A new value, picked at random from the distribution</returns>
        public T Next(Random rng)
        {
            if(this.Entries.Count == 0)
                throw new InvalidOperationException("Can't pick next value of empty weighted collection");
            
            // Random double in [0, sum)
            var randomValue = rng.NextDouble() * this.Sum;

            var runningSum = 0.0;

            foreach (var entry in this.Entries)
            {
                runningSum += entry.Weight;

                if (randomValue <= runningSum)
                    return entry.Value;
            }

            // This should not happen!
            return this.Entries.Last().Value;
        }
    }
}