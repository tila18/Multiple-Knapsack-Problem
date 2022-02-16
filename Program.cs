using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Knapsack
{
    public class Item
    {
        public double Value;
        public double Weight;
        public bool Used;

        public Item(double value, double weight, bool used)
        {
            Value = value;
            Weight = weight;
            Used = used;
        }
    }

    public class Knapsack
    {
        public double MaxCapacity;
        public double CurrentCapacity;
        public double Value;
        public List<Item> Items;

        public Knapsack(double maxCapacity, double currentCapacity, double value, List<Item> items)
        {
            MaxCapacity = maxCapacity;
            CurrentCapacity = currentCapacity;
            Value = value;
            Items = items;
        }

        public bool Add(Item item)
        {
            if (item.Weight > CurrentCapacity || item.Used) // if item can't fit in knapsack or has already been used
                return false;

            item.Used = true;

            Value += item.Value;
            CurrentCapacity -= item.Weight;

            Items.Add(item);

            return true;
        }

        public bool Remove(Item item)
        {
            if (Items.Remove(item))
            {
                item.Used = false;

                Value -= item.Value;
                CurrentCapacity += item.Weight;

                return true;
            }

            return false;
        }
    }

    public class Solution
    {
        public string Name { get; private set; }
        public Knapsack[] Knapsacks { get; private set; }
        public Item[] Items { get; private set; }

        // create copies of knapsacks and items
        public Solution(string name, Knapsack[] knapsacks, Item[] items)
        {
            Name = name;

            Items = items.Select(i => new Item(
                i.Value, i.Weight, i.Used)).ToArray();

            Knapsacks = new Knapsack[knapsacks.Length];
            for (int i = 0; i < knapsacks.Length; ++i)
            {
                Knapsack ks = knapsacks[i];

                Knapsacks[i] = new Knapsack(ks.MaxCapacity, ks.CurrentCapacity, ks.Value, new List<Item>(ks.Items));

                for (int j = 0; j < ks.Items.Count; ++j)
                {
                    for (int k = 0; k < items.Length; ++k)
                    {
                        if (items[k] == ks.Items[j])
                        {
                            Knapsacks[i].Items[j] = Items[k];
                            break;
                        }
                    }
                }
            }
        }

        public double Value() => Knapsacks.Sum(ks => ks.Value);

        public bool Add(int k1, int i1) => Knapsacks[k1].Add(Items[i1]);
        public bool Remove(int k1, int i1) => Knapsacks[k1].Remove(Knapsacks[k1].Items[i1]);

        public bool Move(int k1, int k2, int i1)
        {
            if (!Remove(k1, i1)) // if the item does not exist in k1
                return false;
            if (!Add(k2, i1)) // if the item cannot be added to k2
                return false;

            return true;
        }
    }
    

    public class Program
    {
        public static void Main(string[] args)
        {
            // input weights
            // input volume(s) of knapsacks

            Console.WriteLine("Input weights: ");
            string[] weightsString = Console.ReadLine().Split(' ');
            Console.WriteLine("Input values: ");
            string[] valuesString = Console.ReadLine().Split(' ');

            Item[] items = new Item[weightsString.Length];

            for(int i=0; i<weightsString.Length; i++)
            {
                items[i] = new Item(double.Parse(valuesString[i]), double.Parse(weightsString[i]), false);
            }

            Console.WriteLine("Input weight capacities of knapsacks: ");
            string[] capString = Console.ReadLine().Split(' ');

            Knapsack[] knapsacks = new Knapsack[capString.Length];

            for (int i = 0; i < capString.Length; ++i)
            {
                knapsacks[i] = new Knapsack(double.Parse(capString[i]), double.Parse(capString[i]), 0, new List<Item>());
            }

            Console.WriteLine("Input 0 for Greedy / 1 for Neighbourhood Search: ");
            int answer = Int32.Parse(Console.ReadLine());
            double result = 0;

            switch (answer)
            {
                case 0:
                    result = GreedyKnapsack(knapsacks, items);
                    break;

                case 1:
                    result = NeighbourhoodSearch(knapsacks, items);
                    break;
            }

            Console.WriteLine("\nResult: " + result);
        }

        public class ItemCompare : IComparer
        {
            public int Compare(Object x, Object y)
            {
                Item item1 = (Item)x;
                Item item2 = (Item)y;
                double cpr1 = (double)item1.Value /
                              (double)item1.Weight;
                double cpr2 = (double)item2.Value /
                              (double)item2.Weight;

                if (cpr1 < cpr2)
                    return 1;

                return cpr1 > cpr2 ? -1 : 0;
            }
        }

        // 1. Greedy Knapsack algorithm 
        public static double GreedyKnapsack(Knapsack[] knapsacks, Item[] items)
        {
            double totalMaxValue = 0;

            // Sort on value/weight
            Array.Sort(items, new ItemCompare());

            Console.WriteLine("-- GREEDY ALGORITHM --");
            Console.WriteLine("");

            foreach (Knapsack knapsack in knapsacks)
            {
                for (int i = 0; i < items.Count(); ++i)
                {
                    // if knapsack is full, break
                    if (knapsack.CurrentCapacity == 0)
                        break;

                    knapsack.Add(items[i]);
                }

                Console.WriteLine("Total value of knapsack: " + knapsack.Value);
                Console.WriteLine("Current capacity of knapsack: " + knapsack.CurrentCapacity + "/" + knapsack.MaxCapacity);
                Console.WriteLine("");

                totalMaxValue += knapsack.Value;
            }

            Console.WriteLine("GREEDY RESULT: " + totalMaxValue + "\n");

            Console.WriteLine("-- END GREEDY ALGORITHM --\n");

            return totalMaxValue;
        }


        // 2. Neighbourhood search
        public static double NeighbourhoodSearch(Knapsack[] knapsacks, Item[] items, int maxSolutions = 512, int maxEpochs = 96)
        {
            double totalMaxValue = 0;
            int totalExaminedSolutions = 0;

            GreedyKnapsack(knapsacks, items); // run greedy to get a feasible starting position

            bool localSolutionFound = false;
            Solution currentSolution = new Solution("default", knapsacks, items);

            Console.WriteLine("-- NEIGHBOURHOOD SEARCH --");

            while (!localSolutionFound && maxEpochs > 0) // loop
            {
                List<Solution> solutions = new List<Solution>();

                Knapsack[] currKnapsacks = currentSolution.Knapsacks;
                Item[] currItems = currentSolution.Items;

                solutions.AddRange(RemoveMoveAddItems(currKnapsacks, currItems, maxSolutions));
                solutions.AddRange(MoveAddItems(currKnapsacks, currItems, maxSolutions));
                solutions.AddRange(RemoveAddItems(currKnapsacks, currItems, maxSolutions));
                solutions.AddRange(AddItems(currKnapsacks, currItems, maxSolutions));

                if (solutions.Count > 0)
                    Console.WriteLine("");

                localSolutionFound = true; // if no solution found, break loop
                double bestSolution = currentSolution.Value();

                foreach (Solution solution in solutions) // find best solution
                {
                    double currSolution = solution.Value();

                    if (currSolution > bestSolution)
                    {
                        bestSolution = currSolution;
                        currentSolution = solution;

                        localSolutionFound = false;
                    }

                    Console.WriteLine("solution|best = " + currSolution + "|" + bestSolution + " = " + solution.Name + "|" + currentSolution.Name);
                }

                Console.WriteLine("\n" + solutions.Count);

                totalExaminedSolutions += solutions.Count;
                --maxEpochs;
            }

            Console.WriteLine("\nTotal Examined Solutions: " + totalExaminedSolutions);

            Console.WriteLine("\nSolution:\n");
            foreach (Knapsack knapsack in currentSolution.Knapsacks)
            {
                Console.WriteLine(" Total value of knapsack: " + knapsack.Value);
                Console.WriteLine(" Current capacity of knapsack: " + knapsack.CurrentCapacity + "/" + knapsack.MaxCapacity);
                foreach (Item item in knapsack.Items)
                {
                    Console.WriteLine("   Item Value/Weight: " + item.Value + "/" + item.Weight);
                }
                Console.WriteLine("");

                totalMaxValue += knapsack.Value;
            }

            Console.WriteLine("NEIGHBOURHOOD RESULT: " + totalMaxValue + "\n");

            Console.WriteLine("-- END NEIGHBOURHOOD SEARCH --");

            return totalMaxValue;
        }

        /// <summary>
        /// Move every item from every knapsack to every other, and try add new item from the pool to the knapsack that an item 
        /// was moved from
        /// </summary>
        public static List<Solution> MoveAddItems(Knapsack[] knapsacks, Item[] items, int maxSolutions)
        {
            List<Solution> result = new List<Solution>();

            if (maxSolutions <= 0)
                return result;

            for (int i = 0; i < knapsacks.Length; ++i)
            {
                for (int j = 0; j < knapsacks.Length; ++j)
                {
                    if (i == j)
                        continue;

                    for (int k = 0; k < knapsacks[i].Items.Count; ++k)
                    {
                        Solution mSolution = new Solution("move", knapsacks, items);

                        if (mSolution.Move(i, j, k))
                        {
                            for (int l = 0; l < mSolution.Items.Length; ++l)
                            {
                                Solution maSolution = new Solution("move&add", mSolution.Knapsacks, mSolution.Items);

                                if (maSolution.Add(i, l))
                                {
                                    result.Add(maSolution);

                                    if (result.Count >= maxSolutions)
                                        return result;
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Try remove every item 2 from knapsack 2, try move every item 1 from every knapsack 1 to every other knapsack 2 and 
        /// add every unused item 3 to knapsack 1
        /// </summary>
        public static List<Solution> RemoveMoveAddItems(Knapsack[] knapsacks, Item[] items, int maxSolutions)
        {
            List<Solution> result = new List<Solution>();

            if (maxSolutions <= 0)
                return result;

            for (int i = 0; i < knapsacks.Length; ++i)
            {
                for (int j = 0; j < knapsacks.Length; ++j)
                {
                    if (i == j)
                        continue;

                    for (int k = 0; k < knapsacks[j].Items.Count; ++k)
                    {
                        Solution rSolution = new Solution("remove", knapsacks, items);

                        if (rSolution.Remove(j, k)) // move item k away from j
                        {
                            for (int l = 0; l < knapsacks[i].Items.Count; ++l)
                            {
                                Solution rmSolution = new Solution("remove&move", rSolution.Knapsacks, rSolution.Items);

                                if (rmSolution.Move(i, j, l)) // move item l from i to j
                                {
                                    for (int m = 0; m < rmSolution.Items.Length; ++m)
                                    {
                                        Solution maSolution = new Solution("remove&move&add", rmSolution.Knapsacks, rmSolution.Items);

                                        if (maSolution.Add(i, m)) // move item from pool to i
                                        {
                                            result.Add(maSolution);

                                            if (result.Count >= maxSolutions)
                                                return result;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Remove every item from every knapsack and add every unused item to knapsack
        /// </summary>
        public static List<Solution> RemoveAddItems(Knapsack[] knapsacks, Item[] items, int maxSolutions)
        {
            List<Solution> result = new List<Solution>();

            if (maxSolutions <= 0)
                return result;

            for (int i = 0; i < knapsacks.Length; ++i)
            {
                for (int j = 0; j < knapsacks[i].Items.Count; ++j)
                {
                    Solution rSolution = new Solution("remove", knapsacks, items);

                    if (rSolution.Remove(i, j)) // move item j away from i
                    {
                        for (int k = 0; k < rSolution.Items.Length; ++k)
                        {
                            Solution aSolution = new Solution("remove&add", rSolution.Knapsacks, rSolution.Items);

                            if (aSolution.Add(i, k)) // move item k from pool to i
                            {
                                result.Add(aSolution);

                                if (result.Count >= maxSolutions)
                                    return result;
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Try add every unused item to knapsack
        /// </summary>
        public static List<Solution> AddItems(Knapsack[] knapsacks, Item[] items, int maxSolutions)
        {
            List<Solution> result = new List<Solution>();

            if (maxSolutions <= 0)
                return result;

            for (int i = 0; i < knapsacks.Length; ++i) // for every knapsack, try add item from the pool
            {
                for (int j = 0; j < items.Length; ++j)
                {
                    Solution aSolution = new Solution("add", knapsacks, items);

                    if (aSolution.Add(i, j))
                    {
                        result.Add(aSolution);

                        if (result.Count >= maxSolutions)
                            return result;
                    }
                }
            }

            return result;
        }
    }
}
