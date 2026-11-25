using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Modules.Accounts.Domain;

internal enum SiteStatus
{
    Draft,
    Published,
    Unpublished
}

internal sealed record SitePublicationReadiness(IReadOnlyCollection<string> BlockingReasons)
{
    internal bool CanPublish => BlockingReasons.Count == 0;
}

internal sealed record SitePublicationChange(
    SiteStatus Status,
    Guid ActorId,
    DateTimeOffset ChangedAt,
    string? Reason);

internal sealed class Site
{
    private readonly List<SitePublicationChange> _publicationHistory = [];

    internal Site(Guid id, AccountId accountId, Subdomain subdomain)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Site identifier cannot be empty.", nameof(id));
        }

        Id = id;
        AccountId = accountId;
        Subdomain = subdomain;
        Status = SiteStatus.Draft;
    }

    internal Guid Id { get; }

    internal AccountId AccountId { get; }

    internal Subdomain Subdomain { get; private set; }

    internal SiteStatus Status { get; private set; }

    internal IReadOnlyCollection<SitePublicationChange> PublicationHistory => _publicationHistory.AsReadOnly();

    internal void ChangeSubdomain(Subdomain subdomain)
    {
        if (Status == SiteStatus.Published)
        {
            throw new DomainRuleException(
                "published_site_subdomain_change",
                "A published site's subdomain cannot be changed without unpublishing it first.");
        }

        Subdomain = subdomain;
    }

    internal void Publish(SitePublicationReadiness readiness, Guid actorId, DateTimeOffset changedAt)
    {
        ArgumentNullException.ThrowIfNull(readiness);
        if (actorId == Guid.Empty)
        {
            throw new ArgumentException("Actor identifier cannot be empty.", nameof(actorId));
        }

        if (!readiness.CanPublish)
        {
            throw new DomainRuleException(
                "site_publication_blocked",
                $"Site publication is blocked: {string.Join(", ", readiness.BlockingReasons)}");
        }

        if (Status == SiteStatus.Published)
        {
            return;
        }

        Status = SiteStatus.Published;
        _publicationHistory.Add(new SitePublicationChange(Status, actorId, changedAt, null));
    }

    internal void Unpublish(Guid actorId, DateTimeOffset changedAt, string reason)
    {
        if (Status != SiteStatus.Published)
        {
            throw new DomainRuleException("site_not_published", "Only a published site can be unpublished.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        Status = SiteStatus.Unpublished;
        _publicationHistory.Add(new SitePublicationChange(Status, actorId, changedAt, reason.Trim()));
    }
}
