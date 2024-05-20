namespace HietakissaUtils.LootTable
{
    using System.Collections.Generic;
    using Random = System.Random;
    using UnityEngine;
    using System;


    [Serializable]
    public class LootTable<TTableItem>
    {
        public List<TableItem> Items => items;
        [SerializeField] List<TableItem> items = new List<TableItem>();
        AliasMethod alias;


        public void BakeTable(int? seed = null)
        {
            if (seed == null) BakeTable(new Random());
            else BakeTable(new Random(seed.Value));
        }

        public void BakeTable(Random random)
        {
            List<double> weights = new List<double>();
            for (int i = 0; i < items.Count; i++)
            {
                weights.Add(items[i].Weight);
            }

            alias = new AliasMethod(weights.ToArray(), random);
        }

        public TTableItem Get()
        {
#if UNITY_EDITOR
            if (alias == null)
            {
                Debug.LogError("Tried to get item from table, but the Alias was null. Baked the table automatically, this will NOT happen in a build. Call LootTable.BakeTable() before using it!");
                BakeTable();
            }
#endif
            return items[alias.Next()].Item;
        }

        public TTableItem[] Get(int count)
        {
            TTableItem[] items = new TTableItem[count];
            for (int i = 0; i < count; i++) items[i] = Get();

            return items;
        }

        public void Add(TTableItem item, double weight)
        {
            items.Add(new TableItem(item, weight));
        }
        public void Remove(TTableItem item)
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (Equals(items[i].Item, item)) items.RemoveAt(i);
            }
        }
        public void Clear() => items.Clear();



        [Serializable]
        public class TableItem
        {
            [HorizontalGroup(2)]
            [SerializeField] TTableItem item;
            [SerializeField, HideInInspector] public double weight;

            public TTableItem Item => item;
            public double Weight => weight;

            public TableItem(TTableItem item, double weight)
            {
                this.item = item;
                this.weight = weight;
            }
        }
    }

    public class AliasMethod
    {
        private readonly int[] alias;
        private readonly double[] probability;
        private readonly int n;
        private readonly Random random;

        public AliasMethod(double[] probabilities, Random random)
        {
            n = probabilities.Length;
            alias = new int[n];
            probability = new double[n];
            this.random = random;


            double sum = 0;
            foreach (double prob in probabilities) sum += prob;

            double[] normalizedProbabilities = new double[n];
            for (int i = 0; i < n; i++) normalizedProbabilities[i] = probabilities[i] * n / sum;


            List<int> small = new List<int>();
            List<int> large = new List<int>();

            for (int i = 0; i < n; i++)
            {
                if (normalizedProbabilities[i] < 1.0) small.Add(i);
                else large.Add(i);
            }

            while (small.Count > 0 && large.Count > 0)
            {
                int smallIndex = small[small.Count - 1];
                int largeIndex = large[large.Count - 1];
                small.RemoveAt(small.Count - 1);
                large.RemoveAt(large.Count - 1);

                probability[smallIndex] = normalizedProbabilities[smallIndex];
                alias[smallIndex] = largeIndex;

                normalizedProbabilities[largeIndex] = (normalizedProbabilities[largeIndex] + normalizedProbabilities[smallIndex]) - 1.0;

                if (normalizedProbabilities[largeIndex] < 1.0)
                {
                    small.Add(largeIndex);
                }
                else
                {
                    large.Add(largeIndex);
                }
            }

            while (large.Count > 0)
            {
                int largeIndex = large[large.Count - 1];
                large.RemoveAt(large.Count - 1);
                probability[largeIndex] = 1.0;
            }

            while (small.Count > 0)
            {
                int smallIndex = small[small.Count - 1];
                small.RemoveAt(small.Count - 1);
                probability[smallIndex] = 1.0;
            }
        }

        public int Next()
        {
            int column = random.Next(n);
            bool coinToss = random.NextDouble() < probability[column];
            return coinToss ? column : alias[column];
        }
    }
}