using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BridgeSystems.Bridgemate.DataConnectorClasses.SharedDTO
{
    /// <summary>
    /// This class is a container for participants that can potentially meet each other during the session. Their results will be compared and they will be
    /// ranked against each other. 
    /// Note: if the section is part of a scoringgroup together with an other section the scoring calculation will treat these sections as one.
    /// </summary>
    public class SectionDTO
    {
        /// <summary>
        /// Constructor. Initializes the object with default values.
        /// </summary>
        public SectionDTO()
        { }
        /// <summary>
        /// The section hosts a pairs competition. Rankings will be for each pair.
        /// </summary>
        public const int GameType_Pairs = 10;

        /// <summary>
        /// The section hosts an individual competition. Rankings will be for each player.
        /// </summary>
        public const int GameType_Individual = 20;

        /// <summary>
        /// The section hosts a teams competition. Rankings will be for each teams (although a parallel pairs ranking is possible).
        /// </summary>
        public const int GameType_Teams = 30;

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
        /// Required, the number of the scoringgroup the section belongs to. Must be greater than zero.
        /// </summary>
        public int ScoringGroupNumber
        {
            get; set;
        }

        /// <summary>
        /// Required. The letters of the section. They must be unique within an event.
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
        /// If true the NorthSouthPairSectionLetters and the EastWestPairSectionLetters must have the value for an existing section.
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
        /// An array of TableDTOs. All tables for the section must be specified.
        /// </summary>
        public TableDTO[] Tables
        {
            get; set;
        }

        /// <summary>
        /// Creates a SectionDTO from an SectionUpdateDTO
        /// </summary>
        /// <param name="updatedSection"></param>
        /// <returns></returns>
        public static SectionDTO CreateFromSectionUpdateDTO(SectionUpdateDTO updatedSection)
        {
            var section = new SectionDTO
            {
                SessionGuid = updatedSection.SessionGuid,
                Letters = updatedSection.Letters,
                Winners = updatedSection.Winners,
                GameType = updatedSection.GameType,
                ScoringGroupNumber = updatedSection.ScoringGroupNumber,
                KeepBridgematesAlive = updatedSection.KeepBridgematesAlive,
                IsCombiSection = updatedSection.IsCombiSection,
                NorthSouthPairSectionLetters = updatedSection.NorthSouthPairSectionLetters,
                EastWestPairSectionLetters = updatedSection.EastWestPairSectionLetters,
                Tables = updatedSection.Tables,
            };
            return section;
        }

        /// <summary>
        /// An array of messages describing problems in data integrity and invalid values.
        /// </summary>
        public string[] ValidationMessages
        {
            get;set;
        }

        /// <summary>
        /// Validates the DTO. Produces validation messages if there are problems. 
        /// </summary>
        /// <returns>True if there are no validation errors.</returns>
        public bool Validate()
        {
            var validationMessages=new List<string>();
            if (SessionGuid == null || SessionGuid.Length != 32 || SessionGuid.Any(c => !(c >= 'A' && c <= 'F' || c >= '0' && c <= '9')))
            {
                validationMessages.Add("The guid must be exactly 32 character long and can only contain capital A to F or digits 0 to 9.");
            }
            if (!Regex.IsMatch(Letters ?? "", @"^([A-Z])\1{0,2}$"))
            {
                validationMessages.Add($"Invalid {nameof(Letters)} ({Letters}). Valid values are: 'A-Z', 'AA-ZZ' or 'AAA','ZZZ'");
            }
            if (ScoringGroupNumber <= 0)
            {
                validationMessages.Add($"{nameof(ScoringGroupNumber)} ({ScoringGroupNumber}) must be greater than zero.");
            }
            if (MissingPair < 0)
            {
                validationMessages.Add($"{nameof(MissingPair)} ({MissingPair}) must at least zero.");
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
            ValidationMessages=validationMessages.ToArray();
            return !ValidationMessages.Any();
        }

        public override string ToString()
        {

            var description = $"Section {Letters} '{Name}'; tables: {Tables?.Count()}, scoringGroup {ScoringGroupNumber}, game type {GameType}";
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
