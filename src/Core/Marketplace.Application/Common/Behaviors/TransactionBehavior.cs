using Marketplace.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Marketplace.Application.Common.Behaviors;

public sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(
        IApplicationDbContext dbContext,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Only run transactions for Commands, not Queries
        if (!typeof(TRequest).Name.EndsWith("Command"))
        {
            return await next();
        }

        var response = default(TResponse);
        var typeName = typeof(TRequest).Name;

        try
        {
            _logger.LogInformation("Begin transaction for {CommandName}", typeName);

            // Execute in an explicit transaction
            using var transaction = await _dbContext.BeginTransactionAsync(cancellationToken);
            
            try
            {
                response = await next();
                
                await transaction.CommitAsync(cancellationToken);
                
                _logger.LogInformation("Committed transaction for {CommandName}", typeName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling transaction for {CommandName}", typeName);
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction execution failed for {CommandName}", typeName);
            throw;
        }

        return response!;
    }
}
