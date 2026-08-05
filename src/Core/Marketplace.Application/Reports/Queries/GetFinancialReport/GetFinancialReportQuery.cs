using Marketplace.Application.Common.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Reports.Queries.GetFinancialReport;

public sealed record FinancialReportDto(
    decimal TotalGrossSales,
    decimal PlatformCommissionRevenue,
    decimal VendorPayoutTotal,
    int TotalOrdersCount,
    int PaidOrdersCount,
    decimal AverageOrderValue,
    int ActiveVendorsCount,
    int TotalCustomersCount);

public sealed record GetFinancialReportQuery : IRequest<Result<FinancialReportDto>>;

public sealed class GetFinancialReportQueryHandler : IRequestHandler<GetFinancialReportQuery, Result<FinancialReportDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetFinancialReportQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<FinancialReportDto>> Handle(GetFinancialReportQuery request, CancellationToken cancellationToken)
    {
        var totalGrossSales = await _dbContext.Payments
            .Where(p => p.Status == PaymentStatus.Success)
            .SumAsync(p => p.Amount, cancellationToken);

        var platformCommission = await _dbContext.Payments
            .Where(p => p.Status == PaymentStatus.Success)
            .SumAsync(p => p.PlatformFee, cancellationToken);

        var vendorPayoutTotal = await _dbContext.Payments
            .Where(p => p.Status == PaymentStatus.Success)
            .SumAsync(p => p.VendorAmount, cancellationToken);

        var totalOrdersCount = await _dbContext.Orders.CountAsync(cancellationToken);
        
        var paidOrdersCount = await _dbContext.Payments
            .CountAsync(p => p.Status == PaymentStatus.Success, cancellationToken);
            
        var avgOrderValue = paidOrdersCount > 0 ? Math.Round(totalGrossSales / paidOrdersCount, 2) : 0m;

        var activeVendorsCount = await _dbContext.Vendors.CountAsync(v => v.IsActive, cancellationToken);
        var totalCustomersCount = await _dbContext.Users.CountAsync(u => u.IsActive, cancellationToken);

        var dto = new FinancialReportDto(
            totalGrossSales,
            platformCommission,
            vendorPayoutTotal,
            totalOrdersCount,
            paidOrdersCount,
            avgOrderValue,
            activeVendorsCount,
            totalCustomersCount);

        return Result.Success(dto);
    }
}
