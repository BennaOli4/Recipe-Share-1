using System;
using System.Net.Http;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace RecipeBenchmark
{
    public class RecipeApiBenchmark
    {
        private readonly HttpClient _client = new HttpClient();

        [Benchmark]
        public async Task GetRecipes500Times()
        {
            for (int i = 0; i < 500; i++)
            {
                var response = await _client.GetAsync("http://localhost:5258/api/recipes");
                response.EnsureSuccessStatusCode();
                await response.Content.ReadAsStringAsync();
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<RecipeApiBenchmark>();
        }
    }
}
