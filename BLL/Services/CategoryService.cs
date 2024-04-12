using HM.BLL.Interfaces;
using HM.BLL.Models;
using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HM.BLL.Services;

public class CategoryService(
    HmDbContext context,
    ILogger<CategoryService> logger
    ) : ICategoryService
{
    public async Task<IEnumerable<string>> GetAllCategories(CancellationToken cancellationToken)
    {
        return await context.Categories.Select(c => c.Name).ToListAsync(cancellationToken);
    }

    public async Task<OperationResult> CreateCategoryAsync(CategoryCreateDto category, CancellationToken cancellationToken)
    {
        try
        {
            await context.Categories.AddAsync(new Category() { Name = category.CategoryName }, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return new OperationResult(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating the category {@category}", category);
            return new OperationResult(false, "The category has not been created.");
        }
    }

    public async Task<OperationResult> UpdateCategoryAsync(string oldCategoryName,
        CategoryCreateDto updatedCategory, CancellationToken cancellationToken)
    {
        Category? category = await context.Categories
            .FirstOrDefaultAsync(c => c.Name == oldCategoryName, cancellationToken);
        if (category == null)
        {
            return new OperationResult(false, "Category does not exist.");
        }
        try
        {
            category.Name = updatedCategory.CategoryName;
            context.Update(category);
            await context.SaveChangesAsync(cancellationToken);
            return new OperationResult(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating the category {oldName} to {newName}.",
                oldCategoryName, updatedCategory.CategoryName);
            return new OperationResult(false, "Category has not been updated.");
        }
    }

    public async Task<OperationResult> DeleteCategoryAsync(string categoryName, CancellationToken cancellationToken)
    {
        Category? category = await context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Name == categoryName, cancellationToken);
        if (category == null)
        {
            return new OperationResult(false, "Category does not exist.");
        }
        if (category.Products.Count != 0)
        {
            return new OperationResult(false, "The category contains products and cannot be deleted.");
        }
        try
        {
            context.Remove(category);
            await context.SaveChangesAsync(cancellationToken);
            return new OperationResult(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while deleting category {@category}", category);
            return new OperationResult(false, "The category has not been deleted.");
        }
    }
}
