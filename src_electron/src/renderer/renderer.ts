import { IpcRenderer, ipcRenderer } from "electron";

export class TCRemoteView {
    private _btnLogin;

    constructor() {
        this._btnLogin = document.querySelector('#trimble-login') as HTMLButtonElement;
        this._oauthTestIpc();
        this._bindEvent();
        
    }

    private _oauthTestIpc() {
        ipcRenderer.on('check-oauth', async (req) => {
            console.log(req);
        })
    }

    private _bindEvent() {
        this._btnLogin.addEventListener('click', () => {
            console.log('Trimble Login');
            ipcRenderer.send('request-login');
        });
    }
}