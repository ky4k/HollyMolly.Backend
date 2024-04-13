using HM.BLL.Models;

namespace HM.BLL.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<string>> GetAllCategories(CancellationToken cancellationToken);
    Task<OperationResult> CreateCategoryAsync(CategoryCreateDto category, CancellationToken cancellationToken);
    Task<OperationResult> UpdateCategoryAsync(string oldCategoryName, CategoryCreateDto updatedCategory,
        CancellationToken cancellationToken);
    Task<OperationResult> DeleteCategoryAsync(string categoryName, CancellationToken cancellationToken);
}
