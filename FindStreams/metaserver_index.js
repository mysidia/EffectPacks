// server.js
// MetaData 1.0 server component for rh data
// C Mysidia 2019
//
// where your node app starts
// we've started you off with Express (https://expressjs.com/)
// but feel free to use whatever libraries or frameworks you'd like through `package.json`.
const express = require("express");
const app = express();
const https = require("https");
const rateLimit = require("express-rate-limit");
const NodeCache = require("node-cache");
const sha1 = require("sha1");
var csvtojson = require("@whatmelon12/csvtojson");
var fs = require('fs');
const DBClient = require("@replit/database");
const db = new DBClient();
const crypto = require('crypto')
var bodyParser = require('body-parser')

const clientid = process.env['tclientid'];
const serverkey = process.env['serverkey_value'];
//const mySecret = process.env['serverkey_value'];

// fernet key known only to the server
const frnkeyS = process.env['frnkeyS'];

fernet = require('fernet')
frnsecret = new fernet.Secret(frnkeyS)

const myCache = new NodeCache({ stdTTL: 60, checkperiod: 120 });
var oRequestCount = 0;

let ejs = require('ejs'),
    LRU = require('lru-cache');
ejs.cache = new LRU({"max":100}); // LRU cache with 100-item limit

var statekey = process.env['statekey'];

var rawdata = 
 fs.readFileSync('rhmd.dat').toString().replaceAll("\n","");
var buff = Buffer.from(rawdata, 'base64').toString('utf8');
rawdata = ""
var RHMD = JSON.parse(buff).reduce(function(map,obj) {
  map[ obj["id"] ] = obj
  return map
}, {});
buff = ""

//////
RHMD['meta']['timestamp'] = '1667245051';
RHMD['meta']['metakey'] = process.env['metakey'];
RHMD['meta']['xdata'] = {}
RHMD['meta']['xdata']['cur_metavers'] = '1.0'
RHMD['meta']['xdata']['min_metavers'] = '1.0'
RHMD['meta']['xdata']['cur_toolvers'] = '1.0'
RHMD['meta']['xdata']['min_toolvers'] = '1.0'
RHMD['meta']['xdata']['cur_mdcversion'] = '1.0'
RHMD['meta']['xdata']['min_mdcversion'] = '1.0'
RHMD['meta']['xdata']['1.0_mdckey'] = process.env['mdckey_1'];
RHMD['meta']['xdata']['timestamp'] = '1667245051';

/////




const limiter = rateLimit({
  windowMs: 1 * 15 * 1000 * 3, // 1 minutes
  max: 30, // limit each IP to 100 requests per windowMs
  message: "Oops. Too many requests. Please wait a few minutes."
});
 
// our default array of dreams
const dreams = [
  "Find and count some sheep",
  "Wash the dishes",
  "Beat a real kaizo hack",
  "Make the Add Dream button work correctly (/Dreams is just the form example, after all)"
];

function csvString(sin) {
  /* This function handles quoting text to put in a CSV file */
  return "\"" + sin.replace(/\"/g, "\"\"") + "\"";
}

/* Setup procedure for displaying points */
var starbucksFormatter = (function() {
  var instance;
  function createInstance() {
    var object = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', });
    return object;
  }

  {
    if (!instance) {
      instance = createInstance();
    }
    return instance;
  }
});


function chanIdOkay(chVal) {
  var hashVal = (sha1(("" + chVal).toLowerCase()));
  if (hashVal == "464083146d4abe26b997758f4ccdd100fe3274e8") { return 1; }
  if (hashVal == "c1e9263e79b14a55e617fd1fb5c6f6a1df9f7883") { return 1; }
  return 0;
}

function chanOkay(chVal) {
  var hashVal = (sha1(("" + chVal).toLowerCase()));
  if (hashVal == "b74b76376b077686e90d85d7b04ff238250a0c6c") { return 1; }
  if (hashVal == "e100e01da5e89dd7dc3f69e422e4eca376dd678d") { return 1; }
  return 0;
}



function verifyRequestChannelName(request, response) {

  if (!request || !request.params || !request.params["chname"] || !chanOkay(request.params["chname"])) {
    response.setHeader("Content-Type", "text/plain");
    response.status(503);
    if (!request || !request.params || !request.params["chname"] || request.params["chname"] == "") {
      response.send("Channel name not specified\n");
    } else {
      response.send("That is not one of the channel names in my allowed channels list.\n");
    }
    return 0;
  } else {
    return 1;
  }
}

function getstampOUI(objkey,userkey,twid) {
    recordval = {}
    recordval['objkey'] = objkey
    recordval['userkey'] = userkey
    recordval['twid'] = twid
    tok = new fernet.Token({secret: frnsecret})
    return tok.encode(JSON.stringify(recordval))
}

function verifyRequestChannelId(request, response) {

  if (!request || !request.params || !request.params["chid"] || !chanIdOkay(request.params["chid"])) {
    response.setHeader("Content-Type", "text/plain");
    response.status(503);
    if (!request || !request.params || !request.params["chid"] || request.params["chid"] == "") {
      response.send("Channel id not specified\n");
    } else {
      response.send("That is not one of the channel names in my allowed channels list.\n");
    }
    return 0;
  } else {
    return 1;
  }
}


// make all the files in 'public' available
// https://expressjs.com/en/starter/static-files.html
app.use(express.static("public"));
app.use(limiter);
app.use( bodyParser.json() ); 

app.get("/", (request, response) => {
   part1 = request.header("X-Forwarded-For")
   part1 = part1 + "_"
   part1 = part1 + new Date().getTime()
     shx = crypto.createHash('md5').update(statekey+ part1 + statekey).digest("hex")
  sheader = shx + "_" + part1

  
  //response.sendFile(__dirname + "/views/index.html");  
     ejs.renderFile(__dirname + "/views/index.ejs", 
                    {
                      "clientid" : clientid,
                      "stateid" : Buffer.from(sheader).toString('base64url')
                    }/*data*/, {}/*options*/, 
     function(err, str){
         if (err) {
           console.log(err)
         }
        response.send(str)
    // str => Rendered HTML string
     });

});

        
// https://expressjs.com/en/starter/basic-routing.htm
app.get("/setup", (request, response) => {
   part1 = request.header("X-Forwarded-For")
   part1 = part1 + "_"
   part1 = part1 + new Date().getTime()
     shx = crypto.createHash('md5').update(statekey+ part1 + statekey).digest("hex")
  sheader = shx + "_" + part1

  
  //response.sendFile(__dirname + "/views/index.html");  
     ejs.renderFile(__dirname + "/views/setup.ejs", 
                    {
                      "clientid" : clientid,
                      "stateid" : Buffer.from(sheader).toString('base64url')
                    }/*data*/, {}/*options*/, 
     function(err, str){
         if (err) {
           console.log(err)
         }
        response.send(str)
    // str => Rendered HTML string
     });


});
app.get("/jquery-3.6.1.min.js", (request, response) => {
  response.sendFile(__dirname + "/views/jquery-3.6.1.min.js");  
});

function tokenValidationRequest(appcid,twtoken) {
  urlval = "https://id.twitch.tv/oauth2/validate"
  
  return new Promise((resolve, reject) => {
    oRequestCount++;
    console.log("get " + urlval);
    https.get(urlval, {
       headers : {
         "Client-Id" : appcid,
         "Authorization" : "Bearer " + twtoken
       }
      }, res => {
      res.setEncoding("utf8");
      let body = "";
      res.on("data", data => { body += data; });
      res.on("end", () => { resolve(body.toString()); });
      res.on("error", (error) => { 
                reject(error); 
              });
    });

  });
}

async function validateTwitchToken(appcid,twtoken) {
  var cResult = myCache.get("twvalid_" + twtoken);

  if (cResult && cResult != undefined) {
    return cResult;
  }

  try {
    let http_promise = tokenValidationRequest(appcid,twtoken);
    let response_text = await http_promise;

    myCache.set("twvalid_" + twtoken, JSON.parse(response_text)._id, 1800);
  //  console.log("RESP:" + response_text)
    console.log("RESP:" + response_text)
    return JSON.parse(response_text);
  } catch (error) {
    console.log("VALIDATION ENDPOINT FAIL")
    return undefined;
  }
}


app.get("/process/login", (request, response) => {  
     ejs.renderFile(__dirname + "/views/process.ejs", 
                    {
                      "clientid" : clientid                 
                    }/*data*/, {}/*options*/, 
     function(err, str){
         if (err) {
           console.log(err)
         }
        response.send(str)
    // str => Rendered HTML string
     });
});

async function processDataFunction (req, response) { 
  datafields =                     {
    "twclientid" : req.body.twclientid,
     "twtoken" : req.body.twtoken,
   "twtokentype" : req.body.twtokentype,
         "twlogin" : req.body.twlogin,
               "twid" : req.body.twid,
               "twcreated_at" : req.body.twcreated_at,
               "twitchinfo" : req.body.twitchinfo,
               "twstateinfo" : req.body.twstateinfo
   
                    };
  statearr = Buffer.from(req.body.twstateinfo,'base64url').toString().split('_')
 shx = crypto.createHash('md5').update(statekey+ statearr[1] + '_' + statearr[2] + statekey).digest("hex")
 if (!(shx ===  statearr[0]  )) {
   response.send(
     'Invalid state'
   )
   return
 }

  //console.log("REQUEST AVLIDATION: " + clientid + " " + req.body.twtoken)
  validation = await validateTwitchToken(clientid, req.body.twtoken)

  datafields[ 'validated'] = 0

  //  console.log("VALIDATION:" + validation)
 // console.log(validation.login + " =? "  + req.body.twlogin)
  if (!validation ) {
  
  }
  else if ( 'status' in Object.keys(validation) && validation.status == 401 ) {
    datafields['status'] = '401'
    datafields['message'] = validation.message
  } else
  if (    
     validation.login === req.body.twlogin &&
      validation.user_id === req.body.twid) {
      dbuserobj = await db.get('users_' +  validation.user_id)
      if (!dbuserobj) {
        tsval = Date.now()
        tokenstring = serverkey + ':' +  validation.user_id + ':' + tsval + ':' +  serverkey
        newuserkey = crypto.createHash('sha256').update('TS'+tokenstring).digest().toString('base64url')
        tok = new fernet.Token({secret: frnsecret})
        userkeytoken = 
        tok.encode(JSON.stringify({'tsval':tsval, 'user_id':validation.user_id, 'twlogin':validation.login, 'expires_in':expires_in,
                                  'scopes': validation.scopes, 'client_id': validation.client_id,
                                  'twtoken': req.body.twtoken}))
        
           uo = {
             login: validation.login,
             twid: validation.user_id,
             userkey: newuserkey,
             userkeytoken: userkeytoken
           }
        await db.set('users_' + validation.user_id, uo)
        dbuserobj = await db.get('users_' + validation.user_id)
      }
      datafields[ 'validated' ] = 1
      datafields['userkey' ] = dbuserobj['userkey']
      datafields['userkeytoken'] = dbuserobj['userkeytoken']
  }

  tok = new fernet.Token({secret: frnsecret})
  datafields['encryptedvalue'] = tok.encode(JSON.stringify(datafields))

  if ( datafields.hasOwnProperty('twtoken') ) {
    delete datafields['twtoken']
  }

  //console.log(datafields)
  response.json(datafields) 
  /*ejs.renderFile(__dirname + "/views/process_data.ejs", 
      datafields, {}, 
     function(err, str){
         if (err) {
           console.log(err)
         }
        response.send(str)
     });*/
};

app.post("/process/data", processDataFunction);



app.get("/:userkey/:ts/:sig/rhmd_dist.dat", (request, response) => {
  response.sendFile(__dirname + "/views/rhmd_dist.dat");
});

// send the default array of dreams to the webpage
app.get("/dreamsz7gzzz", (request, response) => {
  // express helps us take JS objects and send them as JSON
  response.json(dreams);
});

app.get("/requestcount", (request, response) => {
  response.send(oRequestCount + "\n");
});

// listen for requests :)
const listener = app.listen(process.env.PORT, () => {
  console.log("Your app is listening on port " + listener.address().port);
});

app.post("/md/:objkey/:rhid", async (request,response) => {
     x_userkey = request.header('userkey')
     x_twid = request.header('twid')
     objkey = request.params["objkey"]
     rhid = request.params["rhid"]
     // /:userkey/:ts/:objkey/:rhid/:sig

     RHMD[rhid]['xdata']['cur_mdcversion'] = '1.0'
     //RHMD[rhid]['xdata']
     
     hh = hash = crypto.createHash('md5').update(process.env['auth_pattern'] + rhid + process.env['auth_pattern']).digest("hex")

  if  (hh === objkey) {
       dbuserobj = await db.get('users_' + x_twid)

        if (dbuserobj && dbuserobj['userkey'] == x_userkey) { 
            response.json( RHMD[rhid] )
        } else {
          request.json({ error : 'User not found' })
        }
       return;
  }
  
  response.json({'error' : 'Invalid lookup' })
})

/*
app.get("/lbseries/:chname/:user", async (request, response) => {
  //sbdata = fs.readFileSync('.data/sbux202006.csv');
  var lbseries = await csvtojson().fromFile('.data/sbux202006.csv');
  var rdata = [];

  console.log("" + request.params["user"]);

  lbseries.forEach(
    (sentry) => {
      if (sentry.username.toLowerCase() == request.params["user"].toLowerCase() &&
        request.params["chname"].toLowerCase() == "jensorbit") {
        rdata.push( // [ sentry.timestamp * 1000 , parseInt(sentry.points) ]
          {
            x: sentry.timestamp * 1000,
            y: parseInt(sentry.points),
            position: parseInt(sentry.position)
          }
        );
      }
    }
  );

  response.json(rdata);

});*/

/*app.get("/betseries/:chname/:user", async (request, response) => {
  //sbdata = fs.readFileSync('.data/sbux202006.csv');
  var betseries = await csvtojson().fromFile('.data/betresults202006.csv');
  var rdata = [];

  console.log("" + request.params["user"]);

  betseries.forEach(
    (sentry) => {
      if (sentry.username.toLowerCase() == request.params["user"].toLowerCase() &&
        request.params["chname"].toLowerCase() == "jensorbit") {
        //                       rdata.push (  [ sentry.timestamp * 1000 , 
        //                                       parseInt(sentry.betvalue) /*, parseInt(sentry.g)*  //
        ] );
        rdata.push({
          x: sentry.timestamp * 1000,
          y: parseInt(sentry.betvalue) /*, parseInt(sentry.g)* / // ,
          g: (sentry.status == "completed") ? sentry.g : 0,
          netgain: (sentry.status == "completed") ? (sentry.g - parseInt(sentry.betvalue)) : 0,
          contestname: sentry.contestname,
          bcommand: sentry.bcommand,
          wintype: (sentry.status == "completed") ? sentry.wintype : (sentry.status),
          pfactor: sentry.pfactor,
          marker: {
            symbol: (sentry.status == "completed") ? (
              (sentry.wintype == "is_win") ? 'triangle' : 'triangle-down') : 'square'
          }
        });

      }
    }
  );

  response.json(rdata);

});*/



/*app.get("/top10/:chname", async (request, response) => {
  var textresult = "";

  if (!verifyRequestChannelName(request, response)) {
    return;
  }
  //response.send("TEST"); return;
  // express helps us take JS objects and send them as JSON
  //  (async function() {
  try {
    var chid = await getChannelId(request.params["chname"]);
    var leaders = await getPointsLeaderboard(chid);
    var ranknum = 1;
    var userIndex = 0;
    var user = null;

    for (userIndex = 0; userIndex < leaders.users.length &&
      ranknum <= 10; userIndex++) {
      user = leaders.users[userIndex];
      if (ranknum > 1) {
        textresult += ", ";
      }
      textresult += " " + ranknum + ". " + user.username + " (" + user.points + ")";
      ranknum++;
    }
    // express helps us take JS objects and send them as JSON
    //  })();

    response.setHeader("Content-Type", "text/plain");
    response.send("Top 10: " + textresult + "\n");
  } catch (error) {
    response.send("ERROR: " + error.toString());
  }
});*/

/*
app.get("/top10txt/:chname", async (request, response) => {
  var textresult = "";

  if (!verifyRequestChannelName(request, response)) {
    return;
  }

  //response.send("TEST"); return;
  // express helps us take JS objects and send them as JSON
  //  (async function() {
  try {

    var chid = await getChannelId(request.params["chname"]);
    var leaders = await getPointsLeaderboard(chid);
    var ranknum = 1;
    var userIndex = 0;
    var user = null;

    for (userIndex = 0; userIndex < leaders.users.length &&
      ranknum <= 10; userIndex++) {
      user = leaders.users[userIndex];
      if (ranknum > 1) {
        textresult += "";
      }
      textresult += " ⭐ " + ranknum + ". " + user.username + " (" + starbucksFormatter().format(user.points) + ")\n";
      ranknum++;
    }
    // express helps us take JS objects and send them as JSON
    //  })();

    response.setHeader("Content-Type", "text/plain");
    response.send(textresult + "\n");
  } catch (error) {
    response.send("ERROR: " + error.toString());
  }
});*/

/*
app.get("/top200points/:chname", async (request, response) => {
  var lbTimestamp = (new Date()) / 1000;
  var textresult = "rank,username,points,timestamp\n";

  if (!verifyRequestChannelName(request, response)) {
    return;
  }

  //response.send("TEST"); return;
  // express helps us take JS objects and send them as JSON
  //  (async function() {
  try {

    var chid = await getChannelId(request.params["chname"]);
    var leaders = await getPointsLeaderboard(chid);
    var ranknum = 1;
    var userIndex = 0;
    var user = null;

    for (userIndex = 0; userIndex < leaders.users.length; userIndex++) {
      user = leaders.users[userIndex];
      textresult += "" + csvString(ranknum.toString()) + "," + csvString(user.username) + "," + csvString(user.points.toString()) + "," + csvString("" + lbTimestamp) + "\n";
      ranknum++;
    }
    // express helps us take JS objects and send them as JSON
    //  })();

    response.setHeader("Content-Type", "text/plain");
    response.send(textresult + "\n");
  } catch (error) {
    response.send("ERROR: " + error.toString());
  }
});*/

//requestLeaderBoardAlltime
/*app.get("/top200alltime/:chname", async (request, response) => {
  var textresult = "";

  if (!verifyRequestChannelName(request, response)) {
    return;
  }

  //response.send("TEST"); return;
  // express helps us take JS objects and send them as JSON
  //  (async function() {
  try {

    var chid = await getChannelId(request.params["chname"]);
    var leaders = await getPointsLeaderboardAlltime(chid);
    var ranknum = 1;
    var userIndex = 0;
    var user = null;

    for (userIndex = 0; userIndex < leaders.users.length; userIndex++) {
      user = leaders.users[userIndex];
      textresult += "" + csvString(ranknum.toString()) + "," + csvString(user.username) + "," + csvString(user.points.toString()) + "\n";
      ranknum++;
    }
    // express helps us take JS objects and send them as JSON
    //  })();

    response.setHeader("Content-Type", "text/plain");
    response.send(textresult + "\n");
  } catch (error) {
    response.send("ERROR: " + error.toString());
  }
});*/

/*
app.get("/getcontests1/:chid", async (request, response) => {
  var textresult = "";

  if (!verifyRequestChannelId(request, response)) {
    return;
  }

  //response.send("TEST"); return;
  // express helps us take JS objects and send them as JSON
  //  (async function() {
  try {

    //var chid = await getChannelId(request.params["chname"]);
    var chid = await (request.params["chid"]);
    var contests = await getContests(chid);


    response.setHeader("Content-Type", "text/json");
    response.json(contests);
    //response.send(contests + "\n");    
  } catch (error) {
    response.send("ERROR: " + error.toString());
  }
});
*/

/*
app.get("/latestcontest/:chid/title", async (request, response) => {
  var textresult = "";
  var chid = (request.params["chid"]);;

  if (!verifyRequestChannelId(request, response)) {
    if (!verifyRequestChannelName(request, response)) {
      return;
    } else {
      chid = await getChannelId(request.params["chid"]);
    }
  }

  //response.send("TEST"); return;
  // express helps us take JS objects and send them as JSON
  //  (async function() {
  try {

    //var chid = await getChannelId(request.params["chname"]);
    var chid = await (request.params["chid"]);
    var contests = await getContests(chid);

    response.setHeader("Content-Type", "text/plain");
    response.send("" + (contests.active.title) + "\n");
    //response.send(contests + "\n");    
  } catch (error) {
    response.send("ERROR: " + error.toString());
  }
});
*/


/*
app.get("/findid/:chname", async (request, response) => {
  if (!verifyRequestChannelName(request, response)) {
    return;
  }

  if (1) {
    //response.send("{}"); return;
    //response.send(request.params["chname"] + " TEST");
    result1 = await getChannelId(request.params["chname"]);
    response.send(result1);

    //  (async function() {
    //  result1 = await getChannelId(request.params["chname"]);
    //  // express helps us take JS objects and send them as JSON
    //  response.send(result1);
    //  })();
  }
});
*/

async function getChannelId(channel_name) {
  var cId = myCache.get("chid_" + channel_name);

  if (cId && cId != undefined) {
    return cId;
  }

  try {
    let http_promise = requestChannelId(channel_name);
    let response_text = await http_promise;

    myCache.set("chid_" + channel_name, JSON.parse(response_text)._id, 1200);
    return JSON.parse(response_text)._id;
  } catch (error) {
    return undefined;
  }
}

async function getPointsLeaderboard(channelid) {
  // var lbTimestamp = (new Date())/1000; 
  var pointsLB = myCache.get("pointslb_" + channelid);
  if (pointsLB != undefined) {
    return pointsLB;
  }

  try {
    let http_promise = requestLeaderBoard(channelid);
    let response_text = await http_promise;

    myCache.set("pointslb_" + channelid, JSON.parse(response_text), 2);
    return JSON.parse(response_text);
  } catch (error) {
    return null;
  }
}

async function getPointsLeaderboardAlltime(channelid) {
  var pointsLB = myCache.get("pointslballtime_" + channelid);
  if (pointsLB != undefined) {
    return pointsLB;
  }

  try {
    let http_promise = requestLeaderBoardAlltime(channelid);
    let response_text = await http_promise;

    myCache.set("pointslballtime_" + channelid, JSON.parse(response_text), 5);
    return JSON.parse(response_text);
  } catch (error) {
    return null;
  }
}

async function getContests(channelid) {
  // var lbTimestamp = (new Date())/1000; 
  var pointsLB = myCache.get("contests_" + channelid);
  if (pointsLB != undefined) {
    return pointsLB;
  }

  try {
    let http_promise = requestContests(channelid);
    let response_text = await http_promise;

    myCache.set("contests_" + channelid, JSON.parse(response_text), 2);
    return JSON.parse(response_text);
  } catch (error) {
    return null;
  }
}



function requestChannelId(channel_name) {
  urlval = "https://api.streamelements.com/kappa/v2/channels/" + encodeURI(channel_name);

  return new Promise((resolve, reject) => {

    oRequestCount++;
    console.log("get " + urlval);
    https.get(urlval, res => {
      res.setEncoding("utf8");
      let body = "";
      res.on("data", data => { body += data; });
      res.on("end", () => { resolve(body.toString()); });
      res.on("error", (error) => { reject(error); });
    });

  });
}

function requestLeaderBoard(channelid) {
  urlval = "https://api.streamelements.com/kappa/v2/points/" + channelid + "/top/all?limit=200";

  return new Promise((resolve, reject) => {
    oRequestCount++;
    console.log("get " + urlval);
    https.get(urlval, res => {
      res.setEncoding("utf8");
      let body = "";
      res.on("data", data => { body += data; });
      res.on("end", () => { resolve(body.toString()); });
      res.on("error", (error) => { reject(error); });
    });

  });
}


function requestLeaderBoardAlltime(channelid) {
  urlval = "https://api.streamelements.com/kappa/v2/points/" + channelid + "/alltime/all?limit=200";

  return new Promise((resolve, reject) => {
    oRequestCount++;
    console.log("get " + urlval);
    https.get(urlval, res => {
      res.setEncoding("utf8");
      let body = "";
      res.on("data", data => { body += data; });
      res.on("end", () => { resolve(body.toString()); });
      res.on("error", (error) => { reject(error); });
    });

  });
}


function requestContests(channelid) {
  urlval = "https://api.streamelements.com/kappa/v2/contests/" + channelid + "";

  return new Promise((resolve, reject) => {
    oRequestCount++;
    console.log("get " + urlval);
    https.get(urlval, res => {
      res.setEncoding("utf8");
      let body = "";
      res.on("data", data => { body += data; });
      res.on("end", () => { resolve(body.toString()); });
      res.on("error", (error) => { reject(error); });
    });

  });
}



async function requestLeaderBoardPages(channelid, offset, pagesToGet) {
  var offsetString = (offset <= 0) ? "" : ("offset=" + offset);
  var urlval = "https://api.streamelements.com/kappa/v2/points/" + channelid + "/top/all?limit=200" + offsetString;

  return new Promise((resolve, reject) => {
    oRequestCount++;
    console.log("get " + urlval);
    https.get(urlval, res => {
      res.setEncoding("utf8");
      let body = "";
      res.on("data", data => { body += data; });
      res.on("end", () => { /*resolve(body.toString());*/
        var d = JSON.parse(body);

        if (d.users.length >= 200 && offset < pagesToGet * 200) {
          var e = async function() { return await requestLeaderBoardPages(channelid, offset + 200, pagesToGet - 1) }();

          e.users.forEach(function(uobj) {
            d.users.push(uobj);
          });
        } else {
          resolve(d);
        }
      });
      res.on("error", (error) => { reject(error); });
    });

  });
}
