import { webSocket as staticWebSocket } from 'rxjs/webSocket';
declare module 'rxjs/internal/Observable' {
    namespace Observable {
        let webSocket: typeof staticWebSocket;
    }
}
