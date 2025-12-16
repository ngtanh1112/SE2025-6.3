const express = require('express');
const mysql = require('mysql2/promise');
const crypto = require('crypto');
const cors = require('cors');

const app = express();
const PORT = 8080;

// Middleware
app.use(cors());
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// Server Constants (from Unity)
const SERVER_CONFIG = {
  privateKey: 'RKaEFn3f1OpzsqtV7L9B7Usjnxl0QwA2',
  publicKey: '1ADVDohGq8xvFLZg8NMA9SvnjE0RHJmE',
  appName: 'm3HomeDesign'
};

// MySQL Connection Pool
const pool = mysql.createPool({
  host: 'localhost',
  user: 'root',
  password: '',
  database: 'game_backend',
  waitForConnections: true,
  connectionLimit: 10,
  queueLimit: 0
});

// Utility Functions
function generateNonce() {
  return crypto.randomBytes(16).toString('hex');
}

function verifySignature(params, signature) {
  // Simplified signature verification - in production, implement proper HMAC verification
  return true; // Accept all requests for now
}

function generatePlayerId() {
  return 'player_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
}

// ==================== ROUTES ====================

// Health check
app.get('/', (req, res) => {
  res.json({ 
    status: 'ok', 
    message: 'Game Backend Server Running',
    timestamp: new Date().toISOString()
  });
});

// 1. Get Nonce
app.get('/client/nonce', (req, res) => {
  const nonce = generateNonce();
  res.json({ 
    success: true,
    nonce: nonce,
    timestamp: Date.now()
  });
});

// 2. Get Server Time
app.get('/client/time', (req, res) => {
  res.json({
    success: true,
    serverTime: Date.now(),
    timestamp: new Date().toISOString()
  });
});

// 3. Get Player ID
app.get('/users/getId', async (req, res) => {
  try {
    const { installId, app } = req.query;
    
    if (!installId) {
      return res.json({
        success: true,
        playerId: generatePlayerId(),
        isNew: true
      });
    }

    // Check if player exists
    const [rows] = await pool.query(
      'SELECT player_id FROM players WHERE install_id = ?',
      [installId]
    );

    if (rows.length > 0) {
      res.json({
        success: true,
        playerId: rows[0].player_id,
        isNew: false
      });
    } else {
      // Create new player
      const playerId = generatePlayerId();
      await pool.query(
        'INSERT INTO players (player_id, install_id, app_name, created_at) VALUES (?, ?, ?, NOW())',
        [playerId, installId, app || SERVER_CONFIG.appName]
      );
      
      res.json({
        success: true,
        playerId: playerId,
        isNew: true
      });
    }
  } catch (error) {
    console.error('Get ID Error:', error);
    res.json({
      success: true,
      playerId: generatePlayerId(),
      isNew: true
    });
  }
});

// 4. Update Player Score (Leaderboard)
app.get('/comp/update', async (req, res) => {
  try {
    const { app, pid, sR, c, nonce, rank2, lv } = req.query;
    const score = parseInt(sR) || 0;

    await pool.query(
      `INSERT INTO leaderboard (player_id, app_name, score, country, rank, level_version, updated_at)
       VALUES (?, ?, ?, ?, ?, ?, NOW())
       ON DUPLICATE KEY UPDATE score = GREATEST(score, ?), updated_at = NOW()`,
      [pid, app, score, c, rank2, lv, score]
    );

    res.json({
      success: true,
      message: 'Score updated',
      score: score
    });
  } catch (error) {
    console.error('Update Score Error:', error);
    res.json({ success: false, error: error.message });
  }
});

// 5. Get Leaderboard
app.get('/comp/lead', async (req, res) => {
  try {
    const { app, pid, sR, country, distAroundPlayer = 5, topEntries = 10 } = req.query;

    // Get top players
    const [topPlayers] = await pool.query(
      `SELECT player_id, score, country, rank, updated_at 
       FROM leaderboard 
       WHERE app_name = ? 
       ORDER BY score DESC 
       LIMIT ?`,
      [app, parseInt(topEntries)]
    );

    // Get players around current player
    const [aroundPlayers] = await pool.query(
      `SELECT player_id, score, country, rank 
       FROM leaderboard 
       WHERE app_name = ? AND player_id != ?
       ORDER BY ABS(score - ?) 
       LIMIT ?`,
      [app, pid, parseInt(sR) || 0, parseInt(distAroundPlayer)]
    );

    res.json({
      success: true,
      topPlayers: topPlayers,
      aroundPlayers: aroundPlayers,
      playerScore: parseInt(sR) || 0
    });
  } catch (error) {
    console.error('Leaderboard Error:', error);
    res.json({ 
      success: true, 
      topPlayers: [], 
      aroundPlayers: [] 
    });
  }
});

// 6. Get Segmented Leaderboards
app.get('/comp/segmentedLead', async (req, res) => {
  try {
    const { app, pid, country, topEntries = 10 } = req.query;

    const [players] = await pool.query(
      `SELECT player_id, score, country 
       FROM leaderboard 
       WHERE app_name = ? AND country = ?
       ORDER BY score DESC 
       LIMIT ?`,
      [app, country, parseInt(topEntries)]
    );

    res.json({
      success: true,
      leaderboard: players
    });
  } catch (error) {
    console.error('Segmented Leaderboard Error:', error);
    res.json({ success: true, leaderboard: [] });
  }
});

// 7. Cloud Sync - Get
app.get('/cs/get', async (req, res) => {
  try {
    const { app, pid, nonce } = req.query;

    const [rows] = await pool.query(
      'SELECT cloud_data, updated_at FROM cloud_sync WHERE player_id = ? AND app_name = ?',
      [pid, app]
    );

    if (rows.length > 0) {
      res.json({
        success: true,
        data: rows[0].cloud_data,
        timestamp: rows[0].updated_at
      });
    } else {
      res.json({
        success: true,
        data: null,
        message: 'No cloud data found'
      });
    }
  } catch (error) {
    console.error('Cloud Sync Get Error:', error);
    res.json({ success: false, error: error.message });
  }
});

// 8. Cloud Sync - Update
app.post('/cs/update', async (req, res) => {
  try {
    const { app, pid, nonce } = req.query;
    const cloudData = req.body.data || JSON.stringify(req.body);

    await pool.query(
      `INSERT INTO cloud_sync (player_id, app_name, cloud_data, updated_at)
       VALUES (?, ?, ?, NOW())
       ON DUPLICATE KEY UPDATE cloud_data = ?, updated_at = NOW()`,
      [pid, app, cloudData, cloudData]
    );

    res.json({
      success: true,
      message: 'Cloud data saved'
    });
  } catch (error) {
    console.error('Cloud Sync Update Error:', error);
    res.json({ success: false, error: error.message });
  }
});

// 9. Get Active Competition
app.get('/comp/active', async (req, res) => {
  try {
    const [rows] = await pool.query(
      'SELECT * FROM competitions WHERE is_active = 1 AND end_time > NOW() ORDER BY start_time DESC LIMIT 1'
    );

    if (rows.length > 0) {
      res.json({
        success: true,
        competition: rows[0]
      });
    } else {
      res.json({
        success: true,
        competition: null,
        message: 'No active competition'
      });
    }
  } catch (error) {
    console.error('Active Competition Error:', error);
    res.json({ success: true, competition: null });
  }
});

// 10. Get App Messages
app.get('/getMessage', async (req, res) => {
  try {
    const { app, playerID } = req.query;

    const [rows] = await pool.query(
      'SELECT * FROM app_messages WHERE app_name = ? AND (target_player_id = ? OR target_player_id IS NULL) AND is_active = 1',
      [app, playerID]
    );

    res.json({
      success: true,
      messages: rows
    });
  } catch (error) {
    console.error('Get Messages Error:', error);
    res.json({ success: true, messages: [] });
  }
});

// 11. Update App Messages
app.post('/updateMessage', async (req, res) => {
  try {
    const { message_id, pid } = req.query;

    await pool.query(
      'UPDATE app_messages SET read_by_players = CONCAT(read_by_players, ?) WHERE message_id = ?',
      [pid + ',', message_id]
    );

    res.json({
      success: true,
      message: 'Message updated'
    });
  } catch (error) {
    console.error('Update Message Error:', error);
    res.json({ success: false, error: error.message });
  }
});

// 12. Get Player Messages
app.get('/messages/getMessages', async (req, res) => {
  try {
    const { app, pid } = req.query;

    const [rows] = await pool.query(
      'SELECT * FROM player_messages WHERE player_id = ? AND app_name = ? ORDER BY created_at DESC',
      [pid, app]
    );

    res.json({
      success: true,
      messages: rows
    });
  } catch (error) {
    console.error('Get Player Messages Error:', error);
    res.json({ success: true, messages: [] });
  }
});

// 13. Mark Message as Read
app.get('/messages/markAsRead', async (req, res) => {
  try {
    const { app, pid, requestIds } = req.query;

    const ids = requestIds.split(',');
    await pool.query(
      'UPDATE player_messages SET is_read = 1 WHERE message_id IN (?) AND player_id = ?',
      [ids, pid]
    );

    res.json({
      success: true,
      message: 'Messages marked as read'
    });
  } catch (error) {
    console.error('Mark Read Error:', error);
    res.json({ success: false, error: error.message });
  }
});

// 14. Get Prizes
app.get('/comp/getPrizes', async (req, res) => {
  try {
    const { app, pid } = req.query;

    const [rows] = await pool.query(
      'SELECT * FROM prizes WHERE player_id = ? AND app_name = ? AND is_claimed = 0',
      [pid, app]
    );

    res.json({
      success: true,
      prizes: rows
    });
  } catch (error) {
    console.error('Get Prizes Error:', error);
    res.json({ success: true, prizes: [] });
  }
});

// 15. Acknowledge Prizes
app.get('/comp/ackPrizes', async (req, res) => {
  try {
    const { app, pid } = req.query;

    await pool.query(
      'UPDATE prizes SET is_claimed = 1, claimed_at = NOW() WHERE player_id = ? AND app_name = ?',
      [pid, app]
    );

    res.json({
      success: true,
      message: 'Prizes acknowledged'
    });
  } catch (error) {
    console.error('Ack Prizes Error:', error);
    res.json({ success: false, error: error.message });
  }
});

// 16. Verify In-App Purchase
app.post('/ia/play/verify', async (req, res) => {
  try {
    const purchaseData = req.body;

    // Log purchase for verification
    await pool.query(
      'INSERT INTO purchases (player_id, product_id, purchase_token, purchase_data, verified, created_at) VALUES (?, ?, ?, ?, ?, NOW())',
      [purchaseData.playerId, purchaseData.productId, purchaseData.purchaseToken, JSON.stringify(purchaseData), 1]
    );

    res.json({
      success: true,
      verified: true,
      message: 'Purchase verified'
    });
  } catch (error) {
    console.error('Verify Purchase Error:', error);
    res.json({ success: false, verified: false, error: error.message });
  }
});

// 17. Get Online Events
app.get('/getonlineevents', async (req, res) => {
  try {
    const [rows] = await pool.query(
      'SELECT * FROM online_events WHERE is_active = 1 AND end_time > NOW()'
    );

    res.json({
      success: true,
      events: rows
    });
  } catch (error) {
    console.error('Get Events Error:', error);
    res.json({ success: true, events: [] });
  }
});

// 18. Get Events Leaderboard
app.get('/geteventslead', async (req, res) => {
  try {
    const { app, Eventid } = req.query;

    const [rows] = await pool.query(
      'SELECT * FROM event_leaderboard WHERE event_id = ? AND app_name = ? ORDER BY score DESC LIMIT 100',
      [Eventid, app]
    );

    res.json({
      success: true,
      leaderboard: rows
    });
  } catch (error) {
    console.error('Events Leaderboard Error:', error);
    res.json({ success: true, leaderboard: [] });
  }
});

// 19. Update Event Score
app.post('/updatechallengescore', async (req, res) => {
  try {
    const { app, PlayerId, Eventid, Score1, Score2, player_name, player_rank } = req.query;

    await pool.query(
      `INSERT INTO event_leaderboard (event_id, player_id, app_name, score, score2, player_name, rank, updated_at)
       VALUES (?, ?, ?, ?, ?, ?, ?, NOW())
       ON DUPLICATE KEY UPDATE score = GREATEST(score, ?), updated_at = NOW()`,
      [Eventid, PlayerId, app, Score1, Score2, player_name, player_rank, Score1]
    );

    res.json({
      success: true,
      message: 'Event score updated'
    });
  } catch (error) {
    console.error('Update Event Score Error:', error);
    res.json({ success: false, error: error.message });
  }
});

// 20. Upload Lead Data
app.post('/savedata', async (req, res) => {
  try {
    const { gameName, eventId, playerId, DataId, nonce } = req.query;
    const data = req.body.data || JSON.stringify(req.body);

    await pool.query(
      'INSERT INTO saved_data (player_id, game_name, event_id, data_id, data_content, created_at) VALUES (?, ?, ?, ?, ?, NOW())',
      [playerId, gameName, eventId, DataId, data]
    );

    res.json({
      success: true,
      message: 'Data saved'
    });
  } catch (error) {
    console.error('Save Data Error:', error);
    res.json({ success: false, error: error.message });
  }
});

// Facebook endpoints (stub implementations)
app.get('/facebook/getInvitableFriends', (req, res) => {
  res.json({ success: true, friends: [] });
});

app.get('/facebook/getPlayingFriends', (req, res) => {
  res.json({ success: true, friends: [] });
});

app.get('/facebook/getPlayerProfile', (req, res) => {
  res.json({ success: true, profile: {} });
});

// Get Player Positions
app.get('/comp/getPositions', async (req, res) => {
  try {
    const { app, pid } = req.query;

    const [rows] = await pool.query(
      'SELECT player_id, score, rank FROM leaderboard WHERE app_name = ? ORDER BY score DESC',
      [app]
    );

    res.json({
      success: true,
      positions: rows
    });
  } catch (error) {
    console.error('Get Positions Error:', error);
    res.json({ success: true, positions: [] });
  }
});

// Get Friend Profiles
app.get('/cs/getProfiles', (req, res) => {
  res.json({ success: true, profiles: [] });
});

// Error handling middleware
app.use((err, req, res, next) => {
  console.error('Server Error:', err);
  res.status(500).json({ 
    success: false, 
    error: 'Internal server error' 
  });
});

// Start server
app.listen(PORT, () => {
  console.log(`=================================`);
  console.log(`Game Backend Server Started`);
  console.log(`Port: ${PORT}`);
  console.log(`URL: http://localhost:${PORT}`);
  console.log(`App Name: ${SERVER_CONFIG.appName}`);
  console.log(`=================================`);
});

// Handle graceful shutdown
process.on('SIGTERM', () => {
  console.log('SIGTERM received, closing server...');
  server.close(() => {
    console.log('Server closed');
    pool.end();
  });
});