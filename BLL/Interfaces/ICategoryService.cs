using HM.BLL.Models;
using Microsoft.AspNetCore.Http;

namespace HM.BLL.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryGroupDto>> GetAllCategoryGroups(CancellationToken cancellationToken);
    Task<CategoryGroupDto?> GetCategoryGroupByIdAsync(int categoryGroupId, CancellationToken cancellationToken);
    Task<OperationResult<CategoryGroupDto>> CreateCategoryGroupAsync(
        CategoryGroupCreateDto categoryGroupDto, CancellationToken cancellationToken);
    Task<OperationResult> UpdateCategoryGroupAsync(int categoryGroupId,
        CategoryGroupCreateDto categoryGroupDto, CancellationToken cancellationToken);
    Task<OperationResult> UpdateCategoryGroupImageAsync(int categoryGroupId,
        IFormFile image, string baseUrlPath, CancellationToken cancellationToken);
    Task<OperationResult> DeleteCategoryGroupAsync(int categoryGroupId, CancellationToken cancellationToken);

    Task<CategoryDto?> GetCategoryByIdAsync(int categoryGroupId, int categoryId, CancellationToken cancellationToken);
    Task<bool> IsCategoryInCategoryGroupAsync(int categoryGroupId, int categoryId, CancellationToken cancellationToken);
    Task<OperationResult<CategoryDto>> CreateCategoryAsync(int categoryGroupId,
        CategoryCreateDto categoryDto, CancellationToken cancellationToken);
    Task<OperationResult> UpdateCategoryAsync(int categoryGroupId, int categoryId,
        CategoryUpdateDto categoryDto, CancellationToken cancellationToken);
    Task<OperationResult> UpdateCategoryImageAsync(int categoryGroupId, int categoryId,
        IFormFile image, string baseUrlPath, CancellationToken cancellationToken);
    Task<OperationResult> DeleteCategoryAsync(int categoryGroupId, int categoryId, CancellationToken cancellationToken);
}
