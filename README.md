# EXILEDBattleRoyale

## Config

Use the `Plugins/BattleRoyale/config.yml` file

### battle_rooms_dbg

Type: bool

Function: Output room names in current seed to a file.

Default Value: false

### battle_items_spawned

Type: int

Function: How many items should be spawned.

Default Value: 100

### battle_scp

Type: int

Function: What class (by id) to spawn the SCPs or the "Border" as.

Default Value: 0 or Scp173

### battle_scp_hp

Type: float

Function: How much health the `battle_scp` will spawn with.

Default Value: 1000.0

### battle_classd_hp

Type: float

Function: How much health the Class-Ds will spawn with.

Default Value: 100.0

## Room and Zone Tiers

You can configure where players will spawn and what they will spawn with.

### battle_tiers

Type: String List

Function: What different room tiers there is.

Default Value: LCZ

Notes: When `battle_tier_start_rooms` or `battle_tier_rooms` is not in the config file, this will be the default value.

### battle_tier_rooms

Type: String Dictionary

Function: The random item spawn rooms and their tiers are.

Default Value:

`LCZ_Curve: LCZ`,
`LCZ_Crossing: LCZ`,
`LCZ_TCross: LCZ`,
`LCZ_Airlock: LCZ`,
`LCZ_Straight: LCZ`

### battle_tier_start_rooms

Type: String Dictionary

Function: The Class-D spawn rooms and their tiers are.

Default Value:

`LCZ_Curve: LCZ`,
`LCZ_Crossing: LCZ`,
`LCZ_TCross: LCZ`,
`LCZ_Airlock: LCZ`,
`LCZ_Straight: LCZ`

### battle_tier_`tier_name`

Type: int List

Function: What items (by id) should spawn in the tier `tier_name`.

Default Value: N/A

### battle_tier_start_`tier_name`

Type: int List

Function: What items (by id) should the player spawn with in the tier `tier_name`.

Default Value: N/A


## Example Config

```
battle_rooms_dbg: false

battle_tiers:
    - LCZ
    - HCZ

battle_tier_LCZ:
    - 13
    - 15
    - 2
    - 14
    - 23
    - 28
    - 29
    - 22
battle_tier_HCZ:
    - 14
    - 10
    - 21
    - 23
    - 28
    - 29
    - 22


battle_tier_start_LCZ:
    - 13
    - 0
battle_tier_start_HCZ:
    - 13
    - 3


battle_tier_start_rooms:
    LCZ_Curve: LCZ
    LCZ_Crossing: LCZ
    LCZ_TCross: LCZ
    LCZ_Straight: LCZ
    HCZ_Curve: HCZ
    HCZ_Crossing: HCZ
    HCZ_Room3: HCZ
    HCZ_Straight: HCZ

battle_tier_rooms:
    LCZ_173: LCZ
    LCZ_372: LCZ
    LCZ_012: LCZ
    HCZ_Servers: HCZ
    HCZ_Tesla: HCZ
```