using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SportsData.Models
{
    public interface IEntity
    {
        Guid Id { get; set; }
    }
    
    public enum Side
    {
        Home,
        Away,
    }

    public enum PeriodType
    {
        RegularPeriod,
        Overtime,
        Penalties,
        Other
    }

    public enum EventStatus
    {
        NotStarted,
        Live,
        Suspended,
        Ended,
        Closed,
        Cancelled,
        Delayed,
        Interrupted,
        Postponed,
        Abandoned,
        PreMatch,
        Other
    }

    public enum GameStatus
    {
        Suspended,
        NotAvailable,
        Live,
        Other
    }

    public enum BookingStatus
    {
        CanBuy,
        CanBook,
        Booked,
        NotAvailable
    }

    [DataContract]
    public record EventResult
    {
        [DataMember(Order = 1)] public Guid Id { get; set; }

        [DataMember(Order = 2)] public int? Rank { get; set; }

        [DataMember(Order = 3)] public int? Score { get; set; }

        [DataMember(Order = 4)] public decimal? ScoreDecimal { get; set; }

        [DataMember(Order = 5)] public decimal? LeaguePoints { get; set; }

        [DataMember(Order = 6)] public string Time { get; set; }

        [DataMember(Order = 7)] public int? TimeRank { get; set; }

        [DataMember(Order = 8)] public string Status { get; set; }

        [DataMember(Order = 9)] public string StatusDescription { get; set; }

        [DataMember(Order = 10)] public int? Sprint { get; set; }

        [DataMember(Order = 11)] public decimal? SprintPoints { get; set; }

        [DataMember(Order = 12)] public int? SprintRank { get; set; }

        [DataMember(Order = 13)] public int? Climber { get; set; }

        [DataMember(Order = 14)] public decimal? ClimbingPoints { get; set; }

        [DataMember(Order = 15)] public int? ClimbingRank { get; set; }

        [DataMember(Order = 16)] public decimal? HomeTeamScore { get; set; }

        [DataMember(Order = 17)] public decimal? AwayTeamScore { get; set; }

        [DataMember(Order = 18)] public int StatusCode { get; set; }

        [DataMember(Order = 19)] public string StatusCodeDescription { get; set; }
    }

    [DataContract]
    public record EventOutcome
    {
        [DataMember(Order = 1)] public string Winner { get; set; }

        [DataMember(Order = 2)] public EventStatus EventStatus { get; set; }

        [DataMember(Order = 3)] public GameStatus GameStatus { get; set; }

        [DataMember(Order = 4)] public IEnumerable<EventResult> EventResults { get; set; }

        [DataMember(Order = 5)] public IReadOnlyDictionary<string, string> Details { get; set; }
    }

    [DataContract]
    public record ExternalIds
    {
        [DataMember(Order = 1)] public int ProviderId1 { get; set; }

        [DataMember(Order = 2)] public int ProviderId2 { get; set; }

        [DataMember(Order = 3)] public int RotationNumber { get; set; }

        [DataMember(Order = 4)] public IReadOnlyDictionary<string, string> References { get; set; }
    }

    [DataContract]
    public record Player
    {
        [DataMember(Order = 1)] public Guid Id { get; set; }

        [DataMember(Order = 2)] public string Name { get; set; }
    }

    [DataContract]
    public record SportsUniform
    {
        [DataMember(Order = 1)] public string MainColor { get; set; }

        [DataMember(Order = 2)] public string Number { get; set; }

        [DataMember(Order = 3)] public string SecondaryColor { get; set; }

        [DataMember(Order = 4)] public string Type { get; set; }

        [DataMember(Order = 5)] public bool? HasHorizantalStripes { get; set; }

        [DataMember(Order = 6)] public bool? HasSplit { get; set; }

        [DataMember(Order = 7)] public bool? HasSquares { get; set; }

        [DataMember(Order = 8)] public bool? HasVerticalStripes { get; set; }
    }

    [DataContract]
    public record Coach
    {
        [DataMember(Order = 1)] public Guid Id { get; set; }

        [DataMember(Order = 2)] public string CountryCode { get; set; }

        [DataMember(Order = 3)] public string Name { get; set; }

        [DataMember(Order = 4)] public string Nationality { get; set; }
    }

    [DataContract]
    public record Location
    {
        [DataMember(Order = 1)] public Guid Id { get; set; }

        [DataMember(Order = 2)] public string Name { get; set; }

        [DataMember(Order = 3)] public string City { get; set; }

        [DataMember(Order = 4)] public string Country { get; set; }

        [DataMember(Order = 5)] public int? SeatsNumber { get; set; }

        [DataMember(Order = 6)] public string GeoData { get; set; }

        [DataMember(Order = 7)] public string CountryCode { get; set; }
    }

    [DataContract]
    public record SportsTeam
    {
        [DataMember(Order = 1)] public Guid Id { get; set; }

        [DataMember(Order = 2)] public string Name { get; set; }

        [DataMember(Order = 3)] public string Country { get; set; }

        [DataMember(Order = 4)] public string ShortName { get; set; }

        [DataMember(Order = 5)] public bool IsVirtual { get; set; }

        [DataMember(Order = 6)] public ExternalIds ExternalIds { get; set; }

        [DataMember(Order = 7)] public string CountryCode { get; set; }

        [DataMember(Order = 8)] public IEnumerable<Player> Players { get; set; }

        [DataMember(Order = 9)] public IEnumerable<SportsUniform> SportsUniformVariations { get; set; }

        [DataMember(Order = 10)] public Coach Coach { get; set; }

        [DataMember(Order = 11)] public Location Location { get; set; }

        [DataMember(Order = 12)] public string Qualifier { get; set; }
    }

    [DataContract]
    public record Season
    {
        [DataMember(Order = 1)] public Guid Id { get; set; }

        [DataMember(Order = 2)] public string Name { get; set; }
    }

    [DataContract]
    public record GamePhase
    {
        [DataMember(Order = 1)] public string Type { get; set; }

        [DataMember(Order = 2)] public int? Number { get; set; }

        [DataMember(Order = 3)] public string GroupType { get; set; }

        [DataMember(Order = 4)] public string AlternativeEvent { get; set; }

        [DataMember(Order = 5)] public string Name { get; set; }

        [DataMember(Order = 6)] public string GroupName { get; set; }

        [DataMember(Order = 7)] public string LongName { get; set; }

        [DataMember(Order = 8)] public int? CupMatches { get; set; }

        [DataMember(Order = 9)] public int? TotalMatches { get; set; }

        [DataMember(Order = 10)] public int ProviderId { get; set; }
    }

    [DataContract]
    public record SportInfo
    {
        [DataMember(Order = 1)] public Guid Id { get; set; }

        [DataMember(Order = 2)] public string Name { get; set; }
    }

    [DataContract]
    public record LongTermEvent : IEntity
    {
        [DataMember(Order = 1)] public Guid Id { get; set; }

        [DataMember(Order = 2)] public string Name { get; set; }

        [DataMember(Order = 3)] public string SportId { get; set; }

        [DataMember(Order = 4)] public DateTime? ScheduledTime { get; set; }

        [DataMember(Order = 5)] public DateTime? ScheduledEndTime { get; set; }

        [DataMember(Order = 6)] public SportInfo Sport { get; set; }

        [DataMember(Order = 7)] public bool HasLiveCoverage { get; set; }
    }

    [DataContract]
    public record Media
    {
        [DataMember(Order = 1)] public string Channel { get; set; }

        [DataMember(Order = 2)] public DateTime? StartTime { get; set; }
    }

    [DataContract]
    public record BroadcastInfo
    {
        [DataMember(Order = 1)] public string Level { get; set; }

        [DataMember(Order = 2)] public bool SupportsLiveBroadcast { get; set; }

        [DataMember(Order = 3)] public IEnumerable<string> Details { get; set; }
    }

    [DataContract]
    public record Reference
    {
        [DataMember(Order = 1)] public string Link { get; set; }

        [DataMember(Order = 2)] public string Name { get; set; }
    }

    [DataContract]
    public record TvChannel
    {
        [DataMember(Order = 1)] public int Id { get; set; }

        [DataMember(Order = 2)] public string Name { get; set; }
    }

    [DataContract]
    public record TradersTracking
    {
        [DataMember(Order = 1)] public bool SupportsAutoTracking { get; set; }

        [DataMember(Order = 2)] public bool SupportsStatistics { get; set; }

        [DataMember(Order = 3)] public bool SupportsLiveServices { get; set; }

        [DataMember(Order = 4)] public bool SupportsLiveScore { get; set; }

        [DataMember(Order = 5)] public IEnumerable<Reference> References { get; set; }

        [DataMember(Order = 6)] public IEnumerable<TvChannel> TvChannels { get; set; }
    }

    [DataContract]
    public record ScheduleChangeRecord
    {
        [DataMember(Order = 1)] public DateTime PreviousValue { get; set; }

        [DataMember(Order = 2)] public DateTime NewValue { get; set; }

        [DataMember(Order = 3)] public DateTime Timastamp { get; set; }
    }

    [DataContract]
    public record Fixture
    {
        [DataMember(Order = 1)] public DateTime? KickoffTime { get; set; }

        [DataMember(Order = 2)] public bool? ReScheduled { get; set; }

        [DataMember(Order = 3)] public bool? Confirmed { get; set; }

        [DataMember(Order = 4)] public DateTime? LiveTime { get; set; }

        [DataMember(Order = 5)] public Guid AlternativeEvent { get; set; }

        [DataMember(Order = 6)] public IReadOnlyDictionary<string, string> Details { get; set; }

        [DataMember(Order = 7)] public IEnumerable<Media> Media { get; set; }

        [DataMember(Order = 8)] public BroadcastInfo BroadcastInfo { get; set; }

        [DataMember(Order = 9)] public TradersTracking TradersTracking { get; set; }

        [DataMember(Order = 10)] public ExternalIds ExternalIds { get; set; }

        [DataMember(Order = 11)] public IEnumerable<ScheduleChangeRecord> ScheduleChangeRecords { get; set; }

        [DataMember(Order = 12)] public Guid StageId { get; set; }

        [DataMember(Order = 13)] public IEnumerable<Guid> ParentIds { get; set; }
    }

    [DataContract]
    public record Assist
    {
        [DataMember(Order = 1)] public Guid Id { get; set; }

        [DataMember(Order = 2)] public string Name { get; set; }

        [DataMember(Order = 3)] public string Type { get; set; }
    }

    [DataContract]
    public record GoalScorer
    {
        [DataMember(Order = 1)] public Guid Id { get; set; }

        [DataMember(Order = 2)] public string Name { get; set; }
    }

    [DataContract]
    public record TimelineRecord
    {
        [DataMember(Order = 1)] public int Id { get; set; }

        [DataMember(Order = 2)] public decimal? HomeScore { get; set; }

        [DataMember(Order = 3)] public decimal? AwayScore { get; set; }

        [DataMember(Order = 4)] public int? LiveTime { get; set; }

        [DataMember(Order = 5)] public string Period { get; set; }

        [DataMember(Order = 6)] public string PeriodName { get; set; }

        [DataMember(Order = 7)] public string Points { get; set; }

        [DataMember(Order = 8)] public string InjuryTime { get; set; }

        [DataMember(Order = 9)] public Side? Side { get; set; }

        [DataMember(Order = 10)] public string Type { get; set; }

        [DataMember(Order = 11)] public string Value { get; set; }

        [DataMember(Order = 12)] public int CoordinatesA { get; set; }

        [DataMember(Order = 13)] public int CoordinatesB { get; set; }

        [DataMember(Order = 14)] public DateTime Time { get; set; }

        [DataMember(Order = 15)] public IEnumerable<Assist> Assists { get; set; }

        [DataMember(Order = 16)] public GoalScorer GoalScorer { get; set; }

        [DataMember(Order = 17)] public Player Player { get; set; }
    }

    [DataContract]
    public record DelayedInfo
    {
        [DataMember(Order = 1)] public int Id { get; set; }

        [DataMember(Order = 2)] public string Description { get; set; }
    }

    [DataContract]
    public record EventTiming
    {
        [DataMember(Order = 1)] public string EventTime { get; set; }

        [DataMember(Order = 2)] public string Injurytime { get; set; }

        [DataMember(Order = 3)] public string AnnouncedInjuryTime { get; set; }

        [DataMember(Order = 4)] public string RemainingDuration { get; set; }

        [DataMember(Order = 5)] public string TimeLeftInPeriod { get; set; }

        [DataMember(Order = 6)] public bool? IsRunning { get; set; }
    }

    [DataContract]
    public record GameScore
    {
        [DataMember(Order = 1)] public decimal HomeScore { get; set; }

        [DataMember(Order = 2)] public decimal AwayScore { get; set; }

        [DataMember(Order = 3)] public PeriodType? PeriodType { get; set; }

        [DataMember(Order = 4)] public int? PeriodNumber { get; set; }

        [DataMember(Order = 5)] public int? MatchStatusCode { get; set; }

        [DataMember(Order = 6)] public string MatchStatusDetails { get; set; }
    }

    [DataContract]
    public record EventStatusDetails
    {
        [DataMember(Order = 1)] public string WinnerId { get; set; }

        [DataMember(Order = 2)] public EventStatus Status { get; set; }

        [DataMember(Order = 3)] public GameStatus GameStatus { get; set; }

        [DataMember(Order = 4)] public IEnumerable<EventResult> EventResults { get; set; }

        [DataMember(Order = 5)] public IReadOnlyDictionary<string, string> Properties { get; set; }

        [DataMember(Order = 6)] public EventTiming EventTiming { get; set; }

        [DataMember(Order = 7)] public IEnumerable<GameScore> PeriodScores { get; set; }

        [DataMember(Order = 8)] public decimal HomeScore { get; set; }

        [DataMember(Order = 9)] public decimal AwayScore { get; set; }

        [DataMember(Order = 10)] public string Description { get; set; }
    }

    [DataContract]
    public record Official
    {
        [DataMember(Order = 1)] public Guid Id { get; set; }

        [DataMember(Order = 2)] public string Name { get; set; }

        [DataMember(Order = 3)] public string Details { get; set; }
    }

    [DataContract]
    public record Environment
    {
        [DataMember(Order = 1)] public string FieldType { get; set; }

        [DataMember(Order = 2)] public int? Temperature { get; set; }

        [DataMember(Order = 3)] public string Weather { get; set; }

        [DataMember(Order = 4)] public string WindSpeed { get; set; }

        [DataMember(Order = 5)] public string WindImpact { get; set; }
    }

    [DataContract]
    public record EventConditions
    {
        [DataMember(Order = 1)] public string Spectators { get; set; }

        [DataMember(Order = 2)] public string Format { get; set; }

        [DataMember(Order = 3)] public Official Official { get; set; }

        [DataMember(Order = 4)] public Environment Environment { get; set; }
    }

    [DataContract]
    public record Competitor
    {
        [DataMember(Order = 1)] public Guid Id { get; set; }

        [DataMember(Order = 2)] public string Name { get; set; }

        [DataMember(Order = 3)] public string Country { get; set; }

        [DataMember(Order = 4)] public string Abbreviation { get; set; }

        [DataMember(Order = 5)] public bool IsVirtual { get; set; }

        [DataMember(Order = 6)] public ExternalIds ExternalIds { get; set; }

        [DataMember(Order = 7)] public string CountryCode { get; set; }

        [DataMember(Order = 8)] public IEnumerable<Player> Players { get; set; }

        [DataMember(Order = 9)] public IEnumerable<SportsUniform> SportUniforms { get; set; }

        [DataMember(Order = 10)] public Coach Coach { get; set; }

        [DataMember(Order = 11)] public Location Location { get; set; }
    }

    [DataContract]
    public record SportEvent : IEntity
    {
        [DataMember(Order = 1)] public Guid Id { get; set; }

        [DataMember(Order = 2)] public string Name { get; set; }

        [DataMember(Order = 3)] public string SportId { get; set; }

        [DataMember(Order = 4)] public DateTime? StartTime { get; set; }

        [DataMember(Order = 5)] public DateTime? EndTime { get; set; }

        [DataMember(Order = 6)] public EventOutcome EventOutcome { get; set; }

        [DataMember(Order = 7)] public SportsTeam HomeTeam { get; set; }

        [DataMember(Order = 8)] public SportsTeam AwayTeam { get; set; }

        [DataMember(Order = 9)] public Season Season { get; set; }

        [DataMember(Order = 10)] public GamePhase GamePhase { get; set; }

        [DataMember(Order = 11)] public LongTermEvent LongTermEvent { get; set; }

        [DataMember(Order = 12)] public Fixture Fixture { get; set; }

        [DataMember(Order = 13)] public IEnumerable<TimelineRecord> TimelineRecords { get; set; }

        [DataMember(Order = 14)] public DelayedInfo DelayedInfo { get; set; }

        [DataMember(Order = 15)] public EventStatusDetails StatusDetails { get; set; }

        [DataMember(Order = 16)] public BookingStatus? BookingStatus { get; set; }

        [DataMember(Order = 17)] public Location Location { get; set; }

        [DataMember(Order = 18)] public EventConditions Conditions { get; set; }

        [DataMember(Order = 19)] public IEnumerable<Competitor> Competitors { get; set; }
    }
}