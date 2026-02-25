public static class CMD
{
    // ==================== 0-19: LOGIN/LOGOUT/ZONE ====================
    public const int LOGIN_GAME = 0;
    public const int LOGOUT = 2;
    public const int RESPAWN = 14;

    // ==================== 20-29: MOVEMENT ====================
    public const int MOVEMENT_INPUT = 20;
    public const int STOP_COMMAND = 22;
    public const int MOVE_TO_POSITION = 23;

    // ==================== 30-39: BASIC COMBAT ====================
    public const int ATTACK = 30;
    public const int DAMAGE_DEALT = 31;
    public const int DEATH = 32;
    public const int SKILL_CAST = 33;
    public const int TRU_BAN_MINH = 34;
    public const int TRU_BAN_LINH = 35;
    public const int TURRET_SHOT = 36;

    // ==================== 40-49: MATCHMAKING ====================
    public const int FIND_MATCH = 40;
    public const int MATCH_FOUND = 41;
    public const int SELECT_HERO = 42;
    public const int HERO_SELECTED = 43;
    public const int GAME_START = 44;
    public const int GAME_END = 45;
    public const int CANCEL_FIND_MATCH = 46;

    // ==================== 50-59: GAME STATE SNAPSHOTS ====================
    public const int GAME_SNAPSHOT = 50; // players (JsonArray): userId, teamId, x, y, heading, isMoving, isAlive
    public const int MINION_SNAPSHOT = 52; // minions (JsonArray): id, teamId, x, y, laneId
    public const int MONSTER_SNAPSHOT = 53; // monsters (JsonArray): id, campId, x, y

    public const int RESOURCE_SNAPSHOT = 51;
    public const int RESOURCE_MINION_SNAPSHOT = 57; // minions (JsonArray): id, hp, maxHp
    public const int RESOURCE_MONSTER_SNAPSHOT = 58; // monsters (JsonArray): id, campId, hp, maxHp
    public const int RESOURCE_TURRET_SNAPSHOT = 59; // turrets (JsonArray): id, teanId, hp, maxHp

    public const int MINION_SPAWNED = 55;
    public const int MONSTER_SPAWNED = 56;

    // ==================== 60-69: GAME DATA ====================
    public const int GET_DANH_SACH_LOAI_TUONG = 60;
    public const int GET_CHI_SO_TUONG = 61;
    public const int GET_INFO_MO_TA_KN_TUONG = 62;
    public const int GET_ITEM_LIST = 63;
    public const int GET_SPELL_LIST = 64;

    // ==================== 70-80: ROOM/LOBBY ====================
    public const int GET_GAME_MODE_LIST = 70;
    public const int CREATE_ROOM = 71;
    public const int INVITE_TO_ROOM = 72;
    public const int LEAVE_ROOM = 73;
    public const int ROOM_INFO = 74;
    public const int ROOM_INVITE_RECEIVED = 75;
    public const int ACCEPT_INVITE = 76;
    public const int DECLINE_INVITE = 77;
    public const int ROOM_PLAYER_JOINED = 78;
    public const int ROOM_PLAYER_LEFT = 79;
    public const int ROOM_START_GAME = 80;

    // ==================== 81-91: FRIEND ====================
    public const int DATA_FRIEND = 81;
    public const int GUI_YEU_CAU_KET_BAN = 82;
    public const int PUT_LOI_MOI_KET_BAN = 83;
    public const int DONG_Y_KET_BAN = 84;
    public const int TU_CHOI_KET_BAN = 85;
    public const int PUT_TU_CHOI_KET_BAN = 86;
    public const int DELETE_FRIEND = 86;
    public const int FRIEND_ONLINE_STATUS = 87;
    public const int FIND_GOI_Y_KET_BAN = 109;
    public const int FIND_FRIEND = 111;

    // ==================== 92-108: PARTY ====================
    public const int CREATE_PARTY = 92;
    public const int INVITE_TO_PARTY = 93;
    public const int PARTY_INVITE_RECEIVED = 94;
    public const int ACCEPT_PARTY_INVITE = 95;
    public const int DECLINE_PARTY_INVITE = 96;
    public const int LEAVE_PARTY = 97;
    public const int PARTY_INFO = 98;
    public const int PARTY_PLAYER_JOINED = 99;
    public const int PARTY_PLAYER_LEFT = 100;

    public const int PARTY_FIND_MATCH = 101;
    public const int PARTY_CANCEL_FIND = 102;
    public const int PARTY_MATCH_FOUND = 103;
    public const int PARTY_ACCEPT_MATCH = 104;
    public const int PARTY_DECLINE_MATCH = 105;

    public const int PARTY_MATCH_READY = 106;
    public const int PARTY_MATCH_CANCELLED = 107;
    public const int JOIN_PARTY_MATCH_AGAIN = 108;

    // ==================== 109-126: MISC ====================
    public const int NOTIFY_DATA_GAME = 112;
    public const int PUT_NOTIFY_GAME = 113;
    public const int CHAT_THE_GIOI = 120;
    public const int CHAT_THE_GIOI_DATA = 121;
    public const int CHAT_THE_GIOI_HISTORY = 127;
    public const int CHAT_THE_GIOI_HISTORY_DATA = 128;
    public const int CHAT_FRIEND = 122;
    public const int CHAT_FRIEND_DATA = 129; // nhận về: fromUserId, fromDisplayName, fromLevel, fromAvatarId, content, timestamps
    public const int READ_CHAT_FRIEND = 124;

    public const int CHAT_IN_MATCH = 125;
    public const int CHAT_IN_MATCH_DATA = 126;

    public const int VOICE_CHANNEL_JOIN = 140;
    public const int VOICE_CHANNEL_LEAVE = 141;

    // ==================== 200-209: IN-MATCH GOLD ====================
    public const int GOLD_REWARD = 200;
    public const int GOLD_SPEND = 201;
    public const int GOLD_UPDATE = 202;

    // ==================== 210-219: IN-MATCH EXP & LEVEL ====================
    public const int EXP_GAIN = 210;
    public const int LEVEL_UP = 211;

    // ==================== 220-229: IN-MATCH MANA ====================
    public const int NO_MANA = 220;
    public const int MANA_UPDATE = 221;

    // ==================== 230-239: IN-MATCH HP ====================
    public const int HEAL = 230;
    public const int HP_UPDATE = 231;

    // ==================== 250-269: IN-MATCH SKILLS ====================
    public const int SKILL_UPGRADE = 250;
    public const int SKILL_COOLDOWN = 251;
    public const int SKILL_READY = 252;

    // ==================== 270-279: IN-MATCH BUFFS ====================
    public const int BUFF_APPLIED = 270;
    public const int BUFF_REMOVED = 271;
    public const int SHIELD_APPLIED = 272;
    public const int SHIELD_BROKEN = 273;

    // ==================== 280-299: IN-MATCH DEBUFFS ====================
    public const int DEBUFF_APPLIED = 280;
    public const int DEBUFF_REMOVED = 281;
    public const int CC_STUN = 282;
    public const int CC_SLOW = 283;
    public const int CC_KNOCKUP = 284;
    public const int CC_SILENCE = 285;

    public const int BUY_ITEM = 301;
    public const int SELL_ITEM = 302;
    public const int GET_ITEM_INFO = 303;

    public const int GET_EVENT_INFO = 401;

    // ==================== SPECIAL ====================
    public const int UDP_HANDSHAKE = 999;
    public const int ERROR = 99;
}