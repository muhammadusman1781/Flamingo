# Leaderboard Screen Implementation Guide

## Overview
This implementation displays a leaderboard showing the top 10 players with their names and ranks, fetched from the API.

## Files Modified

### 1. Models.cs
Added new models for leaderboard data:
- `LeaderboardResponse` - API response wrapper
- `LeaderboardData` - Contains top_three and remaining players
- `TopThreePlayer` - Player data for top 3 (includes border_type)
- `RemainingPlayer` - Player data for ranks 4-10 (includes coins)

### 2. LeaderboardScreen.cs
Displays leaderboard with:
- Player names for ranks 1-10
- RTL support for all text fields
- API integration to fetch leaderboard data
- Back button to return to previous screen
- Loading panel support

## Unity Setup Instructions

### LeaderboardScreen Setup

1. **Create UI Hierarchy:**
   ```
   LeaderboardScreen (GameObject with LeaderboardScreen.cs)
   ├── Rank1PlayerNameText (RTLTextMeshPro)
   ├── Rank2PlayerNameText (RTLTextMeshPro)
   ├── Rank3PlayerNameText (RTLTextMeshPro)
   ├── Rank4PlayerNameText (RTLTextMeshPro)
   ├── Rank5PlayerNameText (RTLTextMeshPro)
   ├── Rank6PlayerNameText (RTLTextMeshPro)
   ├── Rank7PlayerNameText (RTLTextMeshPro)
   ├── Rank8PlayerNameText (RTLTextMeshPro)
   ├── Rank9PlayerNameText (RTLTextMeshPro)
   ├── Rank10PlayerNameText (RTLTextMeshPro)
   ├── BackButton (Button)
   └── LoadingPanel (GameObject - optional)
   ```

2. **Assign References in Inspector:**
   - Assign all 10 RTLTextMeshPro components to their respective rank text fields
   - Assign the BackButton
   - Optionally assign a LoadingPanel GameObject

## API Integration

### Endpoint Used
- **URL:** `{baseUrl}/auth/leaderboard/`
- **Method:** GET
- **Authentication:** Token required

### Response Structure
```json
{
    "status": "success",
    "message": "Leaderboard fetched successfully.",
    "data": {
        "top_three": [
            {
                "profile_picture": null,
                "player_name": "Usman Hanif",
                "rank": 1,
                "border_type": "gold"
            },
            {
                "profile_picture": null,
                "player_name": "Usman THV",
                "rank": 2,
                "border_type": "silver"
            },
            {
                "profile_picture": null,
                "player_name": "meharabrehman g",
                "rank": 3,
                "border_type": "gold"
            }
        ],
        "remaining": [
            {
                "profile_picture": null,
                "player_name": "Xynova x studios",
                "coins": 50,
                "rank": 4
            },
            {
                "profile_picture": null,
                "player_name": "ahmad@gmail.com",
                "coins": 0,
                "rank": 5
            }
            // ... ranks 6-10
        ]
    }
}
```

## Features

### Current Implementation
- ✅ Displays player names for ranks 1-10
- ✅ RTL support using RTLTextMeshPro
- ✅ API integration with proper error handling
- ✅ Combines top_three and remaining arrays into unified list
- ✅ Sorts players by rank to ensure correct order
- ✅ Shows "-" for missing ranks
- ✅ Loading panel support
- ✅ Back button functionality

### Future Enhancements (Optional)
- Profile pictures display
- Border types for top 3 players (gold/silver)
- Coins display for ranks 4-10
- Highlight current player's position
- Refresh button to reload leaderboard
- Pull-to-refresh functionality

## How It Works

1. **Data Loading:**
   - Screen automatically loads leaderboard data on Start()
   - API call is made using NetworkingHandler.getMessage()
   - Loading panel is shown during API call

2. **Data Processing:**
   - Top 3 and remaining players are combined into a single list
   - List is sorted by rank to ensure correct order
   - Each rank (1-10) is populated with the corresponding player name

3. **UI Updates:**
   - If a player exists for a rank, their name is displayed
   - If no player exists for a rank, "-" is displayed
   - All text uses RTLTextMeshPro for Arabic support

## RTL Support
All player name text fields use `RTLTextMeshPro` components from the RTLTMPro library to support Arabic text rendering.

## Testing
1. Ensure NetworkingHandler instance exists in scene
2. Ensure ServerConstants has valid `baseUrl`
3. Ensure user token is set in `serverConstants.UserProfileData.token`
4. Open LeaderboardScreen - it will automatically load data on Start()

## UI Layout Suggestions

### Simple Layout
```
╔════════════════════════════════════╗
║         Leaderboard                ║
║                                    ║
║  1. [Player Name]                  ║
║  2. [Player Name]                  ║
║  3. [Player Name]                  ║
║  4. [Player Name]                  ║
║  5. [Player Name]                  ║
║  6. [Player Name]                  ║
║  7. [Player Name]                  ║
║  8. [Player Name]                  ║
║  9. [Player Name]                  ║
║ 10. [Player Name]                  ║
║                                    ║
║        [Back Button]               ║
╚════════════════════════════════════╝
```

### Enhanced Layout (for future)
```
╔════════════════════════════════════╗
║         Leaderboard                ║
║                                    ║
║  🥇 1. [Name] [Border]             ║
║  🥈 2. [Name] [Border]             ║
║  🥉 3. [Name] [Border]             ║
║                                    ║
║  4. [Name]           50 coins      ║
║  5. [Name]            0 coins      ║
║  6. [Name]            0 coins      ║
║  7. [Name]            0 coins      ║
║  8. [Name]            0 coins      ║
║  9. [Name]            0 coins      ║
║ 10. [Name]            0 coins      ║
║                                    ║
║        [Back Button]               ║
╚════════════════════════════════════╝
```

## Notes
- Loading panel is optional but recommended for better UX
- The screen handles null/missing player data gracefully
- Back button properly hides the screen without destroying it
- All ranks are displayed even if some are missing players
- The code separates top_three and remaining data as per API structure

