using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Modules.Accounts.Domain;

internal readonly record struct PlanId(Guid Value)
{
    internal static PlanId New() => new(Guid.NewGuid());
}

internal sealed record Plan(PlanId Id, string Name, bool IsPilotPlan)
{
    internal static Plan Create(string name, bool isPilotPlan)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Plan(PlanId.New(), name.Trim(), isPilotPlan);
    }
}

internal enum ContractStatus
{
    Active,
    Ended
}

internal sealed class AccountContract
{
    internal AccountContract(
        Guid id,
        AccountId accountId,
        PlanId planId,
        DateOnly startsOn,
        DateOnly? endsOn)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Contract identifier cannot be empty.", nameof(id));
        }

        if (endsOn.HasValue && endsOn.Value < startsOn)
        {
            throw new ArgumentException("Contract end date cannot precede its start date.", nameof(endsOn));
        }

        Id = id;
        AccountId = accountId;
        PlanId = planId;
        StartsOn = startsOn;
        EndsOn = endsOn;
        Status = ContractStatus.Active;
    }

    internal Guid Id { get; }

    internal AccountId AccountId { get; }

    internal PlanId PlanId { get; }

    internal DateOnly StartsOn { get; }

    internal DateOnly? EndsOn { get; private set; }

    internal ContractStatus Status { get; private set; }

    internal void End(DateOnly endsOn)
    {
        if (Status == ContractStatus.Ended)
        {
            return;
        }

        if (endsOn < StartsOn)
        {
            throw new DomainRuleException("contract_end_before_start", "A contract cannot end before it starts.");
        }

        EndsOn = endsOn;
        Status = ContractStatus.Ended;
    }
}
