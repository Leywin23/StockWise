import * as signalR from '@microsoft/signalr'

const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:7178/stockHub")
    .withAutomaticReconnect()
    .build();

export default connection;