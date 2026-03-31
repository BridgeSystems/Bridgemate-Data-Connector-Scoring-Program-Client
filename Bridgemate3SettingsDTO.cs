using System.Collections.Generic;
using System.Linq;

namespace BridgeSystems.Bridgemate.DataConnectorClasses.SharedDTO
{
    /// <summary>
    /// Specifies how the number of tricks obtained must be entered.
    /// </summary>
    public enum Bm3ResultEntryMethod
    {
        /// <summary>
        /// Enter the number of tricks obtained relative to the contract level.
        /// </summary>
        UpDownTricks = 0,

        /// <summary>
        /// Enter the total number of tricks obtained.
        /// </summary>
        NumberOfTricks,

        /// <summary>
        /// Enter the number of tricks obtained American style.
        /// </summary>
        AmericanStyle
    }

    /// <summary>
    /// Specifies whether the lead card must be entered and how this will be validated.
    /// </summary>
    public enum Bm3LeadCardEntryOption
    {
        /// <summary>
        /// No lead card entry.
        /// </summary>
        Disabled,

        /// <summary>
        /// The lead card must be entered. It will not be validated against the handrecord.
        /// </summary>
        EnabledNoValidation,

        /// <summary>
        /// The lead card must be entered. It will be validated against the handrecord and it must be changed if validation fails.
        /// </summary>
        EnabledWithValidationMustComply,

        /// <summary>
        /// The lead card must be entered. It will be validated against the handrecord and a warning is issued if validation fails.
        /// </summary>
        EnabledWithValidationAllowNonCompliance
    }

    /// <summary>
    /// Specifies wheter and when players can make themselves known.
    /// </summary>
    public enum Bm3PlayerRegistrationOption
    {
        /// <summary>
        /// Player cannot make themselves known by entering their detaiils.
        /// </summary>
        Disabled,

        /// <summary>
        /// Player can make themselves known by entering their details in the first round only.
        /// </summary>
        FirstRoundOnly,

        /// <summary>
        /// Player can make themselves known by entering their details in all rounds.
        /// </summary>
        AllRounds
    }

    /// <summary>
    /// Specifies whether player can make themselves known once the round has started.
    /// </summary>
    public enum Bm3PlayerRegistrationOptionDuringPlay
    {
        /// <summary>
        /// Players can make themselved known at the start of the round, before any result has been entered.
        /// </summary>
        Disabled,

        /// <summary>
        /// Players can make themselves known during any stage of the round.
        /// </summary>
        Enabled
    }

    /// <summary>
    /// Players must identify themselves at the start of the session by entering their playernumber.
    /// </summary>
    public enum Bm3PlayerNumberMandatoryOption
    {
       /// <summary>
       /// Indicates that the feature or functionality is disabled.
       /// </summary>
        Disabled,
        
        /// <summary>
        /// Inidcates that the feature is enabled.
        /// </summary>
        Enabled
    }

    /// <summary>
    /// Specifies whether starting positions should be confirmed by players at start of the session
    /// </summary>
    public enum BM3StartingPositionsOption
    {
        /// <summary>
        /// Players cannot / may no confirm starting positions (NE/SW) at start of the session
        /// </summary>
        Disabled,
        /// <summary>
        /// Players must confirm starting positions (NE/SW) at start of the session
        /// </summary>
        Enabled
    }

    /// <summary>
    /// Specifies how the wind rose player view is displayed
    /// </summary>
    public enum BM3PlayerViewOption
    {
        /// <summary>
        /// players are displayed with north at the top
        /// </summary>
        NorthOnTop,
        /// <summary>
        ///  players are displayed with north at the bottom
        /// </summary>
        NorthOnTheBottom

    }
    /// <summary>
    /// Specifies of the players can call the TD using the Bridgemate.
    /// </summary>
    public enum Bm3TdCallOption
    {
        /// <summary>
        /// The players cannot call the TD using the Bridgemate.
        /// </summary>
        Disabled,

        /// <summary>
        /// The players can call the TD using the Bridgemate.
        /// </summary>
        Enabled
    }

    /// <summary>
    /// Specifies if a not played entry must be confirmed by the TD
    /// </summary>
    public enum Bm3ConfirmNPOption
    {
        /// <summary>
        /// TD must not confirm Not Played
        /// </summary>
        Disabled,
        /// <summary>
        /// TD must confirm Not Played
        /// </summary>
        Enabled
    }

    /// <summary>
    /// Specifies whether and when the ranking can be consulted. 
    /// </summary>
    public enum Bm3RankingDisplayOption
    {
        /// <summary>
        /// The ranking cannot be consulted.
        /// </summary>
        Disabled,

        /// <summary>
        /// The ranking can be consulted at the end of each round.
        /// </summary>
        EndOfEachRound,

        /// <summary>
        /// The ranking can be consulted at the end of the last round.
        /// </summary>
        EndOfSession,

        /// <summary>
        /// The ranking can be consulted at any moment.
        /// </summary>
        Always
    }

    /// <summary>
    /// Specifies whether the result for a board can be changed after both pairs have confirmed it.
    /// </summary>
    public enum Bm3ResultReentryOption
    {
        /// <summary>
        /// The result for a board cannot be changed after both pairs have confirmed it.
        /// </summary>
        Disabled,

        /// <summary>
        /// The result for a board can be changed after both pairs have confirmed it.
        /// </summary>
        Enabled
    }

    /// <summary>
    /// Specifies whether the tables for the next round should be displayed after the current round ends.
    /// </summary>
    public enum Bm3NextRoundSeatingOption
    {
        /// <summary>
        /// Do not display the tables for the next round.
        /// </summary>
        Disabled,

        /// <summary>
        /// Display the tables for the next round after the current round ends.
        /// </summary>
        Enabled
    }

    /// <summary>
    /// Specifies wheter to include the own score for a board in the result overview.
    /// </summary>
    public enum Bm3ViewPercentagesOption
    {
        /// <summary>
        /// Do not include the own score for a board in the result overview.
        /// </summary>
        Disabled,

        /// <summary>
        /// Include the own score for a board in the result overview.
        /// </summary>
        Enabled
    }

    /// <summary>
    /// Specifies if all the results obtained in the round can be consulted.
    /// </summary>
    public enum Bm3ScoreRecapOption
    {
        /// <summary>
        /// The results obtained in the round cannot be consulted.
        /// </summary>
        Disabled,

        /// <summary>
        /// The results obtained in the round can be consulted at the end of the round only.
        /// </summary>
        EnabledAtEndOfRoundOnly,

        /// <summary>
        /// The results obtained in the round can be consulted at any time.
        /// </summary>
        Always
    }

    /// <summary>
    /// Specifies if other results on the board can be viewed.
    /// </summary>
    public enum Bm3ViewResultsOption
    {
        /// <summary>
        /// Other results on the board cannot be viewed.
        /// </summary>
        Disabled,

        /// <summary>
        /// Only the own result can be viewed.
        /// </summary>
        OwnResultOnly,

        /// <summary>
        /// Other results, including the own results, on the board can be viewed.
        /// </summary>
        OwnResultAndOtherResults
    }

    /// <summary>
    /// Specifies whether session summaries are enabled or disabled.
    /// </summary>
    public enum Bm3SessionSummaryOption
    {
        /// <summary>
        /// Indicates that the feature or functionality is disabled.
        /// </summary>

        /// <summary>
        /// Indicates that the feature or functionality is enabled.
        /// </summary>
        Enabled
    }

    /// <summary>
    /// Specifies if there should be one column (for NS only) or two (for both NS and EW).
    /// </summary>
    public enum Bm3ScoreColumnOption
    {
        /// <summary>
        /// Show one column, for NS.
        /// </summary>
        NorthSouthOnly,

        /// <summary>
        /// Shpw two columns, for NS and EW.
        /// </summary>
        NorthSouthAndEastWest
    }

    /// <summary>
    /// Specifies which language should be used on the Bridgemate.
    /// </summary>
    public enum Bm3LanguageOption
    {
        /// <summary>
        /// English
        /// </summary>
        English,

        /// <summary>
        /// Dutch
        /// </summary>
        Dutch,

        /// <summary>
        /// French
        /// </summary>
        French,

        /// <summary>
        /// German
        /// </summary>
        German,

        /// <summary>
        /// Italian
        /// </summary>
        Italian,

        /// <summary>
        /// Danish
        /// </summary>
        Danish,

        /// <summary>
        /// Spanish
        /// </summary>
        Spanish,

        /// <summary>
        /// Norwegian
        /// </summary>
        Norwegian,

        /// <summary>
        /// Polish
        /// </summary>
        Polish,

        /// <summary>
        /// Swedish
        /// </summary>
        Swedish,

        /// <summary>
        /// Turkish
        /// </summary>
        Turkish

    }

    /// <summary>
    /// Specifies whether players can consult the handrecord for a board they just played.
    /// </summary>
    public enum Bm3ViewHandrecordOption
    {
        /// <summary>
        /// Handrecords cannot be consulted.
        /// </summary>
        Disabled, 

        /// <summary>
        /// Handrecords can be consulted after play of the board.
        /// </summary>
        Enabled
    }

    /// <summary>
    /// Specifies whether and when handrecords can be entered by the players.
    /// </summary>
    public enum Bm3EnterHandrecordOption
    {
        /// <summary>
        /// Handrecords cannot be entered by the players.
        /// </summary>
        Disabled,

        /// <summary>
        /// Handrecords can be entered after play of the board.
        /// </summary>
        AtEndOfBoard,

        /// <summary>
        /// Handrecords can be entered at the end of the round.
        /// </summary>
        AtEndOfRound,

    }

    /// <summary>
    /// Specifies whether the Bridgemates for the section will work in online mode (continuous communication with the Bridgemate 3 server) 
    /// or if they work in offline mode (communication only at the start and the end of the session).
    /// </summary>
    public enum Bm3ConnectionModeOption
    {
        /// <summary>
        /// The Bridgemates for the section will work in online mode (continuous communication with the Bridgemate 3 server). 
        /// </summary>
        Online,

        /// <summary>
        /// The Bridgemates for the section will work in offline mode (communication will be initiated at the end of the session or can be initiated manually).
        /// </summary>
        Offline
    }

    /// <summary>
    /// Contains all settings for the Bridgemate 3 for a section.
    /// Mind that all values must be supplied.
    /// </summary>
    public class Bridgemate3SettingsDTO : BridgemateSettingsDTO
    {
        /// <summary>
        /// Valid values 1-7. Defines the screen brightness.
        /// </summary>
        public byte BM3ScreenBrightness
        {
            get; set;
        }

        /// <summary>
        /// Valid values 0-15. Defines the number of seconds(times 5) before the screen of the Bridgemate will dim.So the default value of 2 amounts to 10 seconds.
        /// </summary>
        public byte BM3ScreenDimMode
        {
            get; set;
        }

        /// <summary>
        /// Valid values 0-15. Defines the number of seconds (times 5) before the screen of the Bridgemate will turn off. 
        /// A value of zero, the default, means "never turn off".
        /// </summary>
        public byte BM3ScreenOffMode
        {
            get; set;
        }

        /// <summary>
        /// Valid values 0-120. Defines the number of seconds (times 5) before the Bridgemate will enter Sleepmode. 
        /// Once the Bridgemate has entered sleepmode its powerbutton must be used to wake it up again.
        /// </summary>
        public byte BM3SleepMode
        {
            get; set;
        }

        /// <summary>
        /// Valid values 0-7. Currently not in use.
        /// </summary>
        public byte BM3AudioVolume
        {
            get; set;
        }


        /// <summary>
        /// Specifies which language the Bridgemate will use. The default is zero for English.
        /// </summary>
        public Bm3LanguageOption BM3Language
        {
            get; set;
        }

        /// <summary>
        /// Must be four digits. The code the TD must enter to get access to the TD menu. Defaults to "0000".
        /// </summary>
        public string BM3PINcode
        {
            get; set;
        }

        /// <summary>
        /// Specifies if the TD must confirm a "Not Played". Default Off
        /// </summary>
        public Bm3ConfirmNPOption BM3ConfirmNP
        {
            get;set;
        }

        /// <summary>
        /// Specifies if players can call for the Tournament Director using the Bridgemate.
        /// </summary>
        public Bm3TdCallOption BM3TDCall
        {
            get; set;
        }

        /// <summary>
        /// Specifies how players can enter the result for a board: Up/Down tricks, total tricks or American style.
        /// </summary>
        public Bm3ResultEntryMethod BM3ResultMethod
        {
            get; set;
        }

        /// <summary>
        /// Specifies if players may reenter the result of the board after the original entry has been confirmed by the opponents.
        /// </summary>
        public Bm3ResultReentryOption BM3BoardResultReentry
        {
            get; set;
        }

        /// <summary>
        /// Specifies if the leadcard must be enterd together with the contract and if the leadcard should be validated againts the handrecord.
        /// </summary>
        public Bm3LeadCardEntryOption BM3LeadCardEntry
        {
            get; set;
        }

        /// <summary>
        /// Specifies if and when the players can make themselves known by entering their playernumber and/or name on the Bridgemate.
        /// </summary>
        public Bm3PlayerRegistrationOption BM3PlayerNumberEntry
        {
            get; set;
        }

        /// <summary>
        /// Specifies if and when the players can make themselves known by entering their playernumber and/or name on the Bridgemate while the round is in progress (i.e.: after a board has been entered).
        /// </summary>
        public Bm3PlayerRegistrationOptionDuringPlay BM3PlayerNumberEntryDuringPlay
        {
            get; set;
        }

        /// <summary>
        /// Specifies if players must identify themselves at the start of the session by entering their playernumber.
        /// </summary>
        public Bm3PlayerNumberMandatoryOption BM3PlayerNumberMandatory
        {
            get; set;
        }

        /// <summary>
        /// Specifies whether starting positions should be confirmed by players at start of the session
        /// </summary>
        public BM3StartingPositionsOption BM3StartingPositions
        {
            get;set;
        }

        /// <summary>
        ///  Specifies how the wind rose player view is displayed
        /// </summary>
        public BM3PlayerViewOption BM3PlayerView
        {
            get;set;
        }

        /// <summary>
        /// Specifies if and when the Bridgemate will show the current ranking for the players on the table.
        /// </summary>
        public Bm3RankingDisplayOption BM3Ranking
        {
            get; set;
        }

        /// <summary>
        /// Specifes if the seatings for the next round must be displayed after the last result has been entered for the round.
        /// </summary>
        public Bm3NextRoundSeatingOption BM3NextSeatings
        {
            get; set;
        }

        /// <summary>
        /// Specifies if and when the Bridgemate will show a recap of the scores obtained on the table in the current round.
        /// </summary>
        public Bm3ScoreRecapOption BM3ScoreRecap
        {
            get; set;
        }

        /// <summary>
        /// Specifies if the Bridgemate will show the other results for the board that was just entered.
        /// </summary>
        public Bm3ViewResultsOption BM3ViewResults
        {
            get; set;
        }

        /// <summary>
        /// Specifies if the Bridgemate will show a session summary with all the scores obtained in the session once the session has ended.
        /// </summary>
        public Bm3SessionSummaryOption BM3SessionSummary
        {
            get; set;
        }

        /// <summary>
        /// Specifies if the score columns should be for NS only (one column) or for NS and EW (two columns)
        /// </summary>
        public Bm3ScoreColumnOption BM3ScoreColum { get; set; }
       

        /// <summary>
        /// Specifies if the own percentage should be included in the results overview.
        /// </summary>
        public Bm3ViewPercentagesOption BMViewPercentages
        {
            get; set;
        }

        /// <summary>
        /// Specifies if players may view the handrecord after the result has been entered.
        /// </summary>
        public Bm3ViewHandrecordOption BM3ViewHandrecord
        {
            get; set;
        }

        /// <summary>
        /// Specifies if and when the players can enter the handrecord for a board if it is not present.
        /// </summary>
        public Bm3EnterHandrecordOption BM3EnterHandrecord
        {
            get; set;
        }

        /// <summary>
        /// Validates the DTO. Produces validation messages if there are problems. 
        /// </summary>
        /// <returns>True if there are no validation errors.</returns>
        public override bool Validate()
        {         
            base.Validate();
            var validationMessages = new List<string>(ValidationMessages);
            if (BM3ScreenDimMode < 0 || BM3ScreenDimMode > 15)
                validationMessages.Add($"Invalid {nameof(BM3ScreenDimMode)} ({BM3ScreenDimMode}). Value must be between 0 and 15");
            if (BM3ScreenBrightness < 1 || BM3ScreenBrightness > 7)
                validationMessages.Add($"Invalid {nameof(BM3ScreenBrightness)} ({BM3ScreenBrightness}). Value must be between 1 and 7");
            if (BM3SleepMode < 0 || BM3SleepMode > 120)
                validationMessages.Add($"Invalid {nameof(BM3SleepMode)} ({BM3SleepMode}). Value must be between 0 and 120");
            if (BM3AudioVolume < 0 || BM3AudioVolume > 7)
                validationMessages.Add($"Invalid {nameof(BM3AudioVolume)} ({BM3AudioVolume}). Value must be between 0 and 7");
            if (string.IsNullOrWhiteSpace(BM3PINcode) || BM3PINcode.Length != 4 || !int.TryParse(BM3PINcode, out _))
                validationMessages.Add($"Invalid {nameof(BM3PINcode)} ('{BM3PINcode}'). The pincode must be four digits.");
;

            ValidationMessages = validationMessages.ToArray();
            return !validationMessages.Any();
        }
    }
}

