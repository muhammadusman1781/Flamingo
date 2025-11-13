using System;
// AwaisDev: Screen identifiers for centralized UI navigation

public enum ScreenId
{
    None = 0,

    // Core entry/navigation
    Intro = 10,
    Login = 20,
    Signup = 21,
    Otp = 22,
    Home = 30,

    // Game flows
    LetsPlay = 40,
    StageSelection = 41,
    Quiz = 42,
    StudentInfo = 43,
    Challengers = 44,

    // Social/competition
    League = 50,
    TopRank = 51,
    AddFriends = 52,
    ChallengeOtherPlayers = 53,

    // Meta systems
    WheelOfFortune = 60,
    Notifications = 61,
    Profile = 62,
    Rewards = 63,
    Achievements = 64,
    
    // Reward Detail Screens
    BronzeFeatherChile = 80,
    SilverFeatherArgentina = 81,
    GoldenFeatherKenya = 82,
    PlatinumFeatherBolivia = 83,
    TitaniumFeatherIran = 84,
    DiamondFeatherBahamas = 85,
    MercuryFeatherTurkey = 86,
    RubyFeatherFrance = 87,
    EmeraldFeatherSpain = 88,
    LegendaryFeatherIraq = 89,

    // Generic/common
    Category = 70,
    Gameplay = 71,
    Results = 72,
    Leaderboard = 73,
    Settings = 74,
    Shop = 75
}


