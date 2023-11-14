"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || function (mod) {
    if (mod && mod.__esModule) return mod;
    var result = {};
    if (mod != null) for (var k in mod) if (k !== "default" && Object.prototype.hasOwnProperty.call(mod, k)) __createBinding(result, mod, k);
    __setModuleDefault(result, mod);
    return result;
};
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.TCRemote = void 0;
require("dotenv/config");
const { PORT, Client_ID, Client_Secret, App_Name } = process.env;
const electron_1 = require("electron");
const url = __importStar(require("url"));
const path = __importStar(require("path"));
const axios_1 = __importDefault(require("axios"));
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
    "Authorization": `Basic ${basicAuth}`,
    "Content-Type": "application/x-www-form-urlencoded",
    "Accept": "application/json"
};
//Login Endpoint
const authorizationUri = `https://id.trimble.com/oauth/authorize?` +
    `scope=openid+DWGUploader&` +
    `response_type=code&` +
    `redirect_uri=${redirectUri}&` +
    `client_id=${Client_ID}`;
class TCRemote {
    constructor() {
        this._ready = () => {
            console.log(TAG, 'app ready');
            const mainWin = new electron_1.BrowserWindow({
                width: 1280,
                minWidth: 960,
                height: 720,
                minHeight: 640,
                maximizable: true,
                webPreferences: {
                    nodeIntegration: true,
                    contextIsolation: false
                }
            });
            mainWin.loadURL(HTML);
            electron_1.ipcMain.on('request-login', (event) => {
                this.TCAuth();
            });
        };
        this.TCAuth = () => __awaiter(this, void 0, void 0, function* () {
            const authWin = new electron_1.BrowserWindow({
                width: 600,
                height: 400,
                show: true,
                webPreferences: {
                    nodeIntegration: true,
                    contextIsolation: false,
                }
            });
            authWin.loadURL(authorizationUri);
            // Root Endpoint
            const getLogin = () => __awaiter(this, void 0, void 0, function* () {
                try {
                    return yield axios_1.default.get(authorizationUri);
                }
                catch (error) {
                    console.log("error");
                }
            });
            getLogin();
        });
        this.ExpressInit = () => {
            expressApp.set('port', expressPort);
            expressApp.listen(expressApp.get('port'), () => {
                console.log(expressApp.get('port'), '번 포트에서 대기중');
            });
            expressApp.get('/login', (req, res) => {
                return res.redirect(authorizationUri);
            });
            expressApp.get('/', (req, res) => {
                const { code } = req.query;
                console.log(code);
                if (code) {
                    const requestBody = new URLSearchParams();
                    requestBody.append('grant_type', 'authorization_code');
                    requestBody.append('code', code);
                    requestBody.append('redirect_uri', redirectUri);
                    console.log("Getting Token...");
                    console.log(requestBody);
                    console.log("=========");
                    console.log({ headers });
                    console.log("=========");
                    axios.post('https://id.trimble.com/oauth/token', requestBody, { headers })
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
                }
                else {
                    res.send('Authorization code not received.');
                }
            });
            expressApp.get('/checktoken', (req, res) => {
                if (process.env.TOKEN_TYPE) {
                    res.send('Token saved');
                    console.log(`token_type : ${process.env.TOKEN_TYPE}`);
                }
                else {
                    res.send(`No token saved`);
                }
            });
            expressApp.get('/getprojects', (req, res) => {
                if (process.env.TOKEN_ACCESSTOKEN) {
                    const headers = {
                        "Authorization": `Bearer ${process.env.TOKEN_ACCESSTOKEN}`
                    };
                    axios.get('https://app31.connect.trimble.com/tc/api/2.0/projects', { headers })
                        .then(response => {
                        console.log(response);
                        res.send('Loaded Projects');
                    })
                        .catch(error => {
                        console.error('Error : ', error);
                        res.send('Error receiving while loading projects');
                    });
                }
                else {
                    res.send('No valid token exists');
                }
            });
        };
        this.ExpressInit();
        this._app = electron_1.app;
        this._app.on('ready', this._ready);
    }
}
exports.TCRemote = TCRemote;
//# sourceMappingURL=tcremote.js.map