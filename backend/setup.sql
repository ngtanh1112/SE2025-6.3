-- Game Backend Database Schema
-- Create database
CREATE DATABASE IF NOT EXISTS game_backend CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE game_backend;

-- 1. Players Table
CREATE TABLE IF NOT EXISTS players (
    id INT AUTO_INCREMENT PRIMARY KEY,
    player_id VARCHAR(100) UNIQUE NOT NULL,
    install_id VARCHAR(100),
    app_name VARCHAR(50),
    country VARCHAR(10),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_active TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_player_id (player_id),
    INDEX idx_install_id (install_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 2. Leaderboard Table
CREATE TABLE IF NOT EXISTS leaderboard (
    id INT AUTO_INCREMENT PRIMARY KEY,
    player_id VARCHAR(100) NOT NULL,
    app_name VARCHAR(50) NOT NULL,
    score BIGINT DEFAULT 0,
    country VARCHAR(10),
    rank INT DEFAULT 0,
    level_version INT DEFAULT 0,
    player_name VARCHAR(100),
    player_flag VARCHAR(50),
    image_url TEXT,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UNIQUE KEY unique_player_app (player_id, app_name),
    INDEX idx_score (score),
    INDEX idx_app_name (app_name),
    INDEX idx_country (country)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 3. Cloud Sync Table
CREATE TABLE IF NOT EXISTS cloud_sync (
    id INT AUTO_INCREMENT PRIMARY KEY,
    player_id VARCHAR(100) NOT NULL,
    app_name VARCHAR(50) NOT NULL,
    cloud_data LONGTEXT,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UNIQUE KEY unique_player_sync (player_id, app_name),
    INDEX idx_player_id (player_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 4. Competitions Table
CREATE TABLE IF NOT EXISTS competitions (
    id INT AUTO_INCREMENT PRIMARY KEY,
    competition_id VARCHAR(100) UNIQUE NOT NULL,
    app_name VARCHAR(50),
    name VARCHAR(200),
    description TEXT,
    is_active BOOLEAN DEFAULT 1,
    start_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    end_time TIMESTAMP,
    prize_pool TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_active (is_active),
    INDEX idx_app_name (app_name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 5. App Messages Table
CREATE TABLE IF NOT EXISTS app_messages (
    id INT AUTO_INCREMENT PRIMARY KEY,
    message_id VARCHAR(100) UNIQUE NOT NULL,
    app_name VARCHAR(50),
    target_player_id VARCHAR(100),
    title VARCHAR(200),
    message TEXT,
    is_active BOOLEAN DEFAULT 1,
    read_by_players TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    expires_at TIMESTAMP,
    INDEX idx_app_name (app_name),
    INDEX idx_target_player (target_player_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 6. Player Messages Table
CREATE TABLE IF NOT EXISTS player_messages (
    id INT AUTO_INCREMENT PRIMARY KEY,
    message_id VARCHAR(100) UNIQUE NOT NULL,
    player_id VARCHAR(100) NOT NULL,
    app_name VARCHAR(50),
    from_player_id VARCHAR(100),
    message_type VARCHAR(50),
    message_content TEXT,
    is_read BOOLEAN DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_player_id (player_id),
    INDEX idx_is_read (is_read)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 7. Prizes Table
CREATE TABLE IF NOT EXISTS prizes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    prize_id VARCHAR(100) UNIQUE NOT NULL,
    player_id VARCHAR(100) NOT NULL,
    app_name VARCHAR(50),
    prize_type VARCHAR(50),
    prize_data TEXT,
    is_claimed BOOLEAN DEFAULT 0,
    claimed_at TIMESTAMP NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_player_id (player_id),
    INDEX idx_is_claimed (is_claimed)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 8. Purchases Table
CREATE TABLE IF NOT EXISTS purchases (
    id INT AUTO_INCREMENT PRIMARY KEY,
    purchase_id VARCHAR(100) UNIQUE,
    player_id VARCHAR(100) NOT NULL,
    app_name VARCHAR(50),
    product_id VARCHAR(100),
    purchase_token TEXT,
    purchase_data TEXT,
    verified BOOLEAN DEFAULT 0,
    amount DECIMAL(10, 2),
    currency VARCHAR(10),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    verified_at TIMESTAMP NULL,
    INDEX idx_player_id (player_id),
    INDEX idx_verified (verified)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 9. Online Events Table
CREATE TABLE IF NOT EXISTS online_events (
    id INT AUTO_INCREMENT PRIMARY KEY,
    event_id VARCHAR(100) UNIQUE NOT NULL,
    app_name VARCHAR(50),
    event_name VARCHAR(200),
    event_type VARCHAR(50),
    description TEXT,
    is_active BOOLEAN DEFAULT 1,
    start_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    end_time TIMESTAMP,
    reward_data TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_active (is_active),
    INDEX idx_app_name (app_name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 10. Event Leaderboard Table
CREATE TABLE IF NOT EXISTS event_leaderboard (
    id INT AUTO_INCREMENT PRIMARY KEY,
    event_id VARCHAR(100) NOT NULL,
    player_id VARCHAR(100) NOT NULL,
    app_name VARCHAR(50),
    score BIGINT DEFAULT 0,
    score2 BIGINT DEFAULT 0,
    player_name VARCHAR(100),
    rank INT DEFAULT 0,
    country_flag VARCHAR(50),
    image_url TEXT,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UNIQUE KEY unique_event_player (event_id, player_id),
    INDEX idx_event_id (event_id),
    INDEX idx_score (score)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 11. Saved Data Table
CREATE TABLE IF NOT EXISTS saved_data (
    id INT AUTO_INCREMENT PRIMARY KEY,
    player_id VARCHAR(100) NOT NULL,
    game_name VARCHAR(50),
    event_id VARCHAR(100),
    data_id VARCHAR(100),
    data_content LONGTEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_player_id (player_id),
    INDEX idx_data_id (data_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Insert sample data for testing

-- Sample competition
INSERT INTO competitions (competition_id, app_name, name, description, is_active, end_time)
VALUES ('comp_001', 'm3HomeDesign', 'Weekly Challenge', 'Compete for the top score!', 1, DATE_ADD(NOW(), INTERVAL 7 DAY));

-- Sample app message
INSERT INTO app_messages (message_id, app_name, title, message, is_active)
VALUES ('msg_001', 'm3HomeDesign', 'Welcome!', 'Welcome to the game! Complete levels to earn rewards.', 1);

-- Sample event
INSERT INTO online_events (event_id, app_name, event_name, event_type, description, is_active, end_time)
VALUES ('event_001', 'm3HomeDesign', 'Summer Festival', 'special', 'Special summer event with exclusive rewards!', 1, DATE_ADD(NOW(), INTERVAL 14 DAY));

-- Show created tables
SHOW TABLES;

-- Display table counts
SELECT 'players' as table_name, COUNT(*) as count FROM players
UNION ALL
SELECT 'leaderboard', COUNT(*) FROM leaderboard
UNION ALL
SELECT 'cloud_sync', COUNT(*) FROM cloud_sync
UNION ALL
SELECT 'competitions', COUNT(*) FROM competitions
UNION ALL
SELECT 'app_messages', COUNT(*) FROM app_messages
UNION ALL
SELECT 'player_messages', COUNT(*) FROM player_messages
UNION ALL
SELECT 'prizes', COUNT(*) FROM prizes
UNION ALL
SELECT 'purchases', COUNT(*) FROM purchases
UNION ALL
SELECT 'online_events', COUNT(*) FROM online_events
UNION ALL
SELECT 'event_leaderboard', COUNT(*) FROM event_leaderboard
UNION ALL
SELECT 'saved_data', COUNT(*) FROM saved_data;