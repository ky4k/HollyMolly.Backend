using HM.BLL.Extensions;
using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.Supports;
using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HM.BLL.Services;

public class SupportService(
    HmDbContext context,
    ILogger<SupportService> logger
    ) : ISupportService
{
    public async Task<OperationResult> SaveSupportRequestAsync(SupportCreateDto supportDto, CancellationToken cancellationToken)
    {
        try
        {
            await EnsureCorrectOrderIdAsync(supportDto, cancellationToken);
            Support support = supportDto.ToSupport();

            await context.Supports.AddAsync(support, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return new OperationResult(true, "The request to the support service has been successfully saved.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while saving the support request");
            return new OperationResult(false, "Failed to save support request.");
        }
    }

    private async Task EnsureCorrectOrderIdAsync(SupportCreateDto supportDto, CancellationToken cancellationToken)
    {
        if (supportDto.OrderId != null)
        {
            Order? order = await context.Orders
                .FirstOrDefaultAsync(o => o.Id == supportDto.OrderId, cancellationToken);
            if (order == null)
            {
                supportDto.OrderId = null;
            }
        }
    }
}
