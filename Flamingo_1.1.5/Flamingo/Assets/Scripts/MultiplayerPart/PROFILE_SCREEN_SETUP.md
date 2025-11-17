# Profile Screen Implementation Guide

## Overview
This implementation includes a complete profile screen system with RTL support, feather management, and API integration.

## Files Created/Modified

### 1. Models.cs (Modified)
Added new models for user profile data:
- `UserProfileResponse` - API response wrapper
- `UserProfileData` - User profile data including wins, losses, friends count, and feathers
- `UserFeather` - Individual feather data
- `FeatherPriority` - Helper class for sorting feathers by priority

### 2. ProfileScreen.cs (Modified)
Main profile screen that displays:
- Player name (first_name + last_name)
- Friends count
- Wins count
- Losses count
- Top 3 feathers (sorted by priority)
- "Show More" button to view all feathers

### 3. FeathersScreen.cs (New)
Screen that displays all user feathers:
- Shows all feathers sorted by priority
- Each feather shows its image, name, and count
- Click on any feather to see details
- Back button to return to profile

### 4. FeatherDetailScreen.cs (New)
Detailed view of a single feather:
- Large feather image
- Feather name and count
- Feather description (in Arabic)
- Back button to return to feathers list

## Unity Setup Instructions

### ProfileScreen Setup

1. **Create UI Hierarchy:**
   ```
   ProfileScreen (GameObject with ProfileScreen.cs)
   ├── PlayerNameText (RTLTextMeshPro)
   ├── FriendsCountText (RTLTextMeshPro)
   ├── WinsCountText (RTLTextMeshPro)
   ├── LossesCountText (RTLTextMeshPro)
   ├── FeatherSlot1 (GameObject)
   │   ├── FeatherImage (Image)
   │   └── FeatherCount (RTLTextMeshPro)
   ├── FeatherSlot2 (GameObject)
   │   ├── FeatherImage (Image)
   │   └── FeatherCount (RTLTextMeshPro)
   ├── FeatherSlot3 (GameObject)
   │   ├── FeatherImage (Image)
   │   └── FeatherCount (RTLTextMeshPro)
   ├── ShowMoreFeathersButton (Button)
   └── LoadingPanel (GameObject - optional)
   ```

2. **Assign References in Inspector:**
   - Drag RTLTextMeshPro components to corresponding text fields
   - Assign all 3 feather slot GameObjects to the `featherSlots` array
   - Assign all 3 feather Images to the `featherImages` array
   - Assign all 3 feather count texts to the `featherCountTexts` array
   - Assign the ShowMoreFeathersButton
   - Assign the FeathersScreen GameObject reference

### FeathersScreen Setup

1. **Create UI Hierarchy:**
   ```
   FeathersScreen (GameObject with FeathersScreen.cs)
   ├── ScrollView
   │   └── Viewport
   │       └── FeathersContainer (VerticalLayoutGroup)
   ├── BackButton (Button)
   └── FeatherDetailScreen (GameObject)
   ```

2. **Create Feather Item Prefab:**
   ```
   FeatherItemPrefab (with FeatherItemUI.cs and Button)
   ├── FeatherImage (Image)
   ├── FeatherNameText (RTLTextMeshPro)
   └── FeatherCountText (RTLTextMeshPro)
   ```

3. **Assign References in Inspector:**
   - Set `feathersContainer` to the VerticalLayoutGroup GameObject
   - Create and assign the `featherItemPrefab`
   - Assign the `backButton`
   - Assign the `featherDetailScreen` reference

### FeatherDetailScreen Setup

1. **Create UI Hierarchy:**
   ```
   FeatherDetailScreen (GameObject with FeatherDetailScreen.cs)
   ├── FeatherImage (Image - large display)
   ├── FeatherNameText (RTLTextMeshPro)
   ├── FeatherDescriptionText (RTLTextMeshPro)
   ├── FeatherCountText (RTLTextMeshPro)
   └── BackButton (Button)
   ```

2. **Assign References in Inspector:**
   - Assign all UI components to their respective fields
   - Assign the backButton

## API Integration

### Endpoint Used
- **URL:** `{baseUrl}/auth/user/`
- **Method:** GET
- **Authentication:** Token required

### Response Structure
```json
{
    "status": "success",
    "message": "User info fetched successfully.",
    "data": {
        "id": 23,
        "first_name": "Usman",
        "last_name": "Hanif",
        "friends_count": 2,
        "win": 7,
        "lose": 0,
        "user_feathers": [
            {
                "id": 1,
                "feather": 1,
                "feather_type": "Silver"
            }
        ]
    }
}
```

## Feather Priority Order (Top to Bottom)
1. Legendary
2. Emerald
3. Ruby
4. Mercury
5. Diamond
6. Titanium
7. Platinum
8. Golden
9. Silver
10. Bronze

## RTL Support
All text fields use `RTLTextMeshPro` components from the RTLTMPro library to support Arabic text rendering.

## Sprite Management

Currently, the `GetFeatherSprite()` methods return `null`. You need to implement sprite loading:

### Option 1: Resources Folder
```csharp
private Sprite GetFeatherSprite(string featherType)
{
    return Resources.Load<Sprite>($"Feathers/{featherType}");
}
```

Place your feather sprites in: `Assets/Resources/Feathers/`
- Name them: `Legendary.png`, `Emerald.png`, etc.

### Option 2: Sprite Dictionary
```csharp
[System.Serializable]
public class FeatherSprite
{
    public string featherType;
    public Sprite sprite;
}

[Header("Feather Sprites")]
public List<FeatherSprite> featherSprites;

private Sprite GetFeatherSprite(string featherType)
{
    var entry = featherSprites.Find(x => x.featherType == featherType);
    return entry?.sprite;
}
```

## Navigation Flow
1. **ProfileScreen** (Shows player info and top 3 feathers)
   ↓ Click "Show More"
2. **FeathersScreen** (Shows all feathers in a scrollable list)
   ↓ Click on any feather
3. **FeatherDetailScreen** (Shows detailed view of selected feather)

## Testing
1. Ensure NetworkingHandler instance exists in scene
2. Ensure ServerConstants has valid `baseUrl`
3. Ensure user token is set in `serverConstants.UserProfileData.token`
4. Open ProfileScreen - it will automatically load user data on Start()

## Notes
- Loading panel is optional but recommended for better UX
- Feather descriptions are in Arabic by default
- All screens handle null references gracefully
- Back buttons properly hide screens without destroying them

