using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RecipeShare.Data;
using RecipeShare.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecipeShare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipesController : ControllerBase
    {
        private readonly RecipeContext _context;
        private readonly ILogger<RecipesController> _logger;

        public RecipesController(RecipeContext context, ILogger<RecipesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/recipes?tag=vegan
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Recipe>>> GetRecipes([FromQuery] string? tag)
        {
            _logger.LogInformation("GET all recipes requested. Tag filter: {Tag}", tag ?? "none");

            IQueryable<Recipe> query = _context.Recipe.AsQueryable();

            if (!string.IsNullOrEmpty(tag))
            {
                query = query.Where(r => r.DietaryTags != null && r.DietaryTags.Contains(tag));
                _logger.LogInformation("Filtered recipes by tag: {Tag}", tag);
            }

            var recipes = await query.ToListAsync();

            _logger.LogInformation("{Count} recipes returned", recipes.Count);

            return recipes;
        }

        // GET: api/recipes/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Recipe>> GetRecipe(int id)
        {
            _logger.LogInformation("GET recipe requested with id {Id}", id);

            var recipe = await _context.Recipe.FindAsync(id);

            if (recipe == null)
            {
                _logger.LogWarning("Recipe with id {Id} not found", id);
                return NotFound();
            }

            _logger.LogInformation("Recipe with id {Id} found", id);
            return recipe;
        }

        // POST: api/recipes
        [HttpPost]
        public async Task<ActionResult<Recipe>> PostRecipe(Recipe recipe)
        {
            _logger.LogInformation("POST new recipe requested: {Title}", recipe.Title);

            _context.Recipe.Add(recipe);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Recipe created with id {Id}", recipe.Id);

            return CreatedAtAction(nameof(GetRecipe), new { id = recipe.Id }, recipe);
        }

        // PUT: api/recipes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRecipe(int id, Recipe recipe)
        {
            _logger.LogInformation("PUT update recipe requested for id {Id}", id);

            if (id != recipe.Id)
            {
                _logger.LogWarning("PUT recipe id mismatch: route id {RouteId} != recipe id {RecipeId}", id, recipe.Id);
                return BadRequest();
            }

            _context.Entry(recipe).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Recipe with id {Id} updated successfully", id);
            }
            catch (DbUpdateConcurrencyException)
            {
                bool exists = _context.Recipe.Any(e => e.Id == id);
                if (!exists)
                {
                    _logger.LogWarning("PUT update failed: recipe with id {Id} not found", id);
                    return NotFound();
                }
                else
                {
                    _logger.LogError("PUT update failed due to concurrency issue for id {Id}", id);
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/recipes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            _logger.LogInformation("DELETE recipe requested for id {Id}", id);

            var recipe = await _context.Recipe.FindAsync(id);

            if (recipe == null)
            {
                _logger.LogWarning("DELETE failed: recipe with id {Id} not found", id);
                return NotFound();
            }

            _context.Recipe.Remove(recipe);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Recipe with id {Id} deleted successfully", id);

            return NoContent();
        }
    }
}
