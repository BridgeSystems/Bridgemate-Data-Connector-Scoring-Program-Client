
using BridgeSystems.Bridgemate.DataConnector.ScoringProgramClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BridgeSystems.Bridgemate.DataConnectorClasses.SharedDTO
{
    /// <summary>
    /// This class can be used:
    /// 1. To give names for the players in the first round by only specifying their player numbers.
    ///    For this to work the names belonging to the playernumber must have been passed first using
    ///    PlayerDTO's.
    /// 2. To give names for the players in the first round by omitting their playernumber. In this case the
    ///    players will be registerd for this session only.
    /// 3. To specify the seating for every round explicitly, one DTO per (table, round, position). This
    ///    requires the section to have been created with
    ///    <see cref="SectionDTO.HasExplicitParticipations"/> set to true. BCS then stores each
    ///    participation for its own round exactly as sent and does not calculate seatings from the
    ///    movement. Use this for individual sessions and other formats where partnerships change
    ///    between rounds.
    /// 4. To report that a seat was emptied (<see cref="IsRemoval"/>): the player who sat there was removed. Such a DTO
    ///    carries no player number and no name.
    /// The combination of both playernumber and name details is not supported.
    /// </summary>
    public class ParticipationDTO
    {
        /// <summary>
        /// Required. The guid of the session the participation belongs to.
        /// A string built from a Guid, without the curly braces or connecting dashes.
        /// The letters must be capitals
        /// </summary>
        public string SessionGuid
        {
            get; set;
        }
        /// <summary>
        /// Required. The letters of the section the participation belongs to.
        /// Can be A-Z, AA, BB, CC,...ZZ, AAA, BBB, CCC, ... ZZZ
        /// </summary>
        public string SectionLetters
        {
            get; set;
        }

        /// <summary>
        /// Required. The number of the table for the player in the round.
        /// </summary>
        public int TableNumber
        {
            get; set;
        }

        /// <summary>
        /// Required. The position on the table for the player in the round.
        /// 1: North
        /// 2: East
        /// 3: South
        /// 4: West
        /// </summary>
        public TableDirection Direction
        {
            get; set;
        }

        /// <summary>
        /// Optional. The round the participation applies to. The values zero and one are equivalent: they
        /// denote the table where the player sits in the first round, and BCS will use the known movement
        /// to calculate the player's positions in the other rounds.
        /// Values higher than one are only valid for sections that were created with
        /// <see cref="SectionDTO.HasExplicitParticipations"/> set to true. For such sections BCS stores
        /// every participation for its own round exactly as sent and performs no movement-based
        /// calculation. Sending a round number higher than one for any other section is a validation
        /// error.
        /// </summary>
        public int RoundNumber
        {
            get; set;
        }

        /// <summary>
        /// Optional, but must be present together with the SessionGuid if no name details are passed.
        /// The playernumber must have been passed beforehand using a PlayerDTO containg the name details.
        /// </summary>
        public string PlayerNumber
        {
            get; set;
        }

        /// <summary>
        /// Signals that this player was swapped with his partner: North was swapped with South, or East with West.
        /// </summary>
        /// <remarks>This property is only menaningful in conjunction with a 
        /// <see cref="ScoringProgramDataConnectorCommands.PollQueueForNewParticipations">PollQueueForNewParticipations</see> command, or a 
        /// <see cref="ScoringProgramDataConnectorCommands.PollQueueForAllParticipations">PollQueueForAllParticipations</see>> command.</remarks>
        public bool IsPlayerSwap
        {
            get;set;
        }

        /// <summary>
        /// Signals that the player who sat at this seat was removed and the seat is now empty. The DTO then carries no
        /// player: <see cref="PlayerNumber"/>, <see cref="FirstName"/>, <see cref="LastName"/> and <see cref="CountryCode"/>
        /// must be empty. The seat is identified by the session, section, table, direction and round, with the same rules
        /// as any participation: round zero or one means the first round, in a section without explicit participations the
        /// player leaves the pair (every round), in a section with explicit participations only the given round is emptied.
        /// </summary>
        /// <remarks>BCS sends one per seat it cleared, in the queues that
        /// <see cref="ScoringProgramDataConnectorCommands.PollQueueForNewParticipations">PollQueueForNewParticipations</see> and
        /// <see cref="ScoringProgramDataConnectorCommands.PollQueueForAllParticipations">PollQueueForAllParticipations</see>
        /// return. A scoring program that does not know this property sees a participation without a player, which it should
        /// ignore.</remarks>
        public bool IsRemoval
        {
            get; set;
        }

        /// <summary>
        /// Optional, must only be used when the playernumber is empty.
        /// </summary>
        public string FirstName
        {
            get; set;
        }

        /// <summary>
        /// Required when the playernumber is empty.Must only be used when the playernumber is empty.
        /// </summary>
        public string LastName
        {
            get; set;
        }

        /// <summary>
        /// Optional, must only be used when the playernumber is empty.
        /// </summary>
        public string CountryCode
        {
            get; set;
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
        public bool Validate(bool allowPlayerNumberAndName)
        {
            var validationMessages =new List<string>();
            if (SessionGuid?.Length != 32)
            {
                validationMessages.Add($"Invalid {nameof(SessionGuid)} ({SessionGuid}). The value must be in capitals and be exactly 32 characters long.");
            }
            if (!Regex.IsMatch(SectionLetters ?? "", @"^([A-Z])\1{0,2}$"))
            {
                validationMessages.Add($"Invalid {nameof(SectionLetters)} ({SectionLetters}). Valid values are: 'A-Z', 'AA-ZZ' or 'AAA','ZZZ'");
            }
            if ((int)Direction < 1 || (int)Direction > 4)
            {
                validationMessages.Add($"Invalid {nameof(Direction)} ({Direction}). The value must be between 1 and 4.");
            }
            if (TableNumber < 1)
            {
                validationMessages.Add($"Invalid {nameof(TableNumber)} ({TableNumber}). The value must be greater than zero.");
            }
            if (RoundNumber < 0)
            {
                validationMessages.Add($"Invalid {nameof(RoundNumber)} ({RoundNumber}). The value cannot be negative.");
            }
            if (IsRemoval)
            {
                if (!string.IsNullOrWhiteSpace(PlayerNumber) || !string.IsNullOrWhiteSpace(FirstName) ||
                    !string.IsNullOrWhiteSpace(LastName) || !string.IsNullOrWhiteSpace(CountryCode))
                {
                    validationMessages.Add($"A removal ({nameof(IsRemoval)}) must not specify a {nameof(PlayerNumber)}, " +
                                           $"{nameof(FirstName)}, {nameof(LastName)} or {nameof(CountryCode)}.");
                }
                if (IsPlayerSwap)
                {
                    validationMessages.Add($"A participation cannot be both a removal ({nameof(IsRemoval)}) " +
                                           $"and a player swap ({nameof(IsPlayerSwap)}).");
                }
                ValidationMessages = validationMessages.ToArray();
                return !ValidationMessages.Any();
            }
            if (string.IsNullOrWhiteSpace(LastName) && string.IsNullOrWhiteSpace(PlayerNumber))
            {
                validationMessages.Add($"Either the {nameof(LastName)} or the {nameof(PlayerNumber)} must be specified.");
            }
            if (!allowPlayerNumberAndName && !string.IsNullOrWhiteSpace(LastName) && !string.IsNullOrWhiteSpace(PlayerNumber))
            {
                validationMessages.Add($"Either the {nameof(LastName)} or the {nameof(PlayerNumber)} must be specified, " +
                                       $"but not both.");
            }
          
            ValidationMessages = validationMessages.ToArray();
            return !ValidationMessages.Any();
        }

        public ParticipationDTO Clone() => (ParticipationDTO)MemberwiseClone();

        public override string ToString()
        {
            if (IsRemoval)
                return $"REMOVE {SectionLetters}{TableNumber} {Direction} round {RoundNumber}";
            return $"{(IsPlayerSwap?"SWAP ":"")}{SectionLetters}{TableNumber} {Direction} round {RoundNumber}: {PlayerNumber} {FirstName} {LastName}";
        }
    }
}
