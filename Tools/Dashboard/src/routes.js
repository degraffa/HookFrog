const express = require('express'); 

module.exports.getRouter = (connection) => {
    const router = express.Router(); 
    router.get('/burndown', (req, res) => {
        connection.query(
            `
            SELECT 
                quest_id as level_id, COUNT(DISTINCT(USER_ID)) as count 
            FROM team_439_quest 
            WHERE version_id = ? 
            GROUP BY quest_id
            `,
            [104], 
            (error, results, fields) => {
                res.json(results.filter(data => data.level_id > 0));
            }
        );
    })

    router.get('/checkpoint', (req, res) => {
       connection.query(
           ``
       )
    }); 

    router.get('/ab/burndown', (req, res) => {
        connection.query(
            `
            SELECT 
	            abtest.abvalue, quest_id as level_id, COUNT(DISTINCT(quests.USER_ID)) as count 
            FROM team_439_quest as quests
            INNER JOIN team_439_abtest as abtest on abtest.user_id = quests.user_id 
            WHERE version_id = ?
            GROUP BY quest_id, abtest.abvalue
            `,
            [104], 
            (error, results, fields) => {
                res.json(results.filter(data => data.level_id > 0));
            }
        );
    })

    router.get('/dau', (req, res) => {
        connection.query(
            `
            SELECT 
                DATE(FROM_UNIXTIME(server_timestamp)) as dayPlayed, COUNT(DISTINCT(user_id)) as numPlayers 
            FROM team_439_pageload 
            WHERE version_id = ? 
            GROUP BY dayPlayed
            `,
            [104],
            (error, results, fields) => {
                res.json(results);
            }
        )
    })


    return router; 
};