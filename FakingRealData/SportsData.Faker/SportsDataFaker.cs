using System;
using System.Collections.Generic;
using Bogus;
using SportsData.Models;
using Environment = SportsData.Models.Environment;

namespace SportsData.Faker
{
    public static class SportsDataFaker
    {
        public static Faker<EventResult> GetEventResultFaker()
        {
            var eventResultFaker = new Faker<EventResult>()
                .RuleFor(o => o.Id, f => f.Random.Guid())
                .RuleFor(o => o.Rank, f => f.Random.Int(1, 10))
                .RuleFor(o => o.Score, f => f.Random.Int(10, 100))
                .RuleFor(o => o.ScoreDecimal, f => f.Random.Decimal(10m, 100m))
                .RuleFor(o => o.LeaguePoints, f => f.Random.Decimal(10m, 100m))
                .RuleFor(o => o.Time, f => f.Date.Timespan().ToString())
                .RuleFor(o => o.TimeRank, f => f.Random.Int(1, 20))
                .RuleFor(o => o.Status, f => f.PickRandom<EventStatus>().ToString())
                .RuleFor(o => o.StatusDescription, f => f.Lorem.Sentence())
                .RuleFor(o => o.Sprint, f => f.Random.Int(1, 10))
                .RuleFor(o => o.SprintPoints, f => f.Random.Decimal(1m, 10m))
                .RuleFor(o => o.SprintRank, f => f.Random.Int(1, 20))
                .RuleFor(o => o.Climber, f => f.Random.Int(1, 10))
                .RuleFor(o => o.ClimbingPoints, f => f.Random.Decimal(1m, 10m))
                .RuleFor(o => o.ClimbingRank, f => f.Random.Int(1, 20))
                .RuleFor(o => o.HomeTeamScore, f => f.Random.Decimal(1m, 5m))
                .RuleFor(o => o.AwayTeamScore, f => f.Random.Decimal(1m, 5m))
                .RuleFor(o => o.StatusCode, f => f.Random.Int(0, 10))
                .RuleFor(o => o.StatusCodeDescription, f => f.Lorem.Sentence());

            return eventResultFaker;
        }

        public static Faker<EventOutcome> GetEventOutcomeFaker()
        {
            var eventResultFaker = GetEventResultFaker();

            var eventOutcomeFaker = new Faker<EventOutcome>()
                .RuleFor(o => o.Winner, f => f.Random.Guid().ToString())
                .RuleFor(o => o.EventStatus, f => f.PickRandom<EventStatus>())
                .RuleFor(o => o.GameStatus, f => f.PickRandom<GameStatus>())
                .RuleFor(o => o.EventResults, f => eventResultFaker.Generate(5)) 
                .RuleFor(o => o.Details, f => new Dictionary<string, string>
                {
                    { "Property1", f.Random.Word() },
                    { "Property2", f.Random.Number().ToString() }
                });

            return eventOutcomeFaker;
        }

        public static Faker<ExternalIds> GetExternalIdFaker()
        {
            var externalIdFaker = new Faker<ExternalIds>()
                .RuleFor(o => o.ProviderId1, f => f.Random.Int(1000, 9999))
                .RuleFor(o => o.ProviderId2, f => f.Random.Int(1000, 9999))
                .RuleFor(o => o.RotationNumber, f => f.Random.Int(1, 999))
                .RuleFor(o => o.References, f => new Dictionary<string, string>
                {
                    { "RefKey1", f.Random.Word() },
                    { "RefKey2", f.Random.Word() }
                });

            return externalIdFaker;
        }

        public static Faker<Player> GetPlayerFaker()
        {
            var playerFaker = new Faker<Player>()
                .RuleFor(o => o.Id, f => f.Random.Guid())
                .RuleFor(o => o.Name, f => f.Name.FullName());

            return playerFaker;
        }

        public static Faker<SportsUniform> GetSportsUniformFaker()
        {
            var sportsUniformFaker = new Faker<SportsUniform>()
                .RuleFor(o => o.MainColor, f => f.Commerce.Color())
                .RuleFor(o => o.Number, f => f.Random.Number(1, 99).ToString())
                .RuleFor(o => o.SecondaryColor, f => f.Commerce.Color())
                .RuleFor(o => o.Type, f => f.PickRandom(new[] { "HomeTeam", "AwayTeam", "Third" }))
                .RuleFor(o => o.HasHorizantalStripes, f => f.Random.Bool())
                .RuleFor(o => o.HasSplit, f => f.Random.Bool())
                .RuleFor(o => o.HasSquares, f => f.Random.Bool())
                .RuleFor(o => o.HasVerticalStripes, f => f.Random.Bool());

            return sportsUniformFaker;
        }

        public static Faker<Coach> GetCoachFaker()
        {
            var coachFaker = new Faker<Coach>()
                .RuleFor(o => o.Id, f => f.Random.Guid())
                .RuleFor(o => o.CountryCode, f => f.Address.CountryCode())
                .RuleFor(o => o.Name, f => f.Name.FullName())
                .RuleFor(o => o.Nationality, f => f.Address.Country());

            return coachFaker;
        }

        public static Faker<Location> GetLocationFaker(int numberOfUniqueLocationIds = 500)
        {
            var locationIds = GenerateNumberOfGuids(numberOfUniqueLocationIds);

            var locationFaker = new Faker<Location>()
                .RuleFor(o => o.Id, f => f.PickRandom(locationIds))
                .RuleFor(o => o.Name, f => f.Address.StreetName())
                .RuleFor(o => o.City, f => f.Address.City())
                .RuleFor(o => o.Country, f => f.Address.Country())
                .RuleFor(o => o.SeatsNumber, f => f.Random.Int(1000, 100000))
                .RuleFor(o => o.GeoData, f => $"{f.Address.Latitude()},{f.Address.Longitude()}")
                .RuleFor(o => o.CountryCode, f => f.Address.CountryCode());

            return locationFaker;
        }

        public static Faker<SportsTeam> GetSportsTeamFaker()
        {
            var playerFaker = GetPlayerFaker();
            var sportsUniformFaker = GetSportsUniformFaker();
            var coachFaker = GetCoachFaker();
            var locationFaker = GetLocationFaker();
            var externalIdFaker = GetExternalIdFaker();

            var sportsTeamFaker = new Faker<SportsTeam>()
                .RuleFor(o => o.Id, f => f.Random.Guid())
                .RuleFor(o => o.Name, f => f.Company.CompanyName())
                .RuleFor(o => o.Country, f => f.Address.Country())
                .RuleFor(o => o.ShortName, f => f.Lorem.Letter(3))
                .RuleFor(o => o.IsVirtual, f => f.Random.Bool())
                .RuleFor(o => o.ExternalIds, f => externalIdFaker.Generate())
                .RuleFor(o => o.CountryCode, f => f.Address.CountryCode())
                .RuleFor(o => o.Players, f => playerFaker.Generate(5))
                .RuleFor(o => o.SportsUniformVariations, f => sportsUniformFaker.Generate(3))
                .RuleFor(o => o.Coach, f => coachFaker.Generate())
                .RuleFor(o => o.Location, f => locationFaker.Generate())
                .RuleFor(o => o.Qualifier, f => f.Random.String2(2, 3).ToUpper());

            return sportsTeamFaker;
        }

        public static Faker<Season> GetSeasonFaker(int numberOfUniqueSeasonIds = 500)
        {
            var seasonIds = GenerateNumberOfGuids(numberOfUniqueSeasonIds);

            var seasonInfoFaker = new Faker<Season>()
                .RuleFor(o => o.Id, f => f.PickRandom(seasonIds))
                .RuleFor(o => o.Name, f => f.Date.Future().Year.ToString());

            return seasonInfoFaker;
        }

        public static Faker<GamePhase> GetGamePhaseFaker()
        {
            var gamePhaseFaker = new Faker<GamePhase>()
                .RuleFor(o => o.Type, f => f.Random.Word())
                .RuleFor(o => o.Number, f => f.Random.Int(1, 38))
                .RuleFor(o => o.GroupType, f => f.Random.String2(1, "ABCD"))
                .RuleFor(o => o.AlternativeEvent, f => f.Random.Guid().ToString())
                .RuleFor(o => o.Name, f => f.Lorem.Word())
                .RuleFor(o => o.GroupName, f => f.Lorem.Word())
                .RuleFor(o => o.LongName, f => f.Lorem.Sentence())
                .RuleFor(o => o.CupMatches, f => f.Random.Int(1, 10))
                .RuleFor(o => o.TotalMatches, f => f.Random.Int(1, 5))
                .RuleFor(o => o.ProviderId, f => f.Random.Int(1000, 10000));

            return gamePhaseFaker;
        }

        public static Faker<SportInfo> GetSportInfoFaker()
        {
            var sportInfoFaker = new Faker<SportInfo>()
                .RuleFor(o => o.Id, f => f.Random.Guid())
                .RuleFor(o => o.Name, f => f.Commerce.Department());

            return sportInfoFaker;
        }

        public static Faker<LongTermEvent> GetLongTermEventFaker(int numberOfUniqueLongTermEventIds = 500)
        {
            var longTermEventIds = GenerateNumberOfGuids(numberOfUniqueLongTermEventIds);
            var sportInfoFaker = GetSportInfoFaker();

            var longTermEventFaker = new Faker<LongTermEvent>()
                .RuleFor(o => o.Id, f => f.PickRandom(longTermEventIds))
                .RuleFor(o => o.Name, f => f.Commerce.ProductName())
                .RuleFor(o => o.SportId, f => f.Random.Guid().ToString())
                .RuleFor(o => o.ScheduledTime, f => f.Date.Future())
                .RuleFor(o => o.ScheduledEndTime, f => f.Date.Future())
                .RuleFor(o => o.Sport, f => sportInfoFaker.Generate())
                .RuleFor(o => o.HasLiveCoverage, f => f.Random.Bool());

            return longTermEventFaker;
        }

        public static Faker<Media> GetMediaFaker()
        {
            var mediaFaker = new Faker<Media>()
                .RuleFor(o => o.Channel, f => f.Company.CompanyName())
                .RuleFor(o => o.StartTime, f => f.Date.Future());

            return mediaFaker;
        }

        public static Faker<BroadcastInfo> GetBroadcastInfoFaker()
        {
            var broadcastInfoFaker = new Faker<BroadcastInfo>()
                .RuleFor(o => o.Level, f => f.PickRandom(new[] { "Full", "Partial", "Highlight" }))
                .RuleFor(o => o.SupportsLiveBroadcast, f => f.Random.Bool())
                .RuleFor(o => o.Details,
                    f => f.Random.ListItems(
                        new List<string> { "Commentary", "Score Updates", "Player Stats", "Interviews" },
                        f.Random.Int(1, 4)));

            return broadcastInfoFaker;
        }

        public static Faker<Reference> GetReferenceFaker()
        {
            var referenceFaker = new Faker<Reference>()
                .RuleFor(o => o.Link, f => f.Internet.Url())
                .RuleFor(o => o.Name, f => f.Commerce.ProductName());

            return referenceFaker;
        }

        public static Faker<TvChannel> GetTvChannelFaker()
        {
            var tvChannelFaker = new Faker<TvChannel>()
                .RuleFor(o => o.Id, f => f.UniqueIndex)
                .RuleFor(o => o.Name, f => f.Company.CompanyName());

            return tvChannelFaker;
        }

        public static Faker<TradersTracking> GetTraderTrackingFaker()
        {
            var referenceFaker = GetReferenceFaker();
            var tvChannelFaker = GetTvChannelFaker();

            var tradersTrackingFaker = new Faker<TradersTracking>()
                .RuleFor(o => o.SupportsAutoTracking, f => f.Random.Bool())
                .RuleFor(o => o.SupportsStatistics, f => f.Random.Bool())
                .RuleFor(o => o.SupportsLiveServices, f => f.Random.Bool())
                .RuleFor(o => o.SupportsLiveScore, f => f.Random.Bool())
                .RuleFor(o => o.References, f => referenceFaker.Generate(f.Random.Int(1, 5)))
                .RuleFor(o => o.TvChannels, f => tvChannelFaker.Generate(f.Random.Int(1, 5)));

            return tradersTrackingFaker;
        }

        public static Faker<ScheduleChangeRecord> GetScheduleChangeFaker()
        {
            var scheduleChangeFaker = new Faker<ScheduleChangeRecord>()
                .RuleFor(o => o.PreviousValue, f => f.Date.Past())
                .RuleFor(o => o.NewValue, f => f.Date.Soon())
                .RuleFor(o => o.Timastamp, f => f.Date.Recent());

            return scheduleChangeFaker;
        }

        public static Faker<Fixture> GetFixtureFaker()
        {
            var mediaFaker = GetMediaFaker();
            var broadcastInfoFaker = GetBroadcastInfoFaker();
            var traderTrackingFaker = GetTraderTrackingFaker();
            var externalIdFaker = GetExternalIdFaker();
            var scheduleChangeFaker = GetScheduleChangeFaker();

            var fixtureFaker = new Faker<Fixture>()
                .RuleFor(o => o.KickoffTime, f => f.Date.Soon().OrNull(f))
                .RuleFor(o => o.ReScheduled, f => f.Random.Bool().OrNull(f))
                .RuleFor(o => o.Confirmed, f => f.Random.Bool().OrNull(f))
                .RuleFor(o => o.LiveTime, f => f.Date.Future().OrNull(f))
                .RuleFor(o => o.AlternativeEvent, f => f.Random.Guid())
                .RuleFor(o => o.Details, f => new Dictionary<string, string>
                {
                    { "info1", f.Random.Word() },
                    { "info2", f.Random.Word() }
                })
                .RuleFor(o => o.Media, f => mediaFaker.Generate(f.Random.Int(1, 5)))
                .RuleFor(o => o.BroadcastInfo, f => broadcastInfoFaker.Generate())
                .RuleFor(o => o.TradersTracking, f => traderTrackingFaker.Generate())
                .RuleFor(o => o.ExternalIds, f => externalIdFaker.Generate())
                .RuleFor(o => o.ScheduleChangeRecords, f => scheduleChangeFaker.Generate(f.Random.Int(1, 3)))
                .RuleFor(o => o.StageId, f => f.Random.Guid())
                .RuleFor(o => o.ParentIds, f => new List<Guid> { f.Random.Guid(), f.Random.Guid() });

            return fixtureFaker;
        }

        public static Faker<Assist> GetAssistFaker()
        {
            var assistFaker = new Faker<Assist>()
                .RuleFor(o => o.Id, f => f.Random.Guid())
                .RuleFor(o => o.Name, f => f.Name.FirstName())
                .RuleFor(o => o.Type, f => f.Random.Word());

            return assistFaker;
        }

        public static Faker<GoalScorer> GetGoalScorerFaker()
        {
            var goalScorerFaker = new Faker<GoalScorer>()
                .RuleFor(o => o.Id, f => f.Random.Guid())
                .RuleFor(o => o.Name, f => f.Name.FirstName());

            return goalScorerFaker;
        }

        public static Faker<TimelineRecord> GetTimelineRecordFaker()
        {
            var assistFaker = GetAssistFaker();
            var goalScorerFaker = GetGoalScorerFaker();
            var playerFaker = GetPlayerFaker();

            var timelineRecordFaker = new Faker<TimelineRecord>()
                .RuleFor(o => o.Id, f => f.IndexFaker)
                .RuleFor(o => o.HomeScore, f => f.Random.Decimal(0, 5).OrNull(f))
                .RuleFor(o => o.AwayScore, f => f.Random.Decimal(0, 5).OrNull(f))
                .RuleFor(o => o.LiveTime, f => f.Random.Int(0, 90).OrNull(f))
                .RuleFor(o => o.Period, f => f.PickRandom(new[] { "FirstHalf", "SecondHalf", "ExtraTime" }))
                .RuleFor(o => o.PeriodName, f => f.Random.Word())
                .RuleFor(o => o.Points, f => f.Random.Int(0, 3).ToString())
                .RuleFor(o => o.InjuryTime, f => f.Random.Int(0, 5).ToString())
                .RuleFor(o => o.Side, f => f.PickRandom<Side>().OrNull(f))
                .RuleFor(o => o.Type, f => f.Random.Word())
                .RuleFor(o => o.Value, f => f.Random.Word())
                .RuleFor(o => o.CoordinatesA, f => f.Random.Int(0, 100))
                .RuleFor(o => o.CoordinatesB, f => f.Random.Int(0, 100))
                .RuleFor(o => o.Time, f => f.Date.Recent())
                .RuleFor(o => o.Assists, f => assistFaker.Generate(f.Random.Int(1, 3)))
                .RuleFor(o => o.GoalScorer, f => goalScorerFaker.Generate())
                .RuleFor(o => o.Player, f => playerFaker.Generate());

            return timelineRecordFaker;
        }
        
        public static Faker<DelayedInfo> GetDelayedInfoFaker()
        {
            var delayedInfoFaker = new Faker<DelayedInfo>()
                .RuleFor(o => o.Id, f => f.IndexFaker)
                .RuleFor(o => o.Description, f => f.Lorem.Sentence());

            return delayedInfoFaker;
        }

        public static Faker<EventTiming> GetEventTimingFaker()
        {
            var eventTimingFaker = new Faker<EventTiming>()
                .RuleFor(o => o.EventTime, f => f.Date.Recent().ToString("HH:mm:ss"))
                .RuleFor(o => o.Injurytime, f => f.Date.Recent().ToString("HH:mm:ss"))
                .RuleFor(o => o.AnnouncedInjuryTime, f => f.Date.Recent().ToString("HH:mm:ss"))
                .RuleFor(o => o.RemainingDuration, f => f.Date.Recent().ToString("yyyy-MM-dd"))
                .RuleFor(o => o.TimeLeftInPeriod, f => f.Date.Recent().ToString("HH:mm:ss"))
                .RuleFor(o => o.IsRunning, f => f.Random.Bool().OrNull(f));

            return eventTimingFaker;
        }

        public static Faker<GameScore> GetGameScoreFaker()
        {
            var gameScoreFaker = new Faker<GameScore>()
                .RuleFor(o => o.HomeScore, f => f.Random.Decimal(0, 5))
                .RuleFor(o => o.AwayScore, f => f.Random.Decimal(0, 5))
                .RuleFor(o => o.PeriodType, f => f.PickRandom<PeriodType>().OrNull(f))
                .RuleFor(o => o.PeriodNumber, f => f.Random.Int(1, 4).OrNull(f))
                .RuleFor(o => o.MatchStatusCode, f => f.Random.Int(0, 10).OrNull(f))
                .RuleFor(o => o.MatchStatusDetails, f => f.Lorem.Sentence());

            return gameScoreFaker;
        }

        public static Faker<EventStatusDetails> GetEventStatusDetailsFaker()
        {
            var eventResultFaker = GetEventResultFaker();
            var eventTimingFaker = GetEventTimingFaker();
            var gameScoreFaker = GetGameScoreFaker();

            var eventStatusDetailsFaker = new Faker<EventStatusDetails>()
                .RuleFor(o => o.WinnerId, f => f.Random.Uuid().ToString())
                .RuleFor(o => o.Status, f => f.PickRandom<EventStatus>())
                .RuleFor(o => o.GameStatus, f => f.PickRandom<GameStatus>())
                .RuleFor(o => o.EventResults, f => eventResultFaker.Generate(f.Random.Int(1, 5)))
                .RuleFor(o => o.Properties, f => new Dictionary<string, string>
                {
                    { "Key1", f.Random.Word() },
                    { "Key2", f.Random.Number().ToString() }
                })
                .RuleFor(o => o.EventTiming, f => eventTimingFaker.Generate())
                .RuleFor(o => o.PeriodScores, f => gameScoreFaker.Generate(f.Random.Int(1, 4)))
                .RuleFor(o => o.HomeScore, f => f.Random.Decimal(0, 5))
                .RuleFor(o => o.AwayScore, f => f.Random.Decimal(0, 5))
                .RuleFor(o => o.Description, f => f.Lorem.Sentence());

            return eventStatusDetailsFaker;
        }

        public static Faker<Official> GetOfficialFaker()
        {
            var refereeFaker = new Faker<Official>()
                .RuleFor(r => r.Id, f => f.Random.Guid())
                .RuleFor(r => r.Name, f => f.Name.FullName())
                .RuleFor(r => r.Details, f => f.Address.Country());

            return refereeFaker;
        }

        public static Faker<Environment> GetEnvironmentFaker()
        {
            var environmentFaker = new Faker<Environment>()
                .RuleFor(w => w.FieldType, f => f.Lorem.Word())
                .RuleFor(w => w.Temperature, f => f.Random.Int(-10, 40).OrNull(f))
                .RuleFor(w => w.Weather,
                    f => f.PickRandom("Sunny", "Cloudy", "Rainy", "Snowy", "Windy"))
                .RuleFor(w => w.WindSpeed, f => f.PickRandom(new[] { "Calm", "Breezy", "Windy", "Gusty" }))
                .RuleFor(w => w.WindImpact, f => f.Random.Bool() ? f.Lorem.Word() : null);

            return environmentFaker;
        }

        public static Faker<EventConditions> GetEventConditionsFaker()
        {
            var officialFaker = GetOfficialFaker();
            var environmentFaker = GetEnvironmentFaker();

            var eventConditionsFaker = new Faker<EventConditions>()
                .RuleFor(sec => sec.Spectators, f => f.Random.Number(1, 100000).ToString())
                .RuleFor(sec => sec.Format, f => f.PickRandom(new[] { "Normal", "ExtraTime", "PenaltyShootout" }))
                .RuleFor(sec => sec.Official, f => officialFaker.Generate())
                .RuleFor(sec => sec.Environment, f => environmentFaker.Generate());

            return eventConditionsFaker;
        }

        public static Faker<Competitor> GetCompetitorFaker()
        {
            var externalIdFaker = GetExternalIdFaker();
            var playerFaker = GetPlayerFaker();
            var sportsUniformFaker = GetSportsUniformFaker();
            var coachFaker = GetCoachFaker();
            var locationFaker = GetLocationFaker();

            var competitorFaker = new Faker<Competitor>()
                .RuleFor(c => c.Id, f => f.Random.Guid())
                .RuleFor(c => c.Name, f => f.Company.CompanyName())
                .RuleFor(c => c.Country, f => f.Address.Country())
                .RuleFor(c => c.Abbreviation, f => f.Random.AlphaNumeric(3).ToUpper())
                .RuleFor(c => c.IsVirtual, f => f.Random.Bool())
                .RuleFor(c => c.ExternalIds, f => externalIdFaker.Generate())
                .RuleFor(c => c.CountryCode, f => f.Address.CountryCode())
                .RuleFor(c => c.Players, f => playerFaker.Generate(f.Random.Int(1, 5)))
                .RuleFor(c => c.SportUniforms, f => sportsUniformFaker.Generate(f.Random.Int(1, 3)))
                .RuleFor(c => c.Coach, f => coachFaker.Generate())
                .RuleFor(c => c.Location, f => locationFaker.Generate());

            return competitorFaker;
        }

        private static Guid[] GenerateNumberOfGuids(int number)
        {
            var guids = new Guid[number];
            for (int i = 0; i < guids.Length; i++)
                guids[i] = Guid.NewGuid();

            return guids;
        }

        public static Faker<SportEvent> GetSportEventFaker(int numberOfUniqueSportIds = 30)
        {
            var sportIds = GenerateNumberOfGuids(numberOfUniqueSportIds);

            var timelineEventFaker = GetTimelineRecordFaker();
            var eventOutcomeFaker = GetEventOutcomeFaker();
            var sportsTeamFaker = GetSportsTeamFaker();
            var seasonInfoFaker = GetSeasonFaker();
            var gamePhaseFaker = GetGamePhaseFaker();
            var longTermEventFaker = GetLongTermEventFaker();
            var fixtureFaker = GetFixtureFaker();
            var delayedInfoFaker = GetDelayedInfoFaker();
            var eventStatusDetailsFaker = GetEventStatusDetailsFaker();
            var locationFaker = GetLocationFaker();
            var eventConditionsFaker = GetEventConditionsFaker();
            var competitorFaker = GetCompetitorFaker();

            var sportEventFaker = new Faker<SportEvent>()
                .RuleFor(m => m.Id, f => f.Random.Guid())
                .RuleFor(m => m.Name, f => f.Lorem.Sentence())
                .RuleFor(m => m.SportId, f => f.PickRandom(sportIds).ToString())
                .RuleFor(m => m.StartTime, f => f.Date.Future())
                .RuleFor(m => m.EndTime, f => f.Date.Future())
                .RuleFor(m => m.EventOutcome, f => eventOutcomeFaker.Generate())
                .RuleFor(m => m.HomeTeam, f => sportsTeamFaker.Generate())
                .RuleFor(m => m.AwayTeam, f => sportsTeamFaker.Generate())
                .RuleFor(m => m.Season, f => seasonInfoFaker.Generate())
                .RuleFor(m => m.GamePhase, f => gamePhaseFaker.Generate())
                .RuleFor(m => m.LongTermEvent, f => longTermEventFaker.Generate())
                .RuleFor(m => m.Fixture, f => fixtureFaker.Generate())
                .RuleFor(m => m.TimelineRecords, f => timelineEventFaker.Generate(f.Random.Int(1, 10)))
                .RuleFor(m => m.DelayedInfo, f => delayedInfoFaker.Generate())
                .RuleFor(m => m.StatusDetails, f => eventStatusDetailsFaker.Generate())
                .RuleFor(m => m.BookingStatus, f => f.PickRandom<BookingStatus>())
                .RuleFor(m => m.Location, f => locationFaker.Generate())
                .RuleFor(m => m.Conditions, f => eventConditionsFaker.Generate())
                .RuleFor(m => m.Competitors, f => competitorFaker.Generate(f.Random.Int(2, 4)));

            return sportEventFaker;
        }
    }
}