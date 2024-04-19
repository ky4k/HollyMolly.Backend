using HM.BLL.Extensions;
using HM.BLL.Interfaces;
using HM.BLL.Models;
using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HM.BLL.Services;

public class CategoryService(
    HmDbContext context,
    IImageService imageService,
    ILogger<CategoryService> logger
    ) : ICategoryService
{
    public async Task<IEnumerable<CategoryGroupDto>> GetAllCategoryGroups(CancellationToken cancellationToken)
    {
        List<CategoryGroup> groups = await context.CategoryGroups
            .Include(c => c.Categories)
            .ToListAsync(cancellationToken);
        List<CategoryGroupDto> groupsDto = [];
        foreach (var group in groups)
        {
            groupsDto.Add(group.ToCategoryGroupDto());
        }
        return groupsDto;
    }

    public async Task<CategoryGroupDto?> GetCategoryGroupByIdAsync(int categoryGroupId,
        CancellationToken cancellationToken)
    {
        return (await context.CategoryGroups
            .Include(c => c.Categories)
            .FirstOrDefaultAsync(cg => cg.Id == categoryGroupId, cancellationToken))
            ?.ToCategoryGroupDto();
    }

    public async Task<OperationResult<CategoryGroupDto>> CreateCategoryGroupAsync(CategoryGroupCreateDto categoryGroupDto,
        CancellationToken cancellationToken)
    {
        CategoryGroup group = new()
        {
            Name = categoryGroupDto.Name
        };
        try
        {
            await context.CategoryGroups.AddAsync(group, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return new OperationResult<CategoryGroupDto>(true, group.ToCategoryGroupDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating the category group {@category}", categoryGroupDto);
            return new OperationResult<CategoryGroupDto>(false, "The category group has not been created.");
        }
    }

    public async Task<OperationResult> UpdateCategoryGroupAsync(int categoryGroupId,
        CategoryGroupCreateDto categoryGroupDto, CancellationToken cancellationToken)
    {
        CategoryGroup? group = await context.CategoryGroups
            .FirstOrDefaultAsync(cg => cg.Id == categoryGroupId, cancellationToken);
        if (group == null)
        {
            return new OperationResult(false, $"The category group with id {categoryGroupId} does not exist");
        }

        try
        {
            group.Name = categoryGroupDto.Name;
            context.CategoryGroups.Update(group);
            await context.SaveChangesAsync(cancellationToken);
            return new OperationResult(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating the category group {@category}", categoryGroupDto);
            return new OperationResult(false, "The category has not been created.");
        }
    }

    public async Task<OperationResult> UpdateCategoryGroupImageAsync(int categoryGroupId,
        IFormFile image, string baseUrlPath, CancellationToken cancellationToken)
    {
        CategoryGroup? group = await context.CategoryGroups
            .FirstOrDefaultAsync(cg => cg.Id == categoryGroupId, cancellationToken);
        if (group == null)
        {
            return new OperationResult(false, $"The category group with id {categoryGroupId} does not exist");
        }

        try
        {
            string filePath = group.ImageFilePath;
            OperationResult result = await AddImageToCategoryGroupAsync(group, image, baseUrlPath, cancellationToken);

            if (result.Succeeded && !string.IsNullOrEmpty(filePath))
            {
                imageService.DeleteImage(filePath);
            }
            await context.SaveChangesAsync(cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while adding categoryGroup image.");
            return new OperationResult(false, "The image has not been updated.");
        }
    }

    public async Task<OperationResult> DeleteCategoryGroupAsync(int categoryGroupId, CancellationToken cancellationToken)
    {
        CategoryGroup? categoryGroup = await context.CategoryGroups
            .Include(cg => cg.Categories)
            .FirstOrDefaultAsync(cg => cg.Id == categoryGroupId, cancellationToken);
        if (categoryGroup == null)
        {
            return new OperationResult(false, $"The category group with id {categoryGroupId} does not exist");
        }
        if (categoryGroup.Categories.Count != 0)
        {
            return new OperationResult(false, "The category group contains categories and cannot be deleted. " +
                "Delete the subcategories first.");
        }
        try
        {
            imageService.DeleteImage(categoryGroup.ImageFilePath);
            context.CategoryGroups.Remove(categoryGroup);
            await context.SaveChangesAsync(cancellationToken);
            return new OperationResult(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while deleting category group {@categoryGroup}", categoryGroup);
            return new OperationResult(false, "The category has not been deleted.");
        }
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int categoryGroupId,
        int categoryId, CancellationToken cancellationToken)
    {
        return (await context.Categories
            .FirstOrDefaultAsync(cg => cg.Id == categoryId
                && cg.CategoryGroupId == categoryGroupId, cancellationToken))
                ?.ToCategoryDto();
    }

    public async Task<bool> IsCategoryInCategoryGroupAsync(int categoryGroupId,
        int categoryId, CancellationToken cancellationToken)
    {
        return await context.Categories
            .AnyAsync(c => c.Id == categoryId && c.CategoryGroupId == categoryGroupId, cancellationToken);
    }

    public async Task<OperationResult<CategoryDto>> CreateCategoryAsync(int categoryGroupId,
        CategoryCreateDto categoryDto, CancellationToken cancellationToken)
    {
        CategoryGroup? categoryGroup = await context.CategoryGroups
            .FirstOrDefaultAsync(cg => cg.Id == categoryGroupId, cancellationToken);
        if (categoryGroup == null)
        {
            return new OperationResult<CategoryDto>(false,
                $"Category group with the id {categoryGroupId} does not exist");
        }
        try
        {
            Category category = new()
            {
                Name = categoryDto.CategoryName,
                ImageFilePath = "",
                ImageLink = "",
                CategoryGroup = categoryGroup
            };
            await context.Categories.AddAsync(category, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return new OperationResult<CategoryDto>(true, category.ToCategoryDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating the category {@category}", categoryDto);
            return new OperationResult<CategoryDto>(false, "The category has not been created.");
        }
    }

    public async Task<OperationResult> UpdateCategoryAsync(int categoryGroupId, int categoryId,
        CategoryUpdateDto categoryDto, CancellationToken cancellationToken)
    {
        CategoryGroup? categoryGroup = await context.CategoryGroups
            .FirstOrDefaultAsync(cg => cg.Id == categoryDto.CategoryGroupId, cancellationToken);
        if (categoryGroup == null)
        {
            return new OperationResult(false, $"The category group with the id {categoryDto.CategoryGroupId} does not exist");
        }

        Category? category = await context.Categories
            .FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);
        if (category == null)
        {
            return new OperationResult(false, "Category does not exist.");
        }
        if (category.CategoryGroupId != categoryGroupId)
        {
            return new OperationResult(false, $"There are no category with id {categoryId} " +
                $"in the category group with id {categoryGroupId}.");
        }
        try
        {
            category.Name = categoryDto.CategoryName;
            category.CategoryGroupId = categoryDto.CategoryGroupId;
            context.Categories.Update(category);
            await context.SaveChangesAsync(cancellationToken);
            return new OperationResult(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating the category {oldName} to {newName}.",
                category.Name, categoryDto.CategoryName);
            return new OperationResult(false, "Category has not been updated.");
        }
    }

    public async Task<OperationResult> UpdateCategoryImageAsync(int categoryGroupId, int categoryId,
        IFormFile image, string baseUrlPath, CancellationToken cancellationToken)
    {
        Category? category = await context.Categories
            .FirstOrDefaultAsync(cg => cg.Id == categoryId, cancellationToken);
        if (category == null)
        {
            return new OperationResult(false, $"The category with id {categoryId} does not exist");
        }
        if (category.CategoryGroupId != categoryGroupId)
        {
            return new OperationResult(false, $"The category group with id {categoryGroupId} does not contain " +
                $"the category with id {category.Id}");
        }

        try
        {
            string filePath = category.ImageFilePath;
            OperationResult result = await AddImageToCategoryAsync(category, image, baseUrlPath, cancellationToken);
            if (result.Succeeded && !string.IsNullOrEmpty(filePath))
            {
                imageService.DeleteImage(filePath);
            }
            await context.SaveChangesAsync(cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while adding categoryGroup image.");
            return new OperationResult(false, "The image has not been updated.");
        }
    }

    public async Task<OperationResult> DeleteCategoryAsync(int categoryGroupId, int categoryId, CancellationToken cancellationToken)
    {
        Category? category = await context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);
        if (category == null)
        {
            return new OperationResult(false, $"The category with id {categoryId} does not exist");
        }
        if (category.CategoryGroupId != categoryGroupId)
        {
            return new OperationResult(false, $"There are no category with id {categoryId} " +
                $"in the category group with id {categoryGroupId}.");
        }
        if (category.Products.Count != 0)
        {
            return new OperationResult(false, "The category contains products and cannot be deleted.");
        }
        try
        {
            imageService.DeleteImage(category.ImageFilePath);
            context.Categories.Remove(category);
            await context.SaveChangesAsync(cancellationToken);
            return new OperationResult(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while deleting category {@category}", category);
            return new OperationResult(false, "The category has not been deleted.");
        }
    }

    private async Task<OperationResult> AddImageToCategoryAsync(Category category, IFormFile image,
        string baseUrlPath, CancellationToken cancellationToken)
    {
        try
        {
            string savePath = $"images/categories/";
            OperationResult<ImageDto> result = await imageService
                .UploadImageAsync(image, baseUrlPath, savePath, cancellationToken);

            if (result.Payload != null)
            {
                ImageDto imageDto = result.Payload!;
                category.ImageFilePath = imageDto.FilePath;
                category.ImageLink = imageDto.Link;
            }
            await context.SaveChangesAsync(cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while uploading images for the category {@category}", category);
            return new OperationResult(false, "Image has not been added.");
        }
    }

    private async Task<OperationResult> AddImageToCategoryGroupAsync(CategoryGroup categoryGroup,
        IFormFile image, string baseUrlPath, CancellationToken cancellationToken)
    {
        try
        {
            string savePath = $"images/categoryGroups/";
            OperationResult<ImageDto> result = await imageService
                .UploadImageAsync(image, baseUrlPath, savePath, cancellationToken);
            if (result.Payload != null)
            {
                ImageDto imageDto = result.Payload!;
                categoryGroup.ImageFilePath = imageDto.FilePath;
                categoryGroup.ImageLink = imageDto.Link;
            }
            await context.SaveChangesAsync(cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while uploading images for the category group {@categoryGroup}", categoryGroup);
            return new OperationResult(false, "Image has not been added.");
        }
    }
}
