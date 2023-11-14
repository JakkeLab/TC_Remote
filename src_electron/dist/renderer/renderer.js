"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.TCRemoteView = void 0;
const electron_1 = require("electron");
class TCRemoteView {
    constructor() {
        this._btnLogin = document.querySelector('#trimble-login');
        this._oauthTestIpc();
        this._bindEvent();
    }
    _oauthTestIpc() {
        electron_1.ipcRenderer.on('check-oauth', (req) => __awaiter(this, void 0, void 0, function* () {
            console.log(req);
        }));
    }
    _bindEvent() {
        this._btnLogin.addEventListener('click', () => {
            console.log('Trimble Login');
            electron_1.ipcRenderer.send('request-login');
        });
    }
}
exports.TCRemoteView = TCRemoteView;
//# sourceMappingURL=renderer.js.map