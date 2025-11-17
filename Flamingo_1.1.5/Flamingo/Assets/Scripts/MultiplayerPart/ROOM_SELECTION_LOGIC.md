# Room Selection Logic - Fixed Flow

## Overview
Fixed the room selection logic to properly handle different scenarios for joining/creating rooms based on the `isPrivateMatch` flag and room code input.

## Decision Flow

The logic now follows this priority order:

```
1. Check if isPrivateMatch == true
   ├─ YES → Call /multiplayer/rooms/create/
   │         Body: { "game_mode": "...", "room_type": "friends" }
   │
   └─ NO → Check if room_code input is NOT empty
           ├─ YES → Call /multiplayer/rooms/join/
           │         Body: { "game_mode": "...", "room_code": "..." }
           │
           └─ NO → Call /multiplayer/rooms/join/
                    Body: { "game_mode": "..." }
```

## Three Scenarios

### Scenario 1: Private Match Mode (isPrivateMatch = true)
**Action:** Create a private room  
**API Endpoint:** `/multiplayer/rooms/create/`  
**Request Body:**
```json
{
    "game_mode": "2a330f51-737b-4bf5-b3bd-ee5a16419202",
    "room_type": "friends"
}
```
**Note:** Room code input is ignored in this mode

### Scenario 2: Room Code Provided (isPrivateMatch = false, room code NOT empty)
**Action:** Join a specific room using code  
**API Endpoint:** `/multiplayer/rooms/join/`  
**Request Body:**
```json
{
    "game_mode": "2a330f51-737b-4bf5-b3bd-ee5a16419202",
    "room_code": "WPFAWG"
}
```

### Scenario 3: Public Match (isPrivateMatch = false, room code empty)
**Action:** Join any available public room  
**API Endpoint:** `/multiplayer/rooms/join/`  
**Request Body:**
```json
{
    "game_mode": "2a330f51-737b-4bf5-b3bd-ee5a16419202"
}
```

## Code Changes

### Updated Method: `OnRoomButtonClicked()`

**Before:**
```csharp
if (!string.IsNullOrEmpty(roomCode))
{
    JoinRoomWithCode(matchedGameMode.game_id, roomCode);
}
else if (isPrivateMatch)
{
    CreatePrivateRoom(matchedGameMode.game_id);
}
else
{
    JoinRoom(matchedGameMode.game_id);
}
```

**After:**
```csharp
// Priority 1: If private match mode is enabled, create a private room
if (isPrivateMatch)
{
    CreatePrivateRoom(matchedGameMode.game_id);
}
// Priority 2: If room code is provided, join room with code
else if (!string.IsNullOrEmpty(roomCode))
{
    JoinRoomWithCode(matchedGameMode.game_id, roomCode);
}
// Priority 3: Default - join public room
else
{
    JoinRoom(matchedGameMode.game_id);
}
```

## Key Differences

1. **Priority Order Changed:**
   - OLD: Room code check → Private match → Public
   - NEW: Private match → Room code → Public

2. **Private Match Takes Precedence:**
   - If `isPrivateMatch = true`, always create a private room
   - Room code input is ignored when in private match mode

3. **Clearer Logic:**
   - The if-else chain now follows a clear priority order
   - Each scenario is well-documented with comments

## Example Use Cases

### Use Case 1: Creating a Private Room to Play with Friends
```
User Action: Toggle "Private Match" mode
Result: isPrivateMatch = true
Flow: Click room → CreatePrivateRoom() → Create API
```

### Use Case 2: Joining Friend's Private Room
```
User Action: Enter room code "WPFAWG", keep "Public Match" mode
Result: isPrivateMatch = false, roomCode = "WPFAWG"
Flow: Click room → JoinRoomWithCode() → Join API with code
```

### Use Case 3: Quick Match (Public Room)
```
User Action: Keep "Public Match" mode, no room code
Result: isPrivateMatch = false, roomCode = ""
Flow: Click room → JoinRoom() → Join API without code
```

## Related Methods

### JoinRoom(gameModeId)
- Joins a public room
- Request body: `{ "game_mode": "..." }`

### JoinRoomWithCode(gameModeId, roomCode)
- Joins a specific private room using code
- Request body: `{ "game_mode": "...", "room_code": "..." }`

### CreatePrivateRoom(gameModeId)
- Creates a new private room
- Request body: `{ "game_mode": "...", "room_type": "friends" }`

## Testing Checklist

- [ ] Test creating a private room (isPrivateMatch = true)
- [ ] Test joining with a valid room code
- [ ] Test joining with an invalid room code
- [ ] Test joining a public room (no code, public mode)
- [ ] Verify room code is ignored when isPrivateMatch = true
- [ ] Verify empty/whitespace room codes are treated as empty

## Debug Logging

The code includes comprehensive logging for debugging:
- "Private match mode enabled. Creating private room..."
- "Room code provided: {code}. Joining room with code..."
- "No room code and public match mode. Joining public room..."

Check console logs to verify which path is being taken.

