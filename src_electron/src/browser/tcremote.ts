import 'dotenv/config';
const { PORT, Client_ID, Client_Secret, App_Name } = process.env;
import { app, BrowserWindow, ipcMain, dialog, session } from 'electron';
import * as url from 'url';
import * as path from 'path';
import { LoginObjectType } from '../common/type';
import  Axios from 'axios';
const express = require('express');
const expressApp = express();
const expressPort = PORT || 3500;


const TAG = '[main] TCRemote';
const HTML = url.format({
    protocol: 'file',
    pathname: path.join(__dirname, '../../static/index.html'),
});

//Trimble OAuth 설정
const redirectUri = `http://localhost:${expressPort}/`;

//Request
const axios = require('axios');
const basicAuth = Buffer.from(`${Client_ID}:${Client_Secret}`).toString('base64'); 
const headers = {
    "Authorization" : `Basic ${basicAuth}`,
    "Content-Type" : "application/x-www-form-urlencoded",
    "Accept" : "application/json"
}

//Login Endpoint
const authorizationUri = `https://id.trimble.com/oauth/authorize?` + 
`scope=openid+DWGUploader&` +
`response_type=code&` +
`redirect_uri=${redirectUri}&` +
`client_id=${Client_ID}`;

export class TCRemote {
    
    private _app;

    constructor() {
        this.ExpressInit();
        this._app = app;
        this._app.on('ready', this._ready);
    }

    private _ready = () => {
        
        console.log(TAG, 'app ready');
        const mainWin = new BrowserWindow({
            width : 1280,
            minWidth : 960,
            height : 720,
            minHeight : 640,
            maximizable: true,
            webPreferences : {
                nodeIntegration: true,
                contextIsolation : false
            }
        });
        mainWin.loadURL(HTML);
        

        ipcMain.on('request-login', (event) => {
            this.TCAuth()
        })
    }

    private TCAuth = async() => {
        
        const authWin = new BrowserWindow({
            width: 600,
            height: 400,
            show: true,
            webPreferences : {
                nodeIntegration: true,
                contextIsolation : false,
            }
        });

        authWin.loadURL(authorizationUri);

        // Root Endpoint
        const getLogin = async () => {
            try {
                return await Axios.get(authorizationUri);
            } catch (error) {
                console.log("error");
            }
        }
        
        getLogin();
    }

    private ExpressInit = () => {
        expressApp.set('port', expressPort);

        expressApp.listen(expressApp.get('port'), () => {
            console.log(expressApp.get('port'), '번 포트에서 대기중')
        });

        expressApp.get('/login', (req, res) => {
            return res.redirect(authorizationUri)
        });

        expressApp.get('/', (req, res) => {
            const { code } = req.query;
            console.log(code)
            if(code) {
                const requestBody = new URLSearchParams();
                requestBody.append('grant_type', 'authorization_code');
                requestBody.append('code', code);
                requestBody.append('redirect_uri', redirectUri);
                console.log("Getting Token...");
                console.log(requestBody);
                console.log("=========");
                console.log({headers});
                console.log("=========");
                axios.post('https://id.trimble.com/oauth/token', requestBody,  { headers})
                    .then(response => {
                        console.log("Getting Token...");
                        process.env.TOKEN_TYPE = response.data.token_type;
                        process.env.TOKEN_EXPIRESIN = response.data.expires_in;
                        process.env.TOKEN_ACCESSTOKEN = response.data.access_token;
                        process.env.TOKEN_REFRESHTOKEN = response.data.refresh_token;
                        process.env.TOKEN_IDTOKEN = response.data.id_token;
                        res.send('Authroization Completed');
                    })
                    .catch(error => {
                        console.error('Error:', error.message);
                    });
            } else {
                res.send('Authorization code not received.');
            }
        });
        
        expressApp.get('/checktoken', (req, res) => {
            if(process.env.TOKEN_TYPE) {
                res.send('Token saved');
                console.log(`token_type : ${process.env.TOKEN_TYPE}`);
            } else {
                res.send(`No token saved`);
            }  
        })
        
        expressApp.get('/getprojects', (req, res) => {
            if(process.env.TOKEN_ACCESSTOKEN) {
                const headers = {
                    "Authorization" : `Bearer ${process.env.TOKEN_ACCESSTOKEN}`
                }
                axios.get('https://app31.connect.trimble.com/tc/api/2.0/projects', { headers })
                .then(response => {
                    console.log(response)
                    res.send('Loaded Projects');
                })
                .catch(error => {
                    console.error('Error : ', error);
                    res.send('Error receiving while loading projects');
                })
            } else {
                res.send('No valid token exists');
            }
        })
    }
}