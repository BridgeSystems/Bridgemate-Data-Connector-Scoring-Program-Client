# SectionDTO

![Image](<lib/SectionDTO.png>)

&nbsp;

The SectionDTO contains the movement for a group of participants for the duration of the session. It will be part of a [ScoringGroupDTO](<ScoringGroupDTO.md>) which is part of a [SessionDTO](<SessionDTO.md>) which is part of an [InitDTO.](<InitDTO.md>)

##### SessionGuid property

Required. A guid uniquely defining the session. Must be exactly 32 character long, uppercase and cannot contaim dashes or curly braces.

##### ScoringGroupNumber

Required. The number of the ScoringGroupDTO that the section belongs to. Must be greater than zero.

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

Must be present as the SectionDTO contains the movement or a movement update for the section,

Array of [TableDTO](<TableDTO.md>).

