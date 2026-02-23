using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BridgeSystems.Bridgemate.DataConnectorClasses.SharedDTO
{
    /// <summary>
    /// Contains information for an updated or added section
    /// </summary>
    public class SectionUpdateDTO
    {

        /// <summary>
        /// Creates a new SectionUpdateDTO
        /// </summary>
        public SectionUpdateDTO()
        {
        }
        /// <summary>
        /// Required. The guid of the session the section is part of.
        /// A string built from a Guid, without the curly braces or connecting dashes.
        /// The letters must be capitals
        /// </summary>
        public string SessionGuid
        {
            get; set;
        }

        /// <summary>
        /// Required when using the http interface for the dataconnector
        /// </summary>
        public string ClubId
        { get; set; }

        /// <summary>
        /// Required, the number of the scoringgroup the section belongs to. Must be greater than zero.
        /// If the scoringgroup already exists the section wull be added to it, otherwise a new scoringgroup wil be added.
        /// </summary>
        public int ScoringGroupNumber
        {
            get; set;
        }

        /// <summary>
        /// Must be set and must match the scoringmethod of the scoringgroup if the section is added to one, otherwise it will specify the 
        /// scoringmethod for the new scoringgroup. See for valid values the ScoringType contstants in the ScoringGroupDTO.
        /// </summary>
        public int ScoringGroupScoringMethod
        {
            get; set;
        }

        /// <summary>
        /// Required. The letters of the section. If a section with this letters already exists it will be updated. 
        /// Otherwise the section will be added.
        /// Can be A-Z, AA, BB, CC,...ZZ, AAA, BBB, CCC, ... ZZZ
        /// </summary>
        public string Letters
        {
            get; set;
        }

        /// <summary>
        /// Optional.
        /// </summary>
        public string Name
        {
            get; set;
        }

        /// <summary>
        /// Required: either 1 or 2.
        /// </summary>
        public int Winners
        {
            get; set;
        }

        /// <summary>
        /// Required:  Pairs = 10,
        //             Indvidual = 20,
        //             Teams = 30
        /// </summary>
        /// <remarks>
        /// Note: this value does not represent the scoring method (matchpoints, Imps, BAM,...). 
        /// This must be specified at the scoringgroup level.
        /// </remarks
        public int GameType
        {
            get; set;
        }

        /// <summary>
        /// Negative value: the eastwest pairs move down the specified number of tables before play starts.
        /// Positive value: the eastwest pairs move up the specified number of tables before play starts.
        /// Zero: the eastwest pairs do not move.
        /// Currently not supported.
        /// </summary>
        public int EWMoveBeforePlay
        {
            get; set;
        }

        /// <summary>
        /// Optional. A pair that is missing throughout the movement. Alternatively this can be specified by
        /// Leaving out the opponent for a pair that would have met the missing pair. However, BCS will be able
        /// to discern a sit-out table from an empty table if this value is used.
        /// When using a two winner movement a positive number denotes a missing NS pair, a negative value denotes a missing EW pair.
        /// </summary>
        public int MissingPair
        {
            get; set;
        }

        /// <summary>
        /// When "true" the Bridgemate (2) server will be instructed to net close the session when the result for the last available round
        /// have been received. In this way it is possible to play Swiss matches.
        /// </summary>
        public bool KeepBridgematesAlive { get; set; }

        /// <summary>
        /// If true the SitoutSectionLetters must have the value for an existing section.
        /// </summary>
        public bool IsCombiSection
        {
            get; set;
        }

        /// <summary>
        /// Required if IsCombiSection is True. Must be the letters for an existing section.
        /// The letters of the section the North-South pairs in the combisection originate from.
        /// </summary>
        public string NorthSouthPairSectionLetters
        {
            get; set;
        }

        /// <summary>
        /// Required if IsCombisection is True. Must be the letters for an existing section.
        /// The letters of the section the East-West pairs in the combisection originate from.
        /// </summary>
        public string EastWestPairSectionLetters
        {
            get; set;
        }

        /// <summary>
        /// Marks the section for deletion. No tables must be specified. ALl properties other than the SessionGuid an Letters are ignored.
        /// </summary>
        public bool IsDeleted
        {
            get; set;
        }

        /// <summary>
        /// Optional. Any players that have been added and were not included while initializing the session.
        /// </summary>
        public PlayerDataDTO[] AddedPlayers
        {
            get; set;
        }

        /// <summary>
        /// Optional. The players as they are seated in the first round. Be sure to include players as well that do not play in the first round.
        /// </summary>
        public ParticipationDTO[] Participations
        {
            get; set;
        }

        /// <summary>
        /// An array of TableDTOs. All tables for the section must be specified. If the section is to be deleted, no tables must be specified.
        /// </summary>
        public TableDTO[] Tables
        {
            get; set;
        }

        /// <summary>
        /// Creates a SectionUpdateDTO from a SectionDTO
        /// </summary>
        /// <param name="section"></param>
        /// <param name="scoringMethod"></param>
        /// <param name="isDeleted"/>
        /// <returns></returns>
        public static SectionUpdateDTO CreateFromSectionDTO(SectionDTO section, int scoringMethod, bool isDeleted = false)
        {
            var updatedSection = new SectionUpdateDTO
            {
                SessionGuid = section.SessionGuid,
                Letters = section.Letters,
                Winners = section.Winners,
                GameType = section.GameType,
                ScoringGroupNumber = section.ScoringGroupNumber,
                ScoringGroupScoringMethod = scoringMethod,
                KeepBridgematesAlive = section.KeepBridgematesAlive,
                IsDeleted = isDeleted,
                IsCombiSection = section.IsCombiSection,
                NorthSouthPairSectionLetters = section.NorthSouthPairSectionLetters,
                EastWestPairSectionLetters = section.EastWestPairSectionLetters,
                Tables = section.Tables,
                Participations = new ParticipationDTO[] { }
            };
            if (isDeleted)
                updatedSection.Tables = new TableDTO[] { };
            return updatedSection;
        }

        /// <summary>
        /// An array of messages describing problems in data integrity and invalid values.
        /// </summary>
        public string[] ValidationMessages
        {
            get; set;
        }

        /// <summary>
        /// Validates the DTO. Produces validation messages if there are problems. 
        /// </summary>
        /// <returns>True if there are no validation errors.</returns>
        public bool Validate()
        {
            var validationMessages = new List<string>();
            if (SessionGuid == null || SessionGuid.Length != 32 || SessionGuid.Any(c => !(c >= 'A' && c <= 'F' || c >= '0' && c <= '9')))
            {
                validationMessages.Add("The guid must be exactly 32 character long and can only contain capital A to F or digits 0 to 9.");
            }
            if (!Regex.IsMatch(Letters ?? "", @"^([A-Z])\1{0,2}$"))
            {
                validationMessages.Add($"Invalid {nameof(Letters)} ({Letters}). Valid values are: 'A-Z', 'AA-ZZ' or 'AAA','ZZZ'");
            }
            if (IsDeleted)
            {
                if (Tables != null && Tables.Any())
                {
                    validationMessages.Add("A deleted section cannot contain tables.");
                }
                return !validationMessages.Any();
            }
            if (ScoringGroupNumber <= 0)
            {
                validationMessages.Add($"{nameof(ScoringGroupNumber)} ({ScoringGroupNumber}) must be greater than zero.");
            }
            if (!new[] {ScoringGroupDTO.ScoringType_Pairs,
                ScoringGroupDTO.ScoringType_Imp2_Weighted,ScoringGroupDTO.ScoringType_Imp2_10Percent,ScoringGroupDTO.ScoringType_Imp2_NoCorrection,
                ScoringGroupDTO.ScoringType_Imp3_Weighted,ScoringGroupDTO.ScoringType_Imp3_10Percent,ScoringGroupDTO.ScoringType_Imp3_NoCorrection,
                ScoringGroupDTO.ScoringType_XImp2_Total,ScoringGroupDTO.ScoringType_XImp2_Average,
                ScoringGroupDTO.ScoringType_XImp3_Total,ScoringGroupDTO.ScoringType_XImp3_Average,
                ScoringGroupDTO.ScoringType_TeamImps, ScoringGroupDTO.ScoringType_TeamVPDiscrete, ScoringGroupDTO.ScoringType_TeamVPContinuous,
                ScoringGroupDTO.ScoringType_Bam,ScoringGroupDTO. ScoringType_Patton }.Contains(ScoringGroupScoringMethod))
            {
                validationMessages.Add($"Invalid {nameof(ScoringGroupScoringMethod)} ({ScoringGroupScoringMethod}). The value must be a multiple of 10 between 10 and 70 or 51. ");
            }
            if (MissingPair < 0 && Winners != 2)
            {
                validationMessages.Add($"{nameof(MissingPair)} ({MissingPair}) must at least be zero for a one winner section.");
            }
            if (Winners < 1 || Winners > 2)
            {
                validationMessages.Add($"Invalid {nameof(Winners)} ({Winners}). Valid values are 1 or 2.");
            }
            if (GameType != 10 && GameType != 20 && GameType != 30)
            {
                validationMessages.Add($"Invalid {nameof(GameType)} ({GameType}). Valid values are 10, 20 or 30.");
            }
            if (IsCombiSection)
            {
                if (!Regex.IsMatch(NorthSouthPairSectionLetters ?? "", @"^([A-Z])\1{0,2}$"))
                {
                    validationMessages.Add($"Invalid {nameof(NorthSouthPairSectionLetters)} ({NorthSouthPairSectionLetters}). Valid values are: 'A-Z', 'AA-ZZ' or 'AAA','ZZZ'");
                }
                if (!Regex.IsMatch(EastWestPairSectionLetters ?? "", @"^([A-Z])\1{0,2}$"))
                {
                    validationMessages.Add($"Invalid {nameof(EastWestPairSectionLetters)} ({EastWestPairSectionLetters}). Valid values are: 'A-Z', 'AA-ZZ' or 'AAA','ZZZ'");
                }
            }
            if (Math.Abs(EWMoveBeforePlay) > Tables.Count())
            {
                validationMessages.Add($"The absolute value of {nameof(EWMoveBeforePlay)} ({EWMoveBeforePlay}) " +
                                       $"cannot be higher than the number of tables ({Tables.Count()}).");
            }

            foreach (TableDTO table in Tables)
            {
                if (table.SessionGuid != SessionGuid)
                {
                    validationMessages.Add($"Table '{Letters}{table.TableNumber}' must have {nameof(ScoringGroupDTO.SessionGuid)} '{SessionGuid}' " +
                                           $"but it is '{table.SessionGuid}'");
                }
                if (table.SectionLetters != Letters)
                {
                    validationMessages.Add($"Table ' {Letters} {table.TableNumber}' must have {nameof(TableDTO.SectionLetters)} '{Letters}' " +
                                           $"but it is '{table.SectionLetters}'");
                }
                if (!table.Validate())
                {
                    var errorMessage = string.Join("; ", table.ValidationMessages);
                    validationMessages.Add($"Table '{table.SectionLetters}{table.TableNumber}' has validation errrors: {errorMessage}.");
                }
            }
            var tableNumbers = Tables.Select(t => t.TableNumber).OrderBy(nr => nr).ToList();
            var groupedTableNumbers = tableNumbers.GroupBy(nr => nr);

            foreach (var numberGroup in groupedTableNumbers.Where(g => g.Count() > 1))
            {
                validationMessages.Add($"Tablenumber {numberGroup.Key} occurs {numberGroup.Count()} times. ");
            }
            ValidationMessages = validationMessages.ToArray();
            return !ValidationMessages.Any();
        }

        /// <summary>
        /// Returns a string representation of the DTO.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {

            var description = IsDeleted ? 
                $"Section {Letters} '{Name}' will be deleted." :
                $"Section {Letters} '{Name}'; tables: {Tables?.Count()}, scoringGroup {ScoringGroupNumber}, game type {GameType}";
            if (Tables != null)
            {
                foreach (TableDTO table in Tables)
                {
                    description += $"{Environment.NewLine}" +
                        $"   Table {Letters}{table.TableNumber}";
                    description += $"{Environment.NewLine}";
                    if (table.Rounds != null)
                    {

                        foreach (RoundDTO round in table.Rounds)
                        {
                            description += $"    R{round.RoundNumber}: " +
                                $"{round.PairNS,-2}-{round.PairEW,-2}";
                        }
                    }
                }
            }
            return description;
        }
    }
}
