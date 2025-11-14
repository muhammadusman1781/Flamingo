using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameModesResponse
{
    public string status;
    public string message;
    public List<GameMode> data;
}

[Serializable]
public class GameMode
{
    public string game_id;
    public int user_feather;
    public string game_name;
    public int price;
    public int entry_price;
    public int xp;
    public int feather;
    public string feather_type;
}

// Join Room Request
[Serializable]
public class JoinRoomRequest
{
    public string game_mode;
    public string room_code; // Optional: for joining private rooms with code
}

// Create Room Request
[Serializable]
public class CreateRoomRequest
{
    public string game_mode;
    public string room_type;
}

// Join Room Response
[Serializable]
public class JoinRoomResponse
{
    public string status;
    public string message;
    public RoomData data;
}

[Serializable]
public class RoomData
{
    public string slug;
    public string game_mode;
    public string game_mode_name;
    public string room_type;
    public string room_code;
    public int level;
    public int player1;
    public string player1_name;
    public int player2; // 0 or negative means no player
    public string player2_name;
    public int player1_score; // 0 or negative means no score
    public int player2_score; // 0 or negative means no score
    public int winner; // 0 or negative means no winner
    public string winner_name;
    public bool is_active;
    public string created_at;
    public string updated_at;
}

// Questions Response
[Serializable]
public class QuestionsResponse
{
    public string status;
    public string message;
    public QuestionsData data;
}

[Serializable]
public class QuestionsData
{
    public string room_slug;
    public int room_level;
    public int user_grade;
    public int total_questions;
    public List<QuestionMultiplayer> questions;
}

[Serializable]
public class QuestionMultiplayer
{
    public int id;
    public int grade;
    public string subject;
    public int level;
    public string question;
    public string option_a;
    public string option_b;
    public string option_c;
    public string option_d;
    public string right_answer;
    public string created_at;
    public string updated_at;
}

// Submit Score Request
[Serializable]
public class SubmitScoreRequest
{
    public int score;
}

// Submit Score Response (generic response)
[Serializable]
public class SubmitScoreResponse
{
    public string status;
    public string message;
}

// Game Results Response
[Serializable]
public class GameResultsResponse
{
    public string status;
    public string message;
    public GameResultsData data;
}

[Serializable]
public class GameResultsData
{
    public string room_slug;
    public int user_id;
    public string user_name;
    public int user_score;
    public int opponent_id;
    public string opponent_name;
    public int opponent_score;
    public int player1_score;
    public int player2_score;
    public bool both_scores_submitted;
    public string result; // 'win', 'lose', 'tie', or null
    public int winner_id;
    public string winner_name;
}

// Users List Response
[Serializable]
public class UsersListResponse
{
    public string status;
    public string message;
    public List<UserData> data;
}

[Serializable]
public class UserData
{
    public int id;
    public string email;
    public string mobile;
    public string first_name;
    public string last_name;
}

// Send Friend Request
[Serializable]
public class SendFriendRequest
{
    public int receiver_id;
}

// Send Friend Request Response
[Serializable]
public class SendFriendRequestResponse
{
    public string status;
    public string message;
    public string data;
}

// Friends List Response
[Serializable]
public class FriendsListResponse
{
    public string status;
    public string message;
    public List<UserData> data;
}

// Friend Requests Response
[Serializable]
public class FriendRequestsResponse
{
    public string status;
    public string message;
    public FriendRequestsData data;
}

[Serializable]
public class FriendRequestsData
{
    public List<FriendRequestData> received_requests;
    public List<FriendRequestData> sent_requests;
}

[Serializable]
public class FriendRequestData
{
    public int id;
    public UserData sender;
    public UserData receiver;
    public string status;
    public string created_at;
    public string updated_at;
}

// Accept/Reject Friend Request Response
[Serializable]
public class FriendRequestActionResponse
{
    public string status;
    public string message;
}