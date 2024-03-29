/*using System;
using System.Collections.Generic;
using Game.Data;

namespace Game.Simulation.Sites.Modules
{
    /// <summary>
    /// A site module that implements storing of products
    /// </summary>
    [SiteModuleId("sitemodule.productstorage")]
    public class ProductStorage : SiteModule
    {
        /// <summary>
        /// Public, read-only view on the products stored in this module.
        /// </summary>
        public IReadOnlyDictionary<ProductType, double> AllStoredProducts => StoredProducts;

        /// <summary>
        /// All currently stored products and their amount, in tons.
        /// </summary>
        protected Dictionary<ProductType, double> StoredProducts { get; set; }
            = new Dictionary<ProductType, double>();

        public ProductStorage(WorldSite parentSite)
            : base(parentSite)
        {
        }

        /// <summary>
        /// Deposit <see cref="amount"/> tons of product <see cref="type"/> into this storage.
        /// </summary>
        /// <param name="type">Type of product to deposit</param>
        /// <param name="amount">Amount of product to deposit, in tons</param>
        public void Deposit(ProductType type, double amount)
        {
            if (amount < 0.0)
                throw new ArgumentException("Can't deposit negative product amount");

            if (amount == 0)
                throw new ArgumentException("Can't deposit zero product amount");

            if (!Has(type))
                Add(type, amount);
            else
            {
                var oldValue = StoredProducts[type];
                StoredProducts[type] = oldValue + amount;
            }
        }

        /// <summary>
        /// Add a new product entry with given amount to the storage. This will throw if there is already
        /// any amount of the given product type stored.
        /// </summary>
        /// <param name="type">Type of product to add</param>
        /// <param name="amount">Amount to add</param>
        public void Add(ProductType type, double amount)
        {
            if (amount < 0.0)
                throw new ArgumentException("Can't add negative product amount");

            if (amount == 0)
                throw new ArgumentException("Can't add zero product amount");

            if (Has(type))
                throw new InvalidOperationException($"Storage already contains entry of type {type.Identifier}");

            StoredProducts.Add(type, amount);
        }

        /// <summary>
        /// Check whether there currently is any product of given type stored in this storage.
        /// </summary>
        /// <param name="type">Type of product to check for</param>
        public bool Has(ProductType type)
        {
            return StoredProducts.ContainsKey(type);
        }

        /// <summary>
        /// Check whether given amount of given product can currently be withdrawn from the storage.
        /// </summary>
        /// <param name="type">Type of product to withdraw</param>
        /// <param name="amount">Amount of product to withdraw</param>
        public bool CanWithdraw(ProductType type, double amount)
        {
            if (amount <= 0.0)
                return false;

            if (!Has(type))
                return false;

            return GetAmount(type) >= amount;
        }

        /// <summary>
        /// Withdraw given amount of product of given type from the storage.
        /// </summary>
        /// <param name="type">Type of product to withdraw</param>
        /// <param name="amount">Amount of product to withdraw</param>
        public void Withdraw(ProductType type, double amount)
        {
            if (!CanWithdraw(type, amount))
                throw new InvalidOperationException($"Can't withdraw {amount}t of product type {type.Identifier}");

            var currentAmount = GetAmount(type);
            var newAmount = currentAmount - amount;

            if (newAmount > 0.0)
                StoredProducts[type] = 0.0;
            else
                StoredProducts.Remove(type);
        }

        /// <summary>
        /// Get amount of stored product of given type
        /// </summary>
        /// <param name="type">Type of product to retrieve amount of</param>
        public double GetAmount(ProductType type)
        {
            if (!Has(type))
                return 0.0;

            return StoredProducts[type];
        }

        /// <summary>
        /// Remove given product type from this storage, no matter how much of it is currently stored.
        /// </summary>
        /// <param name="type">Type of product to remove</param>
        public void Remove(ProductType type)
        {
            StoredProducts.Remove(type);
        }

        public override void Update(int weeks)
        {
            // Do nothing
        }
    }
}*/