using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using RecipeShare.Controllers;
using RecipeShare.Data;
using RecipeShare.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RecipeShare
{
    public class API_UnitTests
    {
        private readonly DbContextOptions<RecipeContext> _dbOptions;

        public API_UnitTests()
        {
            _dbOptions = new DbContextOptionsBuilder<RecipeContext>()
                .UseInMemoryDatabase(databaseName: "TestRecipeDB")
                .Options;

            // Ensure fresh DB before any test runs
            using var context = new RecipeContext(_dbOptions);
            context.Database.EnsureDeleted();
        }

        private RecipesController GetControllerWithData(IEnumerable<Recipe> seedData)
        {
            var context = new RecipeContext(_dbOptions);
            context.Database.EnsureDeleted();  // Clean before seed
            context.Database.EnsureCreated();

            context.Recipe.AddRange(seedData);
            context.SaveChanges();

            var loggerMock = new Mock<ILogger<RecipesController>>();

            return new RecipesController(context, loggerMock.Object);
        }

        [Fact]
        public async Task GetRecipes_ReturnsAll_WhenNoTag()
        {
            var seed = new List<Recipe>
            {
                new Recipe { Id = 1, Title = "Vegan Salad", DietaryTags = "vegan,healthy" },
                new Recipe { Id = 2, Title = "Chicken Curry", DietaryTags = "meat,spicy" }
            };
            var controller = GetControllerWithData(seed);

            var result = await controller.GetRecipes(null);

            var actionResult = Assert.IsType<ActionResult<IEnumerable<Recipe>>>(result);
            var recipes = Assert.IsAssignableFrom<IEnumerable<Recipe>>(actionResult.Value);
            Assert.Equal(2, recipes.Count());
        }

        [Fact]
        public async Task GetRecipes_FiltersByTag()
        {
            var seed = new List<Recipe>
            {
                new Recipe { Id = 1, Title = "Vegan Salad", DietaryTags = "vegan,healthy" },
                new Recipe { Id = 2, Title = "Chicken Curry", DietaryTags = "meat,spicy" }
            };
            var controller = GetControllerWithData(seed);

            var result = await controller.GetRecipes("vegan");

            var recipes = Assert.IsAssignableFrom<IEnumerable<Recipe>>(result.Value);
            Assert.Single(recipes);
            Assert.Contains(recipes, r => r.Id == 1);
        }

        [Fact]
        public async Task GetRecipe_ReturnsRecipe_WhenFound()
        {
            var seed = new List<Recipe> { new Recipe { Id = 1, Title = "Test Recipe" } };
            var controller = GetControllerWithData(seed);

            var result = await controller.GetRecipe(1);

            var actionResult = Assert.IsType<ActionResult<Recipe>>(result);
            var recipe = Assert.IsType<Recipe>(actionResult.Value);
            Assert.Equal(1, recipe.Id);
        }

        [Fact]
        public async Task GetRecipe_ReturnsNotFound_WhenMissing()
        {
            var controller = GetControllerWithData(new List<Recipe>());

            var result = await controller.GetRecipe(99);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostRecipe_CreatesRecipe()
        {
            var controller = GetControllerWithData(new List<Recipe>());
            var newRecipe = new Recipe { Id = 1, Title = "New Recipe" };

            var result = await controller.PostRecipe(newRecipe);

            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedRecipe = Assert.IsType<Recipe>(createdAtActionResult.Value);
            Assert.Equal("New Recipe", returnedRecipe.Title);
        }

        [Fact]
        public async Task PutRecipe_UpdatesRecipe_WhenValid()
        {
            var seed = new List<Recipe> { new Recipe { Id = 1, Title = "Old Title" } };
            var controller = GetControllerWithData(seed);

            var updatedRecipe = new Recipe { Id = 1, Title = "Updated Title" };

            var result = await controller.PutRecipe(1, updatedRecipe);

            Assert.IsType<NoContentResult>(result);

            var getResult = await controller.GetRecipe(1);
            Assert.Equal("Updated Title", getResult.Value.Title);
        }

        [Fact]
        public async Task PutRecipe_ReturnsBadRequest_WhenIdMismatch()
        {
            var controller = GetControllerWithData(new List<Recipe>());

            var recipe = new Recipe { Id = 1, Title = "Test" };

            var result = await controller.PutRecipe(2, recipe);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task PutRecipe_ReturnsNotFound_WhenRecipeMissing()
        {
            var controller = GetControllerWithData(new List<Recipe>());

            var recipe = new Recipe { Id = 1, Title = "Test" };

            var result = await controller.PutRecipe(1, recipe);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteRecipe_Deletes_WhenExists()
        {
            var seed = new List<Recipe> { new Recipe { Id = 1, Title = "ToDelete" } };
            var controller = GetControllerWithData(seed);

            var result = await controller.DeleteRecipe(1);

            Assert.IsType<NoContentResult>(result);

            var getResult = await controller.GetRecipe(1);
            Assert.IsType<NotFoundResult>(getResult.Result);
        }

        [Fact]
        public async Task DeleteRecipe_ReturnsNotFound_WhenMissing()
        {
            var controller = GetControllerWithData(new List<Recipe>());

            var result = await controller.DeleteRecipe(99);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
