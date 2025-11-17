# Lobby Screen Back Button Implementation

## Overview
Added a back button to the LobbyScreen that deletes the room when the user wants to leave the lobby.

## Changes Made

### 1. Models.cs
Added `DeleteRoomResponse` model:
```csharp
public class DeleteRoomResponse
{
    public string status;
    public string message;
}
```

### 2. LobbyScreen.cs
Added the following functionality:
- **Back Button** - New button reference in Inspector
- **Delete Room API Call** - Calls the delete endpoint before going back
- **Cleanup** - Stops polling before deleting the room

## API Integration

### Endpoint Used
- **URL:** `{baseUrl}/multiplayer/rooms/{slug}/delete/`
- **Method:** POST
- **Authentication:** Token required
- **Body:** Empty string

### Expected Response
```json
{
    "status": "success",
    "message": "Room deleted successfully."
}
```

## Unity Setup

### Inspector Setup
1. Add a Button to your LobbyScreen UI hierarchy
2. Assign this button to the `backButton` field in the LobbyScreen component

### UI Hierarchy Example
```
LobbyScreen (GameObject with LobbyScreen.cs)
├── ... (existing UI elements)
├── BackButton (Button) <- NEW
└── ...
```

## How It Works

1. **User Clicks Back Button**
   - `OnBackButtonClick()` is called

2. **Stop Polling**
   - `StopPolling()` is called to stop checking for second player

3. **Delete Room**
   - `DeleteRoom()` makes API call to delete the room
   - Uses room slug from `CurrentRoomData.slug`

4. **Handle Response**
   - **Success:** Logs deletion and navigates back
   - **Fail:** Logs error but still navigates back (room might already be deleted)

5. **Go Back**
   - `GoBackToPreviousScreen()` hides the lobby screen
   - You can customize this to activate a specific previous screen

## Important Notes

### Room Slug
The room slug is automatically extracted from the current room data:
```csharp
string roomSlug = CurrentRoomData.slug;
string apiUrl = serverConstants.baseUrl + $"/multiplayer/rooms/{roomSlug}/delete/";
```

### Error Handling
- If ServerConstants is null, goes back without API call
- If CurrentRoomData is null, goes back without API call
- If delete API fails, still goes back (graceful degradation)

### Navigation
The current implementation just hides the lobby screen:
```csharp
private void GoBackToPreviousScreen()
{
    gameObject.SetActive(false);
}
```

You can customize this to activate a specific screen:
```csharp
private void GoBackToPreviousScreen()
{
    gameObject.SetActive(false);
    
    // Activate the screen you want to go back to
    if (roomSelectionScreen != null)
    {
        roomSelectionScreen.SetActive(true);
    }
}
```

## Flow Diagram

```
User clicks Back Button
        ↓
Stop Polling for Players
        ↓
Call Delete Room API
        ↓
    ┌───────┴───────┐
    ↓               ↓
Success          Fail
    ↓               ↓
Log Success    Log Error
    ↓               ↓
    └───────┬───────┘
            ↓
   Go Back to Previous Screen
```

## Testing

1. Join or create a room to enter LobbyScreen
2. Click the back button
3. Check console logs for:
   - "Deleting room: [slug]"
   - "Room deleted successfully" or error message
4. Verify the screen navigates back
5. Optional: Try rejoining the same room to verify it was deleted

## Example Console Output

### Successful Deletion
```
Deleting room: abc123xyz
API URL: https://your-api.com/multiplayer/rooms/abc123xyz/delete/
HTTP 200 POST /multiplayer/rooms/abc123xyz/delete/ | Body: {"status":"success","message":"Room deleted successfully."}
Room deleted successfully: {"status":"success","message":"Room deleted successfully."}
Delete status: success
Delete message: Room deleted successfully.
Stopped polling for second player
```

### Failed Deletion (but still navigates back)
```
Deleting room: abc123xyz
API URL: https://your-api.com/multiplayer/rooms/abc123xyz/delete/
HTTP 404 POST /multiplayer/rooms/abc123xyz/delete/ -> Not Found
Failed to delete room: {"error":"Room not found"}
Stopped polling for second player
```

## Integration with Existing Code

The back button functionality integrates seamlessly with the existing LobbyScreen:
- Uses the same `serverConstants` and `NetworkingHandler`
- Respects the existing room data structure
- Stops polling automatically
- Uses consistent error handling patterns

No changes are required to other parts of the code!

