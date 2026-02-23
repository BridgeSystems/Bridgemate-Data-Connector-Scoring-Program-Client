using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BridgeSystems.Bridgemate.DataConnectorClasses.SharedDTO
{
    /// <summary>
    /// The RoundDTO describes the seating for a round on a table: the NS and EW pairs 
    /// and the board numbers expressed by the LowBoardNumber and HighBoardNumber properties.
    /// </summary>
    public class RoundDTO
    {
        /// <summary>
        /// Required. The guid of the session the round belongs to.
        /// </summary>
        public string SessionGuid
        {
            get; set;
        }
        /// <summary>
        /// Required, the letters of the section.
        /// </summary>
        public string SectionLetters
        {
            get; set;
        }

        /// <summary>
        /// Required, must be greater than zero.
        /// </summary>
        public int TableNumber
        {
            get; set;
        }

        /// <summary>
        /// Required, must be greater than zero.
        /// </summary>
        public int RoundNumber
        {
            get; set;
        }

        /// <summary>
        /// Optional, can be zero if the table is empty (has no pairs) or is a sit-out (One pair is absent).
        /// </summary>
        public int LowBoardNumber
        {
            get; set;
        }

        /// <summary>
        /// Optional, can be zero if the table is empty (has no pairs) or is a sit-out (One pair is absent).
        /// </summary>
        public int HighBoardNumber
        {
            get; set;
        }

        /// <summary>
        /// Required, can be zero if the table is empty or the opponents have a sit-out.
        /// </summary>
        public int PairNS
        {
            get; set;
        }

        /// <summary>
        /// Required, can be zero if the table is empty or the opponents have a sit-out.
        /// </summary>
        public int PairEW
        {
            get; set;
        }

        /// <summary>
        /// Optional. The number of the team the NS pair belongs to.
        /// </summary>
        public int TeamNS
        {
            get;set;
        }

        /// <summary>
        /// Optional. The number of the team the EW pair belongs to.
        /// </summary>
        public int TeamEW
        {
            get;set;
        }

        /// <summary>
        /// Optional. The letters of the section where the teammates play the same boards.
        /// If used the MatesTableNumber and MatesTableRoundNumber properties must be set as well.
        /// </summary>
        public string MatesTableSectionLetters
        {
            get; set;
        }

        /// <summary>
        /// Optional. The number of the table where the teammates play the same boards.
        /// If used the MatesTableSectionLetters and MatesTableRoundNumber properties must be set as well.
        /// </summary>
        public int MatesTableTableNumber
        {
            get; set;
        }

        /// <summary>
        /// Optional. The number of the round where the teammates play the same boards.
        /// If used the MatesTableSectionLetters and the MatesTableNumber properties must be set as well.
        /// </summary>
        public int MatesTableRoundNumber
        {
            get; set;
        }

        /// <summary>
        /// Currently not in use for scoring programs. Used for internal administration when handling movement updates.
        /// </summary>
        public bool Updated
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
        public bool Validate()
        {
            var validationMessages=new List<string>();
            if (SessionGuid == null || SessionGuid.Length != 32 || SessionGuid.Any(c => !(c >= 'A' && c <= 'F' || c >= '0' && c <= '9')))
            {
                validationMessages.Add("The guid must be exactly 32 character long and can only contain capital A to F or digits 0 to 9.");
            }
            if (!Regex.IsMatch(SectionLetters ?? "", @"^([A-Z])\1{0,2}$"))
            {
                validationMessages.Add($"Invalid {nameof(SectionLetters)} ({SectionLetters}). Valid values are: 'A-Z', 'AA-ZZ' or 'AAA','ZZZ'");
            }
            if (TableNumber <= 0)
            {
                validationMessages.Add($"{nameof(TableNumber)} ({TableNumber}) must be greater than zero.");
            }
            if (RoundNumber <= 0)
            {
                validationMessages.Add($"{nameof(RoundNumber)} ({RoundNumber}) must be greater than zero.");
            }
            if ((PairNS == 0 || PairEW == 0) && (LowBoardNumber>0 || HighBoardNumber>0)) 
            {
                validationMessages.Add($"No boards may be specified for a sit-out round or an empty round.");
            }
            ValidationMessages=validationMessages.ToArray();
            return !ValidationMessages.Any();
        }

        public override string ToString()
        {
            return $"Table {SectionLetters}{TableNumber} Round {RoundNumber}: {PairNS}-{PairEW}: {LowBoardNumber}-{HighBoardNumber}";
        }

        public RoundDTO Clone()
        {
            return (RoundDTO)MemberwiseClone();
        }

    }
}
