using System.Text;
using System.Text.Json;
using BridgeSystems.Bridgemate.DataConnectorClasses.SharedDTO;

namespace DtoGenerator
{
    /// <summary>
    /// Writes golden validation fixtures: an input payload plus the exact result the .NET
    /// Validate() implementation produces for it, captured by running the validator live at
    /// generation time. The port test suites construct the DTO from Payload, run their own
    /// validator with Args, and assert ExpectedValid plus the ExpectedMessages list verbatim
    /// (including order), which makes the C# validators the single source of truth for
    /// validation behaviour across all client languages.
    ///
    /// The two Directory.Exists rules (InitDTO/ContinueDTO AlternativeDataFolder) are
    /// environment-dependent and deliberately have no fixtures; the ports implement them
    /// natively and test them locally.
    /// </summary>
    internal sealed class ValidationFixtureWriter
    {
        private const string Guid1 = "A1B2C3D4E5F60718293A4B5C6D7E8F90";
        private const string Guid2 = "B1B2C3D4E5F60718293A4B5C6D7E8F90";
        private const string EventGuid1 = "C1B2C3D4E5F60718293A4B5C6D7E8F90";

        private string _outDir = "";

        public void Write(string outDir)
        {
            _outDir = outDir;
            Directory.CreateDirectory(outDir);
            foreach (var stale in Directory.GetFiles(outDir, "*.json"))
                File.Delete(stale);

            EmitParticipationCases();
            EmitPlayerDataCases();
            EmitRoundCases();
            EmitTableCases();
            EmitSectionCases();
            EmitSectionUpdateCases();
            EmitScoringGroupCases();
            EmitSessionCases();
            EmitInitCases();
            EmitHandrecordCases();
            EmitResultCases();
            EmitTdCallCases();
            EmitBridgemateSettingsCases();
            EmitContinueCases();
        }

        private void Case<T>(string caseName, T dto, Func<T, bool> validate, Dictionary<string, object>? args = null) where T : class
        {
            //Serialize the payload before validating: Validate() assigns ValidationMessages on the
            //instance and the payload must show the DTO as a client would send it.
            var payload = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(dto, dto.GetType()));
            var valid = validate(dto);
            var messages = (string[]?)dto.GetType().GetProperty("ValidationMessages")?.GetValue(dto) ?? Array.Empty<string>();
            if (valid != !messages.Any())
                Console.WriteLine($"Warning: validation fixture {typeof(T).Name}.{caseName} has inconsistent result/messages.");
            var fixture = new Dictionary<string, object?>
            {
                ["Dto"] = typeof(T).Name,
                ["Case"] = caseName,
                ["Args"] = args ?? new Dictionary<string, object>(),
                ["Payload"] = payload,
                ["ExpectedValid"] = valid,
                ["ExpectedMessages"] = messages
            };
            var json = JsonSerializer.Serialize(fixture, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path.Combine(_outDir, $"{typeof(T).Name}.{caseName}.json"), json, new UTF8Encoding(false));
        }

        #region Factories (minimal VALID instances; cases break exactly what they test)

        private static ParticipationDTO Participation(int table = 1, TableDirection direction = TableDirection.North,
                                                      int round = 1, string? playerNumber = "1001", string letters = "A",
                                                      string guid = Guid1, string? lastName = null,
                                                      bool isRemoval = false, bool isPlayerSwap = false) =>
            new()
            {
                SessionGuid = guid,
                SectionLetters = letters,
                TableNumber = table,
                Direction = direction,
                RoundNumber = round,
                PlayerNumber = playerNumber,
                LastName = lastName,
                IsRemoval = isRemoval,
                IsPlayerSwap = isPlayerSwap
            };

        private static PlayerDataDTO Player(string number, string guid = Guid1) =>
            new() { SessionGuid = guid, PlayerNumber = number, FirstName = "First" + number, LastName = "Last" + number };

        private static RoundDTO Round(int table, int round, string letters = "A", string guid = Guid1) =>
            new()
            {
                SessionGuid = guid,
                SectionLetters = letters,
                TableNumber = table,
                RoundNumber = round,
                PairNS = table * 2 - 1,
                PairEW = table * 2,
                LowBoardNumber = (round - 1) * 4 + 1,
                HighBoardNumber = round * 4
            };

        private static TableDTO Table(int table, int rounds = 2, string letters = "A", string guid = Guid1) =>
            new()
            {
                SessionGuid = guid,
                SectionLetters = letters,
                TableNumber = table,
                Rounds = Enumerable.Range(1, rounds).Select(r => Round(table, r, letters, guid)).ToArray()
            };

        private static SectionDTO Section(string letters = "A", int tables = 2, string guid = Guid1, int scoringGroupNumber = 1) =>
            new()
            {
                SessionGuid = guid,
                ScoringGroupNumber = scoringGroupNumber,
                Letters = letters,
                Name = "Section " + letters,
                Winners = 1,
                GameType = SectionDTO.GameType_Pairs,
                MissingPair = 0,
                EWMoveBeforePlay = 0,
                Tables = Enumerable.Range(1, tables).Select(t => Table(t, 2, letters, guid)).ToArray()
            };

        private static SectionUpdateDTO SectionUpdate(string letters = "A", int tables = 2, string guid = Guid1) =>
            new()
            {
                SessionGuid = guid,
                ScoringGroupNumber = 1,
                ScoringGroupScoringMethod = ScoringGroupDTO.ScoringType_Pairs,
                Letters = letters,
                Name = "Section " + letters,
                Winners = 1,
                GameType = SectionDTO.GameType_Pairs,
                MissingPair = 0,
                EWMoveBeforePlay = 0,
                Tables = Enumerable.Range(1, tables).Select(t => Table(t, 2, letters, guid)).ToArray()
            };

        private static ScoringGroupDTO ScoringGroup(int number = 1, string letters = "A", string guid = Guid1) =>
            new()
            {
                SessionGuid = guid,
                ScoringGroupNumber = number,
                ScoringMethod = ScoringGroupDTO.ScoringType_Pairs,
                Name = "Group " + number,
                Sections = new[] { Section(letters, 2, guid, number) }
            };

        private static SessionDTO Session(string guid = Guid1, int scoringGroupNumber = 1, string letters = "A") =>
            new()
            {
                SessionGuid = guid,
                Name = "Session " + letters,
                Year = 2026,
                Month = 8,
                Day = 4,
                Hour = 19,
                Minute = 30,
                ScoringGroups = new[] { ScoringGroup(scoringGroupNumber, letters, guid) }
            };

        private static InitDTO Init()
        {
            var players = Enumerable.Range(1001, 8).Select(n => Player(n.ToString())).ToArray();
            var participations = new List<ParticipationDTO>();
            var number = 1001;
            for (var table = 1; table <= 2; table++)
                foreach (var direction in new[] { TableDirection.North, TableDirection.East, TableDirection.South, TableDirection.West })
                    participations.Add(Participation(table, direction, round: 1, playerNumber: (number++).ToString()));
            return new InitDTO
            {
                Commands = InitDTO.StartBCS,
                Sessions = new[] { Session() },
                PlayerData = players,
                Participations = participations.ToArray()
            };
        }

        private static HandrecordDTO Handrecord()
        {
            const string fullSuit = "AKQJT98765432";
            return new HandrecordDTO
            {
                SessionGuid = Guid1,
                ScoringGroupNumber = 1,
                SectionLetters = "A",
                BoardNumber = 1,
                NorthSpades = fullSuit, NorthHearts = "", NorthDiamonds = "", NorthClubs = "",
                EastSpades = "", EastHearts = fullSuit, EastDiamonds = "", EastClubs = "",
                SouthSpades = "", SouthHearts = "", SouthDiamonds = fullSuit, SouthClubs = "",
                WestSpades = "", WestHearts = "", WestDiamonds = "", WestClubs = fullSuit
            };
        }

        private static ResultDTO Result() =>
            new()
            {
                SessionGuid = Guid1,
                SectionLetters = "A",
                TableNumber = 1,
                RoundNumber = 1,
                BoardNumber = 1,
                PairNorthSouth = 1,
                PairEastWest = 2,
                DeclaringPair = 1,
                DeclarerDirection = ResultDTO.Direction_North,
                ScoringDirection = ResultDTO.ScoringDirection_NS,
                Level = 3,
                Denomination = 1,
                Stake = 0,
                TotalTricks = 9,
                LeadCardRank = 0,
                LeadCardSuit = 0
            };

        private static TdCallDTO TdCall() =>
            new() { SessionGuid = Guid1, SectionLetters = "A", TableNumber = 1, RoundNumber = 1, Status = 1 };

        private static Bridgemate2SettingsDTO Bm2Settings() =>
            new() { SessionGuid = Guid1, SectionLetters = "A", BM2PINcode = "7431" };

        private static Bridgemate3SettingsDTO Bm3Settings() =>
            new()
            {
                SessionGuid = Guid1,
                SectionLetters = "A",
                BM3PINcode = "7431",
                BM3ScreenBrightness = 1,
                BM3ScreenDimMode = 0,
                BM3SleepMode = 0,
                BM3AudioVolume = 0
            };

        private static ContinueDTO Continue() =>
            new() { EventGuid = EventGuid1, Commands = InitDTO.StartBCS };

        #endregion

        private static readonly Dictionary<string, object> DisallowNumberAndName =
            new() { ["allowPlayerNumberAndName"] = false };
        private static readonly Dictionary<string, object> AllowNumberAndName =
            new() { ["allowPlayerNumberAndName"] = true };

        private void EmitParticipationCases()
        {
            bool Validate(ParticipationDTO dto) => dto.Validate(allowPlayerNumberAndName: false);

            Case("valid", Participation(), Validate, DisallowNumberAndName);
            Case("valid-round-zero", Participation(round: 0), Validate, DisallowNumberAndName);
            //The BRID-2037 relaxation: rounds above one are legal on the DTO itself.
            Case("valid-round-above-one", Participation(round: 7), Validate, DisallowNumberAndName);
            Case("valid-name-only", Participation(playerNumber: null, lastName: "Smith"), Validate, DisallowNumberAndName);
            Case("valid-number-and-name-allowed",
                 Participation(lastName: "Smith"),
                 dto => dto.Validate(allowPlayerNumberAndName: true),
                 AllowNumberAndName);
            Case("invalid-sessionguid", Participation(guid: "ABC"), Validate, DisallowNumberAndName);
            Case("invalid-sectionletters", Participation(letters: "A1"), Validate, DisallowNumberAndName);
            Case("invalid-direction-none", Participation(direction: TableDirection.None), Validate, DisallowNumberAndName);
            Case("invalid-tablenumber", Participation(table: 0), Validate, DisallowNumberAndName);
            Case("negative-round", Participation(round: -1), Validate, DisallowNumberAndName);
            Case("missing-name-and-number", Participation(playerNumber: null), Validate, DisallowNumberAndName);
            Case("number-and-name-disallowed", Participation(lastName: "Smith"), Validate, DisallowNumberAndName);
            var multi = Participation(table: 0, round: -1, guid: "ABC");
            Case("multiple-errors-ordered", multi, Validate, DisallowNumberAndName);
            //A removal: the seat is emptied, so the DTO carries no player at all.
            Case("valid-removal", Participation(playerNumber: null, isRemoval: true), Validate, DisallowNumberAndName);
            Case("removal-with-player", Participation(lastName: "Smith", isRemoval: true), Validate, DisallowNumberAndName);
            Case("removal-and-swap", Participation(playerNumber: null, isRemoval: true, isPlayerSwap: true), Validate, DisallowNumberAndName);
            Case("removal-invalid-seat", Participation(table: 0, playerNumber: null, isRemoval: true), Validate, DisallowNumberAndName);
        }

        private void EmitPlayerDataCases()
        {
            Case("valid", Player("1001"), dto => dto.Validate());
            Case("invalid-sessionguid", Player("1001", guid: "not-a-guid"), dto => dto.Validate());
            var noNumber = Player("1001");
            noNumber.PlayerNumber = "";
            Case("missing-playernumber", noNumber, dto => dto.Validate());
            var noName = Player("1001");
            noName.LastName = null;
            Case("missing-lastname", noName, dto => dto.Validate());
            var multi = Player("1001", guid: "X");
            multi.PlayerNumber = null;
            multi.LastName = "";
            Case("multiple-errors-ordered", multi, dto => dto.Validate());
        }

        private void EmitRoundCases()
        {
            Case("valid", Round(1, 1), dto => dto.Validate());
            Case("invalid-sessionguid", Round(1, 1, guid: "abc"), dto => dto.Validate());
            Case("invalid-sectionletters", Round(1, 1, letters: "ABC"), dto => dto.Validate());
            Case("invalid-tablenumber", Round(0, 1), dto => dto.Validate());
            Case("invalid-roundnumber", Round(1, 0), dto => dto.Validate());
            var sitOutWithBoards = Round(1, 1);
            sitOutWithBoards.PairNS = 0;
            Case("sitout-with-boards", sitOutWithBoards, dto => dto.Validate());
            var sitOut = Round(1, 1);
            sitOut.PairNS = 0;
            sitOut.LowBoardNumber = 0;
            sitOut.HighBoardNumber = 0;
            Case("valid-sitout", sitOut, dto => dto.Validate());
        }

        private void EmitTableCases()
        {
            Case("valid", Table(1), dto => dto.Validate());
            Case("invalid-sessionguid", Table(1, guid: "abc"), dto => dto.Validate());
            Case("invalid-sectionletters", Table(1, letters: "a"), dto => dto.Validate());
            Case("invalid-tablenumber", Table(0, rounds: 1), dto => dto.Validate());
            var duplicateRounds = Table(1);
            duplicateRounds.Rounds = new[] { Round(1, 1), Round(1, 1) };
            Case("duplicate-roundnumbers", duplicateRounds, dto => dto.Validate());
            var gap = Table(1);
            gap.Rounds = new[] { Round(1, 1), Round(1, 3) };
            Case("non-consecutive-rounds", gap, dto => dto.Validate());
            var startsAtTwo = Table(1);
            startsAtTwo.Rounds = new[] { Round(1, 2), Round(1, 3) };
            Case("rounds-not-starting-at-one", startsAtTwo, dto => dto.Validate());
            var guidMismatch = Table(1);
            guidMismatch.Rounds[0].SessionGuid = Guid2;
            Case("round-sessionguid-mismatch", guidMismatch, dto => dto.Validate());
            var letterMismatch = Table(1);
            letterMismatch.Rounds[0].SectionLetters = "B";
            Case("round-sectionletters-mismatch", letterMismatch, dto => dto.Validate());
            var tableMismatch = Table(1);
            tableMismatch.Rounds[0].TableNumber = 9;
            Case("round-tablenumber-mismatch", tableMismatch, dto => dto.Validate());
            var cascade = Table(1);
            cascade.Rounds[1].RoundNumber = 2;
            cascade.Rounds[1].PairNS = 0;
            Case("round-cascade", cascade, dto => dto.Validate());
        }

        private void EmitSectionCases()
        {
            Case("valid", Section(), dto => dto.Validate());
            Case("invalid-sessionguid", Section(guid: "abc"), dto => dto.Validate());
            Case("invalid-letters", Section(letters: "AB"), dto => dto.Validate());
            var badGroup = Section();
            badGroup.ScoringGroupNumber = 0;
            //The tables keep scoring group 1; only the section-level rule must fire.
            Case("invalid-scoringgroupnumber", badGroup, dto => dto.Validate());
            var missingPair = Section();
            missingPair.MissingPair = -1;
            Case("negative-missingpair-one-winner", missingPair, dto => dto.Validate());
            var winners = Section();
            winners.Winners = 3;
            Case("invalid-winners", winners, dto => dto.Validate());
            var gameType = Section();
            gameType.GameType = 15;
            Case("invalid-gametype", gameType, dto => dto.Validate());
            var combi = Section(letters: "C");
            combi.IsCombiSection = true;
            combi.NorthSouthPairSectionLetters = "A1";
            combi.EastWestPairSectionLetters = "B";
            Case("combi-invalid-ns-source-letters", combi, dto => dto.Validate());
            var ewMove = Section();
            ewMove.EWMoveBeforePlay = 3;
            Case("ewmove-exceeds-tables", ewMove, dto => dto.Validate());
            var tableGuid = Section();
            tableGuid.Tables[0].SessionGuid = Guid2;
            foreach (var round in tableGuid.Tables[0].Rounds)
                round.SessionGuid = Guid2;
            Case("table-sessionguid-mismatch", tableGuid, dto => dto.Validate());
            var tableLetters = Section();
            tableLetters.Tables[0].SectionLetters = "B";
            foreach (var round in tableLetters.Tables[0].Rounds)
                round.SectionLetters = "B";
            Case("table-sectionletters-mismatch", tableLetters, dto => dto.Validate());
            var cascade = Section();
            cascade.Tables[0].Rounds[0].PairNS = 0;
            Case("table-cascade", cascade, dto => dto.Validate());
            var duplicates = Section();
            duplicates.Tables[1].TableNumber = 1;
            foreach (var round in duplicates.Tables[1].Rounds)
                round.TableNumber = 1;
            Case("duplicate-tablenumbers", duplicates, dto => dto.Validate());
            var nullTables = Section();
            nullTables.Tables = null;
            Case("valid-null-tables", nullTables, dto => dto.Validate());
        }

        private void EmitSectionUpdateCases()
        {
            Case("valid", SectionUpdate(), dto => dto.Validate());
            Case("invalid-sessionguid", SectionUpdate(guid: "abc"), dto => dto.Validate());
            Case("invalid-letters", SectionUpdate(letters: "zz"), dto => dto.Validate());
            var deletedWithTables = SectionUpdate();
            deletedWithTables.IsDeleted = true;
            Case("deleted-with-tables", deletedWithTables, dto => dto.Validate());
            var deleted = SectionUpdate();
            deleted.IsDeleted = true;
            deleted.Tables = null;
            Case("valid-deleted", deleted, dto => dto.Validate());
            var scoringMethod = SectionUpdate();
            scoringMethod.ScoringGroupScoringMethod = 11;
            Case("invalid-scoringmethod", scoringMethod, dto => dto.Validate());
            var winners = SectionUpdate();
            winners.Winners = 0;
            Case("invalid-winners", winners, dto => dto.Validate());
            //The new BRID-2037 rules around HasExplicitParticipations.
            var flagNoParticipations = SectionUpdate();
            flagNoParticipations.HasExplicitParticipations = true;
            Case("flag-without-participations", flagNoParticipations, dto => dto.Validate());
            var flagged = SectionUpdate();
            flagged.HasExplicitParticipations = true;
            flagged.Participations = AllRoundParticipations(flagged.Letters, tables: 2, rounds: 2);
            Case("valid-flag-with-participations", flagged, dto => dto.Validate());
            var participationCascade = SectionUpdate();
            participationCascade.HasExplicitParticipations = true;
            participationCascade.Participations = AllRoundParticipations("A", 2, 2);
            participationCascade.Participations[0].TableNumber = 0;
            Case("participation-cascade", participationCascade, dto => dto.Validate());
            var wrongGuid = SectionUpdate();
            wrongGuid.HasExplicitParticipations = true;
            wrongGuid.Participations = AllRoundParticipations("A", 2, 2);
            wrongGuid.Participations[0].SessionGuid = Guid2;
            Case("participation-sessionguid-mismatch", wrongGuid, dto => dto.Validate());
            var wrongLetters = SectionUpdate();
            wrongLetters.HasExplicitParticipations = true;
            wrongLetters.Participations = AllRoundParticipations("A", 2, 2);
            wrongLetters.Participations[0].SectionLetters = "B";
            Case("participation-sectionletters-mismatch", wrongLetters, dto => dto.Validate());
            var roundWithoutFlag = SectionUpdate();
            roundWithoutFlag.Participations = new[] { Participation(round: 2) };
            Case("round-above-one-without-flag", roundWithoutFlag, dto => dto.Validate());
        }

        private static ParticipationDTO[] AllRoundParticipations(string letters, int tables, int rounds)
        {
            var participations = new List<ParticipationDTO>();
            for (var round = 1; round <= rounds; round++)
            {
                var number = 1001;
                for (var table = 1; table <= tables; table++)
                    foreach (var direction in new[] { TableDirection.North, TableDirection.East, TableDirection.South, TableDirection.West })
                        participations.Add(Participation(table, direction, round, (number++).ToString(), letters));
            }
            return participations.ToArray();
        }

        private void EmitScoringGroupCases()
        {
            Case("valid", ScoringGroup(), dto => dto.Validate());
            Case("invalid-sessionguid", ScoringGroup(guid: "9"), dto => dto.Validate());
            var number = ScoringGroup();
            number.ScoringGroupNumber = 0;
            Case("invalid-scoringgroupnumber", number, dto => dto.Validate());
            var method = ScoringGroup();
            method.ScoringMethod = 99;
            Case("invalid-scoringmethod", method, dto => dto.Validate());
            var deletedWithSections = ScoringGroup();
            deletedWithSections.IsDeleted = true;
            Case("deleted-with-sections", deletedWithSections, dto => dto.Validate());
            var deleted = ScoringGroup();
            deleted.IsDeleted = true;
            deleted.Sections = null;
            Case("valid-deleted-null-sections", deleted, dto => dto.Validate());
            var noSections = ScoringGroup();
            noSections.Sections = Array.Empty<SectionDTO>();
            Case("no-sections", noSections, dto => dto.Validate());
            var duplicateLetters = ScoringGroup();
            duplicateLetters.Sections = new[] { Section("A"), Section("A") };
            Case("duplicate-section-letters", duplicateLetters, dto => dto.Validate());
            var guidMismatch = ScoringGroup();
            guidMismatch.Sections = new[] { Section("A", guid: Guid2) };
            Case("section-sessionguid-mismatch", guidMismatch, dto => dto.Validate());
            var groupMismatch = ScoringGroup();
            groupMismatch.Sections = new[] { Section("A", scoringGroupNumber: 9) };
            Case("section-scoringgroupnumber-mismatch", groupMismatch, dto => dto.Validate());
            var cascade = ScoringGroup();
            cascade.Sections[0].Winners = 9;
            Case("section-cascade", cascade, dto => dto.Validate());
        }

        private void EmitSessionCases()
        {
            var forReading = new Dictionary<string, object> { ["forAdding"] = false };
            var forAdding = new Dictionary<string, object> { ["forAdding"] = true };
            bool Validate(SessionDTO dto) => dto.Validate(forAdding: false);

            Case("valid", Session(), Validate, forReading);
            var withEvent = Session();
            withEvent.EventGuid = EventGuid1;
            Case("valid-for-adding", withEvent, dto => dto.Validate(forAdding: true), forAdding);
            Case("invalid-sessionguid", Session(guid: "12"), Validate, forReading);
            Case("for-adding-missing-eventguid", Session(), dto => dto.Validate(forAdding: true), forAdding);
            var badEvent = Session();
            badEvent.EventGuid = "xyz";
            Case("invalid-eventguid", badEvent, Validate, forReading);
            var noGroups = Session();
            noGroups.ScoringGroups = null;
            Case("no-scoringgroups", noGroups, Validate, forReading);
            var duplicateGroups = Session();
            duplicateGroups.ScoringGroups = new[] { ScoringGroup(1, "A"), ScoringGroup(1, "B") };
            Case("duplicate-scoringgroupnumbers", duplicateGroups, Validate, forReading);
            var groupGuid = Session();
            groupGuid.ScoringGroups = new[] { ScoringGroup(1, "A", Guid2) };
            Case("scoringgroup-sessionguid-mismatch", groupGuid, Validate, forReading);
            var cascade = Session();
            cascade.ScoringGroups[0].ScoringMethod = 12;
            Case("scoringgroup-cascade", cascade, Validate, forReading);
            var combiSame = Session();
            var combiSection = Section("C", 1, Guid1, 2);
            combiSection.IsCombiSection = true;
            combiSection.NorthSouthPairSectionLetters = "A";
            combiSection.EastWestPairSectionLetters = "A";
            combiSame.ScoringGroups = new[] { ScoringGroup(1, "A"), new ScoringGroupDTO
            {
                SessionGuid = Guid1,
                ScoringGroupNumber = 2,
                ScoringMethod = ScoringGroupDTO.ScoringType_Pairs,
                Name = "Combi",
                Sections = new[] { combiSection }
            } };
            Case("combi-sources-not-distinct", combiSame, Validate, forReading);
            var combiMissing = Session();
            var combiSection2 = Section("C", 1, Guid1, 2);
            combiSection2.IsCombiSection = true;
            combiSection2.NorthSouthPairSectionLetters = "A";
            combiSection2.EastWestPairSectionLetters = "B";
            combiMissing.ScoringGroups = new[] { ScoringGroup(1, "A"), new ScoringGroupDTO
            {
                SessionGuid = Guid1,
                ScoringGroupNumber = 2,
                ScoringMethod = ScoringGroupDTO.ScoringType_Pairs,
                Name = "Combi",
                Sections = new[] { combiSection2 }
            } };
            Case("combi-source-missing", combiMissing, Validate, forReading);
            var noName = Session();
            noName.Name = "";
            Case("missing-name", noName, Validate, forReading);
            var year = Session();
            year.Year = 1999;
            Case("invalid-year", year, Validate, forReading);
            var month = Session();
            month.Month = 13;
            Case("invalid-month", month, Validate, forReading);
            var day = Session();
            day.Day = 0;
            Case("invalid-day", day, Validate, forReading);
            var hour = Session();
            hour.Hour = 24;
            Case("invalid-hour", hour, Validate, forReading);
            var minute = Session();
            minute.Minute = 60;
            Case("invalid-minute", minute, Validate, forReading);
            var badDate = Session();
            badDate.Month = 2;
            badDate.Day = 30;
            Case("invalid-date", badDate, Validate, forReading);
        }

        private void EmitInitCases()
        {
            Case("valid", Init(), dto => dto.Validate());
            //The headline BRID-2037 fixture: an individual-style section that declares explicit
            //participations and sends the complete seating for every round.
            var explicitInit = Init();
            explicitInit.Sessions[0].ScoringGroups[0].Sections[0].HasExplicitParticipations = true;
            explicitInit.Participations = AllRoundParticipations("A", tables: 2, rounds: 2);
            Case("valid-explicit-participations-all-rounds", explicitInit, dto => dto.Validate());
            var roundWithoutFlag = Init();
            roundWithoutFlag.Participations = roundWithoutFlag.Participations!
                .Concat(new[] { Participation(1, TableDirection.North, round: 2, playerNumber: "1001") })
                .ToArray();
            Case("round-above-one-without-flag", roundWithoutFlag, dto => dto.Validate());
            var commands = Init();
            commands.Commands = 300;
            Case("commands-out-of-range", commands, dto => dto.Validate());
            var noSessions = Init();
            noSessions.Sessions = null;
            noSessions.Participations = null;
            noSessions.PlayerData = null;
            Case("no-sessions", noSessions, dto => dto.Validate());
            var multiNoEvent = Init();
            multiNoEvent.Sessions = new[] { Session(), Session(Guid2, 2, "B") };
            Case("multi-session-missing-eventguid", multiNoEvent, dto => dto.Validate());
            var mismatch = Init();
            mismatch.EventGuid = EventGuid1;
            var session1 = Session();
            session1.EventGuid = EventGuid1;
            var session2 = Session(Guid2, 2, "B");
            session2.EventGuid = null;
            mismatch.Sessions = new[] { session1, session2 };
            Case("session-eventguid-mismatch", mismatch, dto => dto.Validate());
            var badEventGuid = Init();
            badEventGuid.EventGuid = "XYZ";
            Case("invalid-eventguid", badEventGuid, dto => dto.Validate());
            var sessionCascade = Init();
            sessionCascade.Sessions[0].Name = null;
            Case("session-cascade", sessionCascade, dto => dto.Validate());
            var duplicateSections = Init();
            duplicateSections.Sessions[0].ScoringGroups = new[] { ScoringGroup(1, "A"), ScoringGroup(2, "A") };
            Case("duplicate-section-letters", duplicateSections, dto => dto.Validate());
            var duplicateGroups = Init();
            duplicateGroups.EventGuid = EventGuid1;
            var sessionA = Session();
            sessionA.EventGuid = EventGuid1;
            var sessionB = Session(Guid2, 1, "B");
            sessionB.EventGuid = EventGuid1;
            duplicateGroups.Sessions = new[] { sessionA, sessionB };
            Case("duplicate-scoringgroupnumbers", duplicateGroups, dto => dto.Validate());
            var playerCascade = Init();
            playerCascade.PlayerData![0].LastName = "";
            Case("playerdata-cascade", playerCascade, dto => dto.Validate());
            var playerWrongSession = Init();
            playerWrongSession.PlayerData![0].SessionGuid = Guid2;
            //The matching participation for player 1001 now points at a missing player as well.
            Case("playerdata-unknown-session", playerWrongSession, dto => dto.Validate());
            var duplicatePlayers = Init();
            duplicatePlayers.PlayerData![1].PlayerNumber = "1001";
            //Participation 1002 loses its player on purpose: the duplicate hides it.
            Case("duplicate-playernumbers", duplicatePlayers, dto => dto.Validate());
            var participationsWithoutPlayers = Init();
            participationsWithoutPlayers.PlayerData = null;
            Case("participations-without-playerdata", participationsWithoutPlayers, dto => dto.Validate());
            var participationCascade = Init();
            participationCascade.Participations![0].TableNumber = 0;
            Case("participation-cascade", participationCascade, dto => dto.Validate());
            var orphan = Init();
            orphan.Participations![0].PlayerNumber = "9999";
            Case("participation-without-playerdata", orphan, dto => dto.Validate());
            var handrecordCascade = Init();
            handrecordCascade.Handrecords = new[] { Handrecord() };
            handrecordCascade.Handrecords[0].BoardNumber = 0;
            Case("handrecord-cascade", handrecordCascade, dto => dto.Validate());
            var validWithExtras = Init();
            validWithExtras.Handrecords = new[] { Handrecord() };
            validWithExtras.Bridgemate2Settings = new[] { Bm2Settings() };
            validWithExtras.Bridgemate3Settings = new[] { Bm3Settings() };
            Case("valid-with-handrecords-and-settings", validWithExtras, dto => dto.Validate());
            var bm2Cascade = Init();
            bm2Cascade.Bridgemate2Settings = new[] { Bm2Settings() };
            bm2Cascade.Bridgemate2Settings[0].BM2PINcode = "12";
            Case("bridgemate2-cascade", bm2Cascade, dto => dto.Validate());
            var bm2Duplicates = Init();
            bm2Duplicates.Bridgemate2Settings = new[] { Bm2Settings(), Bm2Settings() };
            Case("bridgemate2-duplicate-letters", bm2Duplicates, dto => dto.Validate());
            var bm3Cascade = Init();
            bm3Cascade.Bridgemate3Settings = new[] { Bm3Settings() };
            bm3Cascade.Bridgemate3Settings[0].BM3ScreenBrightness = 8;
            Case("bridgemate3-cascade", bm3Cascade, dto => dto.Validate());
        }

        private void EmitHandrecordCases()
        {
            Case("valid", Handrecord(), dto => dto.Validate());
            Case("invalid-sessionguid", WithGuid(Handrecord(), "GUID"), dto => dto.Validate());
            var group = Handrecord();
            group.ScoringGroupNumber = 0;
            Case("invalid-scoringgroupnumber", group, dto => dto.Validate());
            var letters = Handrecord();
            letters.SectionLetters = "A2";
            Case("invalid-sectionletters", letters, dto => dto.Validate());
            var board = Handrecord();
            board.BoardNumber = 0;
            Case("invalid-boardnumber", board, dto => dto.Validate());
            var nullSuit = Handrecord();
            nullSuit.NorthSpades = null;
            Case("null-suit", nullSuit, dto => dto.Validate());
            var badCard = Handrecord();
            badCard.NorthSpades = "AKQJT9876543X";
            Case("invalid-card", badCard, dto => dto.Validate());
            var shortSuit = Handrecord();
            shortSuit.NorthSpades = "AKQJT987654";
            Case("suit-not-thirteen-cards", shortSuit, dto => dto.Validate());
            var duplicateCard = Handrecord();
            duplicateCard.NorthSpades = "AAKQJT9876543";
            Case("duplicate-card-in-suit", duplicateCard, dto => dto.Validate());
        }

        private static HandrecordDTO WithGuid(HandrecordDTO dto, string guid)
        {
            dto.SessionGuid = guid;
            return dto;
        }

        private void EmitResultCases()
        {
            Case("valid", Result(), dto => dto.Validate());
            var guid = Result();
            guid.SessionGuid = "abc";
            Case("invalid-sessionguid", guid, dto => dto.Validate());
            var letters = Result();
            letters.SectionLetters = "1";
            Case("invalid-sectionletters", letters, dto => dto.Validate());
            var table = Result();
            table.TableNumber = 0;
            Case("invalid-tablenumber", table, dto => dto.Validate());
            var round = Result();
            round.RoundNumber = 0;
            Case("invalid-roundnumber", round, dto => dto.Validate());
            var board = Result();
            board.BoardNumber = 0;
            Case("invalid-boardnumber", board, dto => dto.Validate());
            var deleted = Result();
            deleted.IsDeleted = true;
            deleted.PairNorthSouth = 0;
            deleted.PairEastWest = 0;
            //IsDeleted skips the pair/contract checks: only the header rules apply.
            Case("valid-deleted-header-only", deleted, dto => dto.Validate());
            var pairEw = Result();
            pairEw.PairEastWest = 0;
            Case("invalid-paireastwest", pairEw, dto => dto.Validate());
            var pairNs = Result();
            pairNs.PairNorthSouth = 0;
            Case("invalid-pairnorthsouth", pairNs, dto => dto.Validate());
            var declaring = Result();
            declaring.DeclaringPair = 3;
            Case("invalid-declaringpair", declaring, dto => dto.Validate());
            var declarer = Result();
            declarer.DeclarerDirection = 5;
            Case("invalid-declarerdirection", declarer, dto => dto.Validate());
            var scoringDirection = Result();
            scoringDirection.ScoringDirection = 4;
            Case("invalid-scoringdirection", scoringDirection, dto => dto.Validate());
            var level = Result();
            level.Level = 8;
            Case("invalid-level", level, dto => dto.Validate());
            var denomination = Result();
            denomination.Denomination = 6;
            Case("invalid-denomination", denomination, dto => dto.Validate());
            var stake = Result();
            stake.Stake = 3;
            Case("invalid-stake", stake, dto => dto.Validate());
            var tricks = Result();
            tricks.TotalTricks = 14;
            Case("invalid-totaltricks", tricks, dto => dto.Validate());
            var leadRank = Result();
            leadRank.LeadCardRank = 15;
            //LeadCardRank and LeadCardSuit are an if/else-if pair: an invalid rank suppresses the suit rule.
            leadRank.LeadCardSuit = 9;
            Case("invalid-leadcardrank-suppresses-suit", leadRank, dto => dto.Validate());
            var leadSuit = Result();
            leadSuit.LeadCardRank = 2;
            leadSuit.LeadCardSuit = 5;
            Case("invalid-leadcardsuit", leadSuit, dto => dto.Validate());
            var passedIn = Result();
            passedIn.Level = 0;
            passedIn.Denomination = 0;
            passedIn.DeclarerDirection = 0;
            passedIn.ScoringDirection = 0;
            passedIn.TotalTricks = 0;
            Case("valid-passed-in", passedIn, dto => dto.Validate());
        }

        private void EmitTdCallCases()
        {
            Case("valid", TdCall(), dto => dto.Validate());
            var guid = TdCall();
            guid.SessionGuid = "g";
            Case("invalid-sessionguid", guid, dto => dto.Validate());
            var letters = TdCall();
            letters.SectionLetters = "aa";
            Case("invalid-sectionletters", letters, dto => dto.Validate());
            var table = TdCall();
            table.TableNumber = 0;
            Case("invalid-tablenumber", table, dto => dto.Validate());
            var round = TdCall();
            round.RoundNumber = 0;
            Case("invalid-roundnumber", round, dto => dto.Validate());
            var statusLow = TdCall();
            statusLow.Status = 0;
            Case("invalid-status-zero", statusLow, dto => dto.Validate());
            var statusHigh = TdCall();
            statusHigh.Status = 5;
            Case("invalid-status-five", statusHigh, dto => dto.Validate());
        }

        private void EmitBridgemateSettingsCases()
        {
            Case("valid", Bm2Settings(), dto => dto.Validate());
            var guid = Bm2Settings();
            guid.SessionGuid = "abc";
            Case("invalid-sessionguid", guid, dto => dto.Validate());
            var letters = Bm2Settings();
            letters.SectionLetters = "A9";
            Case("invalid-sectionletters", letters, dto => dto.Validate());
            var pinShort = Bm2Settings();
            pinShort.BM2PINcode = "123";
            Case("invalid-pincode-short", pinShort, dto => dto.Validate());
            var pinAlpha = Bm2Settings();
            pinAlpha.BM2PINcode = "12A4";
            Case("invalid-pincode-alpha", pinAlpha, dto => dto.Validate());
            //int.TryParse accepts a leading sign, so "+123" is four chars that parse: a port using
            //a digits-only regex would diverge here.
            var pinSigned = Bm2Settings();
            pinSigned.BM2PINcode = "+123";
            Case("valid-pincode-signed", pinSigned, dto => dto.Validate());

            Case("valid", Bm3Settings(), dto => dto.Validate());
            var dim = Bm3Settings();
            dim.BM3ScreenDimMode = 16;
            Case("invalid-screendimmode", dim, dto => dto.Validate());
            var brightness = Bm3Settings();
            brightness.BM3ScreenBrightness = 0;
            Case("invalid-screenbrightness", brightness, dto => dto.Validate());
            var sleep = Bm3Settings();
            sleep.BM3SleepMode = 121;
            Case("invalid-sleepmode", sleep, dto => dto.Validate());
            var volume = Bm3Settings();
            volume.BM3AudioVolume = 8;
            Case("invalid-audiovolume", volume, dto => dto.Validate());
            var pin = Bm3Settings();
            pin.BM3PINcode = "12345";
            Case("invalid-pincode", pin, dto => dto.Validate());
        }

        private void EmitContinueCases()
        {
            Case("valid", Continue(), dto => dto.Validate());
            var combined = Continue();
            combined.Commands = InitDTO.StartBCS + InitDTO.Command_StartReading + InitDTO.Command_ClearData;
            Case("valid-combined-commands", combined, dto => dto.Validate());
            var invalid = Continue();
            invalid.Commands = InitDTO.Command_Reset;
            Case("invalid-commands", invalid, dto => dto.Validate());
        }
    }
}
