using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public interface ICurrentUser { Guid UserId { get; } string Email { get; } }
