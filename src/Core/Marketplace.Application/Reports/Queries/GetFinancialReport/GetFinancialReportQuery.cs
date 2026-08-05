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
        var orders = await _dbContext.Orders
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var payments = await _dbContext.Payments
            .AsNoTracking()
            .Where(p => p.Status == PaymentStatus.Success)
            .ToListAsync(cancellationToken);

        var totalGrossSales = payments.Sum(p => p.Amount);
        var platformCommission = payments.Sum(p => p.PlatformFee);
        var vendorPayoutTotal = payments.Sum(p => p.VendorAmount);

        var totalOrdersCount = orders.Count;
        var paidOrdersCount = payments.Count;
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
