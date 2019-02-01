const express = require('express');
const opn = require('opn');
const db = require('./db');
const routes = require('./routes'); 

// Connect to Course MYSQL DB
db.then(connection => {
    console.log("Succesfully connected to course db!")
    console.log("Launching analytics dashboard...") 

    const app = express();
    // Set the view engine to ejs
    app.set('view engine', 'ejs');

    // Load API Routes
    app.use('/api', routes.getRouter(connection)); 

    // Configure Pages
    app.get('/', (req, res) => res.render('pages/index'));
    app.get('/burndown', (req, res) => res.render('pages/burndown', {title: "Burndown Chart"}));
    app.get('/dau', (req, res) => res.render('pages/dau', {title: "Daily Active Users"}));

    app.get('/ab/burndown', (req, res) => res.render('pages/abtest/burndown', {title: "AB Burndown Chart"}));


    // Launch App on Port 8080
    app.listen(8080);
    // Launch User Web Browser to Port 8080
    opn('http://localhost:8080');

}).catch(err => {
    const troubleshooting = 
    `   COULD NOT CONNECT TO COURSE DB SERVER
        =====================================
        TROUBLESHOOTING:
            1) Connect to the campus VPN at: https://it.cornell.edu/cuvpn
            2) Make sure you've created a 'secrets.json' file with valid credentials
    `;
    console.log(troubleshooting);
})
