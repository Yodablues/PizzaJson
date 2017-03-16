using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace ConsoleApplication1
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var pizzaFileContents = GetPizzaConfig();

            //I prefer NewtonSoft to JSON.net for performance - ETC
            var inputConfigs = JsonConvert.DeserializeObject<IList<Topping>>(pizzaFileContents);
            var configurations = HashConfigurations(inputConfigs);
 
            //Grouping by duplicate ToppingHash, then selecting out that SortedSet. Generating a new Topping instance with the count and SortedSet
            //Filtering our any result under 20
            //Applying sort order - ETC
            var results = configurations.GroupBy(o => o.ToppingHash, o => o.Toppings)
                .Select(x => new Topping
                {
                    ToppingHash = x.Key,
                    NumberOfOrders = x.Count(),
                    Toppings = x.First()
                })
                .Where(o=>o.NumberOfOrders >= 20)
                .OrderByDescending(o => o.NumberOfOrders);

            //Back to JSON for output
            var resultsJson = JsonConvert.SerializeObject(results, Formatting.Indented);
            WriteResults(resultsJson);

            foreach (var topping in results)
            {
                Console.WriteLine($"Toppings: {topping.Toppings.Aggregate((i, j) => i + ", " + j)} - Number Of Orders: {topping.NumberOfOrders}");
            }
            Console.WriteLine("");
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }

        /// <summary>
        /// Generates a hashed value from the toppings in a sorted set. Used for easy grouping of duplicated topping configurations
        /// </summary>
        /// <param name="inputConfigs"></param>
        /// <returns>List&lt;Topping&gt; </returns>
        private static IEnumerable<Topping> HashConfigurations(IEnumerable<Topping> inputConfigs)
        {
            var configurations = new List<Topping>();

            foreach (var inputConfig in inputConfigs)
            {
                var topping = new Topping()
                {
                    ToppingHash = inputConfig.Toppings.Aggregate((i, j) => i + j),
                    Toppings = inputConfig.Toppings,
                    NumberOfOrders = 0
                };
                configurations.Add(topping);
            }
            return configurations;
        }

        /// <summary>
        /// Reads in pizza.json file. 
        /// </summary>
        /// <returns></returns>
        private static string GetPizzaConfig()
        {
            if (!File.Exists(Constants.PizzasFile))
            {
                throw new FileNotFoundException($"json not found at file path: { Directory.GetCurrentDirectory()}\\{Constants.PizzasFile}");
            }

            var json = "";

            using (var r = new StreamReader(Constants.PizzasFile))
            {
                json = r.ReadToEnd();
            }

            return json;
        }

        /// <summary>
        /// Writes results file to executing directory.
        /// </summary>
        /// <param name="results"></param>
        private static void WriteResults(string results)
        {
            using (var r = new StreamWriter(Constants.ResultsFile))
            {
                r.WriteLine(results);
                r.Close();
            }

        }
    }
    public class Constants
    {
        public const string PizzasFile = "pizzas.json";
        public const string ResultsFile = "pizzas_results.json";
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Topping
    {
        [JsonProperty]
        public SortedSet<string> Toppings { get; set; }
        [JsonProperty]
        public int NumberOfOrders { get; set; }
        public string ToppingHash { get; set; }
    }
}

