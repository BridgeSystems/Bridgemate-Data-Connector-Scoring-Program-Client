# SectionUpdateDTO

![Image](<lib/SectionUpdateDTO.png>)

The SectionUpdateDTO contains the movement for an updated or new section or it signals that the given section must be deleted. It is sent with the [UpdateMovement](<Overviewofcommunication.md#OverviewOfCommands>) command. The DTO must contain infomration on the scoring group it should be added to. If the scoring group with the given ScoringGroupNumber is not yet present, it will be created.

##### SessionGuid property

Required. A guid uniquely defining the session. Must be exactly 32 character long, uppercase and cannot contaim dashes or curly braces.

##### AddedPlayers

Optional.An array of [PlayerDataDTOs](<PlayerDataDTO.md>). Should at least contain player data for players that have not been added beofre.

##### Participations

Optional. An array of [ParticipationDTOs](<ParticipationDTO.md>).&nbsp;

##### ScoringGroupNumber

Required if IsDeleted is "False". The number of the ScoringGroupDTO that the section belongs to. Must be greater than zero.If the scoringgroup does not yet exist it will be created.

##### ScoringGroupScoringMethod

Required if the updated section belongs to a new scoring group that must be created. Valid values can be found at the information on the [ScoringGroupDTO](<ScoringGroupDTO.md#ScoringMethod>).

##### Letters property

Required. Uniquely defines the section within [the event](<Explanationofusedterms.md>).

##### Winners property

Required. Values can be 1 or 2. In the case of 2 winners the pair numbers in the section can be the same for North-South and East-West. Otherwise the pairnumbers in the section must be unique.

##### GameType property

Required. Values can be 10 for "Pairs", 20 for "Individual" and 30 for "Teams".

##### Name property

Optional.

##### EWMoveBeforePlay property

Currently not supported.

##### MissingPair property

Optional. If specified it will indicate the number for the pair that is not playing. Its opponents will have a sit-out when they are scheduled to play against this pair. This value can&nbsp; be omitted as a sit-out can also be specified on the RoundDTOs. However, if used the graphic representation of sit-out tables in BCS will be improved.

**Note:** For a two winner section a positive number denotes the missing NS pair, a negative value denotes the missing EW pair.

##### IsCombiSection property

Optional. If "true" the section will host the two pairs that would have otherwise have a sit-out in their own sections. Specify the section that will provide the NorthSouth pair and the section that will provide the EastWest pair.

##### NorthSouthPairSectionLetters property

Required if IsCombiSection is "true". The letters for the section where the NorthSouth pair for each round comes from.

##### EastWestPairSectionLetters property

Required if IsCombiSection is "true". The letters for the section where the EastWest pair for each round comes from.

##### Tables property

Must be present if IsDeleted="False" as the SectionDTO contains the movement or a movement update for the section,

Array of [TableDTO](<TableDTO.md>).

