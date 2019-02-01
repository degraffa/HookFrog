const mysql = require('mysql2');
const secrets = require('../secrets.json');

const Client = require('ssh2').Client;
const ssh = new Client();

// Adapted from https://medium.com/@devontem/nodejs-express-using-ssh-to-access-mysql-remotely-60372832dd08
const db = new Promise(function (resolve, reject) {
    console.log("Connecting to course database...");

    ssh.on('ready', function () {
        try {
            ssh.forwardOut(
                // source address, this can usually be any valid address
                '127.0.0.1',
                // source port, this can be any valid port number
                12345,
                // destination address (localhost here refers to the SSH server)
                '127.0.0.1',
                // destination port
                3306,
                function (err, stream) {
                    if (err) throw err; // SSH error: can also send error in promise ex. reject(err)

                    // use `sql` connection as usual
                    const connection = mysql.createConnection({
                        host: 'localhost',
                        user: 'team_439_views',
                        password: secrets.drivePasscode,
                        database: 'cs_4154_fall2018',
                        stream: stream,
                    });

                    // send connection back in variable depending on success or not
                    connection.connect(function (err) {
                        if (err) {
                            reject(err);
                        } else {
                            resolve(connection);
                        }
                    });
                });
        } catch (err) {
            reject(err);
        }
    }).connect({
        host: 'gdiac.cs.cornell.edu',
        port: 22,
        username: secrets.username,
        password: secrets.password
    });
});

module.exports = db;