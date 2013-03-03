using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CubeIt.Tests
{
    /// <summary>
    /// This test class will perform various calculations based on data within different date ranges.
    /// </summary>
    [TestClass]
    public class DateRangeTester
    {
        private static readonly Dimension month = new Dimension(EqualityComparer<int>.Default);
        private static readonly Dimension week = new Dimension(EqualityComparer<int>.Default);
        private static readonly Dimension dayOfWeek = new Dimension(EqualityComparer<DayOfWeek>.Default);

        /// <summary>
        /// We can use the Cube to find the total sales for a month (a dimension).
        /// </summary>
        [TestMethod]
        public void TestCube_TotalSalesForJanuary()
        {
            Cube<decimal> original = getSalesCube();
            /* We have the sales amounts broken out across month, week and day of week.
             * We only want to see sales for the month of January, so we splice it out
             * of the cube.
             */
            Cube<decimal> januaryCube = original.Splice(new KeyPart(month, 1));
            /* Since we filtered by month, we know that the month dimension has a
             * single key part value in it (January). We can safely remove the dimension
             * from the cube. We'll Sum the values in the table, although First would
             * have the same effect.
             */
            Cube<decimal> totalCube = januaryCube.Collapse(month, groupSum);
            /* Now we want to summarize by week. Again, we can just sum the values.
             * Here, we might actually have multiple sales in the same week.
             */
            totalCube = totalCube.Collapse(week, groupSum);
            /* At this point, we have the total sales for each day of week during the month.
             * This could be useful information; however, we just want the grand total,
             * so we will sum up the values for each day of the week.
             */
            totalCube = totalCube.Collapse(dayOfWeek, groupSum);
            /* At this point we have eliminate all of the dimensions in the table. We can
             * grab the total value with an empty key.
             */
            decimal total = totalCube[new Key()];
            Assert.AreEqual(54.24m, total, "The wrong grand total for January was generated.");
            /* We also want to know how many sales we had in total.
             */
            Cube<int> countCube = januaryCube.Collapse(month, groupCount).Collapse(week, groupCount).Collapse<int>(dayOfWeek, groupCount);
            int count = countCube[new Key()];
            Assert.AreEqual(3, count, "The wrong number of sales were found for January.");
        }

        private static decimal groupSum(Group<decimal> group)
        {
            return group.Pairs.Sum(pair => pair.Value);
        }

        private static int groupCount<T>(Group<T> group)
        {
            return group.Pairs.Count();
        }

        private Cube<decimal> getSalesCube()
        {
            Cube<decimal> result = Cube<decimal>.Define(month, week, dayOfWeek);
            foreach (Sale sale in getYearlySales())
            {
                Key key = new Key(
                    new KeyPart(month, sale.Date.Month),
                    new KeyPart(week, sale.Date.DayOfYear / 7),
                    new KeyPart(dayOfWeek, sale.Date.DayOfWeek));
                Cube<decimal> cube = Cube<decimal>.Singleton(key, sale.Amount);
                result = result.ResolvingMerge(groupSum, cube);
            }
            return result;
        }

        private static decimal groupSum(Collision<decimal> group)
        {
            return group.Values.Sum();
        }

        private IEnumerable<Sale> getYearlySales()
        {
            return new Sale[]
            {
                new Sale() { Date = new DateTime(2012, 01, 01), Amount = 12.53m },
                new Sale() { Date = new DateTime(2012, 01, 13), Amount = 24.89m },
                new Sale() { Date = new DateTime(2012, 01, 31), Amount = 16.82m },
                new Sale() { Date = new DateTime(2012, 02, 18), Amount = 2.34m },
                new Sale() { Date = new DateTime(2012, 02, 22), Amount = 2.34m },
                new Sale() { Date = new DateTime(2012, 02, 23), Amount = 2.34m },
                new Sale() { Date = new DateTime(2012, 03, 03), Amount = 2.34m },
                new Sale() { Date = new DateTime(2012, 03, 12), Amount = 2.34m },
                new Sale() { Date = new DateTime(2012, 03, 29), Amount = 2.34m },
                new Sale() { Date = new DateTime(2012, 03, 30), Amount = 2.34m },
                new Sale() { Date = new DateTime(2012, 04, 11), Amount = 2.34m },
                new Sale() { Date = new DateTime(2012, 05, 01), Amount = 2.34m },
                new Sale() { Date = new DateTime(2012, 05, 01), Amount = 2.34m },
                new Sale() { Date = new DateTime(2012, 05, 02), Amount = 2.34m },
                new Sale() { Date = new DateTime(2012, 05, 02), Amount = 2.34m },
                new Sale() { Date = new DateTime(2012, 06, 22), Amount = 2.34m },
                new Sale() { Date = new DateTime(2012, 07, 05), Amount = 2.34m },
                new Sale() { Date = new DateTime(2012, 07, 14), Amount = 2.34m },
                new Sale() { Date = new DateTime(2012, 07, 21), Amount = 2.34m },
                new Sale() { Date = new DateTime(2012, 08, 03), Amount = 2.34m },
                new Sale() { Date = new DateTime(2012, 09, 24), Amount = 2.34m },
                new Sale() { Date = new DateTime(2012, 10, 01), Amount = 2.34m },
                new Sale() { Date = new DateTime(2012, 10, 03), Amount = 2.34m },
                new Sale() { Date = new DateTime(2012, 10, 07), Amount = 2.34m },
                new Sale() { Date = new DateTime(2012, 10, 11), Amount = 2.34m },
                new Sale() { Date = new DateTime(2012, 10, 16), Amount = 2.34m },
                new Sale() { Date = new DateTime(2012, 11, 17), Amount = 2.34m },
                new Sale() { Date = new DateTime(2012, 11, 18), Amount = 2.34m },
                new Sale() { Date = new DateTime(2012, 12, 02), Amount = 2.34m },
                new Sale() { Date = new DateTime(2012, 12, 22), Amount = 2.34m },
            };
        }

        private class Sale
        {
            public DateTime Date { get; set; }

            public decimal Amount { get; set; }
        }
    }
}
