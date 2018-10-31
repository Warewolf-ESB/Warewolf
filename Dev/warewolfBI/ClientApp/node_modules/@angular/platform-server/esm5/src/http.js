/**
 * @license
 * Copyright Google Inc. All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */
import * as tslib_1 from "tslib";
var xhr2 = require('xhr2');
import { Injectable, Injector } from '@angular/core';
import { BrowserXhr, Http, ReadyState, RequestOptions, XHRBackend, XSRFStrategy } from '@angular/http';
import { HttpHandler, HttpBackend, XhrFactory, ɵHttpInterceptingHandler as HttpInterceptingHandler } from '@angular/common/http';
import { Observable } from 'rxjs';
var isAbsoluteUrl = /^[a-zA-Z\-\+.]+:\/\//;
function validateRequestUrl(url) {
    if (!isAbsoluteUrl.test(url)) {
        throw new Error("URLs requested via Http on the server must be absolute. URL: " + url);
    }
}
var ServerXhr = /** @class */ (function () {
    function ServerXhr() {
    }
    ServerXhr.prototype.build = function () { return new xhr2.XMLHttpRequest(); };
    ServerXhr = tslib_1.__decorate([
        Injectable()
    ], ServerXhr);
    return ServerXhr;
}());
export { ServerXhr };
var ServerXsrfStrategy = /** @class */ (function () {
    function ServerXsrfStrategy() {
    }
    ServerXsrfStrategy.prototype.configureRequest = function (req) { };
    ServerXsrfStrategy = tslib_1.__decorate([
        Injectable()
    ], ServerXsrfStrategy);
    return ServerXsrfStrategy;
}());
export { ServerXsrfStrategy };
var ZoneMacroTaskWrapper = /** @class */ (function () {
    function ZoneMacroTaskWrapper() {
    }
    ZoneMacroTaskWrapper.prototype.wrap = function (request) {
        var _this = this;
        return new Observable(function (observer) {
            var task = null;
            var scheduled = false;
            var sub = null;
            var savedResult = null;
            var savedError = null;
            var scheduleTask = function (_task) {
                task = _task;
                scheduled = true;
                var delegate = _this.delegate(request);
                sub = delegate.subscribe(function (res) { return savedResult = res; }, function (err) {
                    if (!scheduled) {
                        throw new Error('An http observable was completed twice. This shouldn\'t happen, please file a bug.');
                    }
                    savedError = err;
                    scheduled = false;
                    task.invoke();
                }, function () {
                    if (!scheduled) {
                        throw new Error('An http observable was completed twice. This shouldn\'t happen, please file a bug.');
                    }
                    scheduled = false;
                    task.invoke();
                });
            };
            var cancelTask = function (_task) {
                if (!scheduled) {
                    return;
                }
                scheduled = false;
                if (sub) {
                    sub.unsubscribe();
                    sub = null;
                }
            };
            var onComplete = function () {
                if (savedError !== null) {
                    observer.error(savedError);
                }
                else {
                    observer.next(savedResult);
                    observer.complete();
                }
            };
            // MockBackend for Http is synchronous, which means that if scheduleTask is by
            // scheduleMacroTask, the request will hit MockBackend and the response will be
            // sent, causing task.invoke() to be called.
            var _task = Zone.current.scheduleMacroTask('ZoneMacroTaskWrapper.subscribe', onComplete, {}, function () { return null; }, cancelTask);
            scheduleTask(_task);
            return function () {
                if (scheduled && task) {
                    task.zone.cancelTask(task);
                    scheduled = false;
                }
                if (sub) {
                    sub.unsubscribe();
                    sub = null;
                }
            };
        });
    };
    return ZoneMacroTaskWrapper;
}());
export { ZoneMacroTaskWrapper };
var ZoneMacroTaskConnection = /** @class */ (function (_super) {
    tslib_1.__extends(ZoneMacroTaskConnection, _super);
    function ZoneMacroTaskConnection(request, backend) {
        var _this = _super.call(this) || this;
        _this.request = request;
        _this.backend = backend;
        validateRequestUrl(request.url);
        _this.response = _this.wrap(request);
        return _this;
    }
    ZoneMacroTaskConnection.prototype.delegate = function (request) {
        this.lastConnection = this.backend.createConnection(request);
        return this.lastConnection.response;
    };
    Object.defineProperty(ZoneMacroTaskConnection.prototype, "readyState", {
        get: function () {
            return !!this.lastConnection ? this.lastConnection.readyState : ReadyState.Unsent;
        },
        enumerable: true,
        configurable: true
    });
    return ZoneMacroTaskConnection;
}(ZoneMacroTaskWrapper));
export { ZoneMacroTaskConnection };
var ZoneMacroTaskBackend = /** @class */ (function () {
    function ZoneMacroTaskBackend(backend) {
        this.backend = backend;
    }
    ZoneMacroTaskBackend.prototype.createConnection = function (request) {
        return new ZoneMacroTaskConnection(request, this.backend);
    };
    return ZoneMacroTaskBackend;
}());
export { ZoneMacroTaskBackend };
var ZoneClientBackend = /** @class */ (function (_super) {
    tslib_1.__extends(ZoneClientBackend, _super);
    function ZoneClientBackend(backend) {
        var _this = _super.call(this) || this;
        _this.backend = backend;
        return _this;
    }
    ZoneClientBackend.prototype.handle = function (request) { return this.wrap(request); };
    ZoneClientBackend.prototype.delegate = function (request) {
        return this.backend.handle(request);
    };
    return ZoneClientBackend;
}(ZoneMacroTaskWrapper));
export { ZoneClientBackend };
export function httpFactory(xhrBackend, options) {
    var macroBackend = new ZoneMacroTaskBackend(xhrBackend);
    return new Http(macroBackend, options);
}
export function zoneWrappedInterceptingHandler(backend, injector) {
    var realBackend = new HttpInterceptingHandler(backend, injector);
    return new ZoneClientBackend(realBackend);
}
export var SERVER_HTTP_PROVIDERS = [
    { provide: Http, useFactory: httpFactory, deps: [XHRBackend, RequestOptions] },
    { provide: BrowserXhr, useClass: ServerXhr }, { provide: XSRFStrategy, useClass: ServerXsrfStrategy },
    { provide: XhrFactory, useClass: ServerXhr }, {
        provide: HttpHandler,
        useFactory: zoneWrappedInterceptingHandler,
        deps: [HttpBackend, Injector]
    }
];

//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiaHR0cC5qcyIsInNvdXJjZVJvb3QiOiIiLCJzb3VyY2VzIjpbIi4uLy4uLy4uLy4uLy4uLy4uLy4uLy4uLy4uL3BhY2thZ2VzL3BsYXRmb3JtLXNlcnZlci9zcmMvaHR0cC50cyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTs7Ozs7O0dBTUc7O0FBRUgsSUFBTSxJQUFJLEdBQVEsT0FBTyxDQUFDLE1BQU0sQ0FBQyxDQUFDO0FBRWxDLE9BQU8sRUFBQyxVQUFVLEVBQUUsUUFBUSxFQUFrQyxNQUFNLGVBQWUsQ0FBQztBQUNwRixPQUFPLEVBQUMsVUFBVSxFQUFpQyxJQUFJLEVBQUUsVUFBVSxFQUFXLGNBQWMsRUFBWSxVQUFVLEVBQUUsWUFBWSxFQUFDLE1BQU0sZUFBZSxDQUFDO0FBRXZKLE9BQU8sRUFBeUIsV0FBVyxFQUFzQyxXQUFXLEVBQUUsVUFBVSxFQUFFLHdCQUF3QixJQUFJLHVCQUF1QixFQUFDLE1BQU0sc0JBQXNCLENBQUM7QUFFM0wsT0FBTyxFQUFDLFVBQVUsRUFBeUIsTUFBTSxNQUFNLENBQUM7QUFFeEQsSUFBTSxhQUFhLEdBQUcsc0JBQXNCLENBQUM7QUFFN0MsNEJBQTRCLEdBQVc7SUFDckMsSUFBSSxDQUFDLGFBQWEsQ0FBQyxJQUFJLENBQUMsR0FBRyxDQUFDLEVBQUU7UUFDNUIsTUFBTSxJQUFJLEtBQUssQ0FBQyxrRUFBZ0UsR0FBSyxDQUFDLENBQUM7S0FDeEY7QUFDSCxDQUFDO0FBR0Q7SUFBQTtJQUVBLENBQUM7SUFEQyx5QkFBSyxHQUFMLGNBQTBCLE9BQU8sSUFBSSxJQUFJLENBQUMsY0FBYyxFQUFFLENBQUMsQ0FBQyxDQUFDO0lBRGxELFNBQVM7UUFEckIsVUFBVSxFQUFFO09BQ0EsU0FBUyxDQUVyQjtJQUFELGdCQUFDO0NBQUEsQUFGRCxJQUVDO1NBRlksU0FBUztBQUt0QjtJQUFBO0lBRUEsQ0FBQztJQURDLDZDQUFnQixHQUFoQixVQUFpQixHQUFZLElBQVMsQ0FBQztJQUQ1QixrQkFBa0I7UUFEOUIsVUFBVSxF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bnRlbnQiOlsiLyoqXG4gKiBAbGljZW5zZVxuICogQ29weXJpZ2h0IEdvb2dsZSBJbmMuIEFsbCBSaWdodHMgUmVzZXJ2ZWQuXG4gKlxuICogVXNlIG9mIHRoaXMgc291cmNlIGNvZGUgaXMgZ292ZXJuZWQgYnkgYW4gTUlULXN0eWxlIGxpY2Vuc2UgdGhhdCBjYW4gYmVcbiAqIGZvdW5kIGluIHRoZSBMSUNFTlNFIGZpbGUgYXQgaHR0cHM6Ly9hbmd1bGFyLmlvL2xpY2Vuc2VcbiAqL1xuXG5jb25zdCB4aHIyOiBhbnkgPSByZXF1aXJlKCd4aHIyJyk7XG5cbmltcG9ydCB7SW5qZWN0YWJsZSwgSW5qZWN0b3IsIE9wdGlvbmFsLCBQcm92aWRlciwgSW5qZWN0RmxhZ3N9IGZyb20gJ0Bhbmd1bGFyL2NvcmUnO1xuaW1wb3J0IHtCcm93c2VyWGhyLCBDb25uZWN0aW9uLCBDb25uZWN0aW9uQmFja2VuZCwgSHR0cCwgUmVhZHlTdGF0ZSwgUmVxdWVzdCwgUmVxdWVzdE9wdGlvbnMsIFJlc3BvbnNlLCBYSFJCYWNrZW5kLCBYU1JGU3RyYXRlZ3l9IGZyb20gJ0Bhbmd1bGFyL2h0dHAnO1xuXG5pbXBvcnQge0h0dHBFdmVudCwgSHR0cFJlcXVlc3QsIEh0dHBIYW5kbGVyLCBIdHRwSW50ZXJjZXB0b3IsIEhUVFBfSU5URVJDRVBUT1JTLCBIdHRwQmFja2VuZCwgWGhyRmFjdG9yeSwgybVIdHRwSW50ZXJjZXB0aW5nSGFuZGxlciBhcyBIdHRwSW50ZXJjZXB0aW5nSGFuZGxlcn0gZnJvbSAnQGFuZ3VsYXIvY29tbW9uL2h0dHAnO1xuXG5pbXBvcnQge09ic2VydmFibGUsIE9ic2VydmVyLCBTdWJzY3JpcHRpb259IGZyb20gJ3J4anMnO1xuXG5jb25zdCBpc0Fic29sdXRlVXJsID0gL15bYS16QS1aXFwtXFwrLl0rOlxcL1xcLy87XG5cbmZ1bmN0aW9uIHZhbGlkYXRlUmVxdWVzdFVybCh1cmw6IHN0cmluZyk6IHZvaWQge1xuICBpZiAoIWlzQWJzb2x1dGVVcmwudGVzdCh1cmwpKSB7XG4gICAgdGhyb3cgbmV3IEVycm9yKGBVUkxzIHJlcXVlc3RlZCB2aWEgSHR0cCBvbiB0aGUgc2VydmVyIG11c3QgYmUgYWJzb2x1dGUuIFVSTDogJHt1cmx9YCk7XG4gIH1cbn1cblxuQEluamVjdGFibGUoKVxuZXhwb3J0IGNsYXNzIFNlcnZlclhociBpbXBsZW1lbnRzIEJyb3dzZXJYaHIge1xuICBidWlsZCgpOiBYTUxIdHRwUmVxdWVzdCB7IHJldHVybiBuZXcgeGhyMi5YTUxIdHRwUmVxdWVzdCgpOyB9XG59XG5cbkBJbmplY3RhYmxlKClcbmV4cG9ydCBjbGFzcyBTZXJ2ZXJYc3JmU3RyYXRlZ3kgaW1wbGVtZW50cyBYU1JGU3RyYXRlZ3kge1xuICBjb25maWd1cmVSZXF1ZXN0KHJlcTogUmVxdWVzdCk6IHZvaWQge31cbn1cblxuZXhwb3J0IGFic3RyYWN0IGNsYXNzIFpvbmVNYWNyb1Rhc2tXcmFwcGVyPFMsIFI+IHtcbiAgd3JhcChyZXF1ZXN0OiBTKTogT2JzZXJ2YWJsZTxSPiB7XG4gICAgcmV0dXJuIG5ldyBPYnNlcnZhYmxlKChvYnNlcnZlcjogT2JzZXJ2ZXI8Uj4pID0+IHtcbiAgICAgIGxldCB0YXNrOiBUYXNrID0gbnVsbCAhO1xuICAgICAgbGV0IHNjaGVkdWxlZDogYm9vbGVhbiA9IGZhbHNlO1xuICAgICAgbGV0IHN1YjogU3Vic2NyaXB0aW9ufG51bGwgPSBudWxsO1xuICAgICAgbGV0IHNhdmVkUmVzdWx0OiBhbnkgPSBudWxsO1xuICAgICAgbGV0IHNhdmVkRXJyb3I6IGFueSA9IG51bGw7XG5cbiAgICAgIGNvbnN0IHNjaGVkdWxlVGFzayA9IChfdGFzazogVGFzaykgPT4ge1xuICAgICAgICB0YXNrID0gX3Rhc2s7XG4gICAgICAgIHNjaGVkdWxlZCA9IHRydWU7XG5cbiAgICAgICAgY29uc3QgZGVsZWdhdGUgPSB0aGlzLmRlbGVnYXRlKHJlcXVlc3QpO1xuICAgICAgICBzdWIgPSBkZWxlZ2F0ZS5zdWJzY3JpYmUoXG4gICAgICAgICAgICByZXMgPT4gc2F2ZWRSZXN1bHQgPSByZXMsXG4gICAgICAgICAgICBlcnIgPT4ge1xuICAgICAgICAgICAgICBpZiAoIXNjaGVkdWxlZCkge1xuICAgICAgICAgICAgICAgIHRocm93IG5ldyBFcnJvcihcbiAgICAgICAgICAgICAgICAgICAgJ0FuIGh0dHAgb2JzZXJ2YWJsZSB3YXMgY29tcGxldGVkIHR3aWNlLiBUaGlzIHNob3VsZG5cXCd0IGhhcHBlbiwgcGxlYXNlIGZpbGUgYSBidWcuJyk7XG4gICAgICAgICAgICAgIH1cbiAgICAgICAgICAgICAgc2F2ZWRFcnJvciA9IGVycjtcbiAgICAgICAgICAgICAgc2NoZWR1bGVkID0gZmFsc2U7XG4gICAgICAgICAgICAgIHRhc2suaW52b2tlKCk7XG4gICAgICAgICAgICB9LFxuICAgICAgICAgICAgKCkgPT4ge1xuICAgICAgICAgICAgICBpZiAoIXNjaGVkdWxlZCkge1xuICAgICAgICAgICAgICAgIHRocm93IG5ldyBFcnJvcihcbiAgICAgICAgICAgICAgICAgICAgJ0FuIGh0dHAgb2JzZXJ2YWJsZSB3YXMgY29tcGxldGVkIHR3aWNlLiBUaGlzIHNob3VsZG5cXCd0IGhhcHBlbiwgcGxlYXNlIGZpbGUgYSBidWcuJyk7XG4gICAgICAgICAgICAgIH1cbiAgICAgICAgICAgICAgc2NoZWR1bGVkID0gZmFsc2U7XG4gICAgICAgICAgICAgIHRhc2suaW52b2tlKCk7XG4gICAgICAgICAgICB9KTtcbiAgICAgIH07XG5cbiAgICAgIGNvbnN0IGNhbmNlbFRhc2sgPSAoX3Rhc2s6IFRhc2spID0+IHtcbiAgICAgICAgaWYgKCFzY2hlZHVsZWQpIHtcbiAgICAgICAgICByZXR1cm47XG4gICAgICAgIH1cbiAgICAgICAgc2NoZWR1bGVkID0gZmFsc2U7XG4gICAgICAgIGlmIChzdWIpIHtcbiAgICAgICAgICBzdWIudW5zdWJzY3JpYmUoKTtcbiAgICAgICAgICBzdWIgPSBudWxsO1xuICAgICAgICB9XG4gICAgICB9O1xuXG4gICAgICBjb25zdCBvbkNvbXBsZXRlID0gKCkgPT4ge1xuICAgICAgICBpZiAoc2F2ZWRFcnJvciAhPT0gbnVsbCkge1xuICAgICAgICAgIG9ic2VydmVyLmVycm9yKHNhdmVkRXJyb3IpO1xuICAgICAgICB9IGVsc2Uge1xuICAgICAgICAgIG9ic2VydmVyLm5leHQoc2F2ZWRSZXN1bHQpO1xuICAgICAgICAgIG9ic2VydmVyLmNvbXBsZXRlKCk7XG4gICAgICAgIH1cbiAgICAgIH07XG5cbiAgICAgIC8vIE1vY2tCYWNrZW5kIGZvciBIdHRwIGlzIHN5bmNocm9ub3VzLCB3aGljaCBtZWFucyB0aGF0IGlmIHNjaGVkdWxlVGFzayBpcyBieVxuICAgICAgLy8gc2NoZWR1bGVNYWNyb1Rhc2ssIHRoZSByZXF1ZXN0IHdpbGwgaGl0IE1vY2tCYWNrZW5kIGFuZCB0aGUgcmVzcG9uc2Ugd2lsbCBiZVxuICAgICAgLy8gc2VudCwgY2F1c2luZyB0YXNrLmludm9rZSgpIHRvIGJlIGNhbGxlZC5cbiAgICAgIGNvbnN0IF90YXNrID0gWm9uZS5jdXJyZW50LnNjaGVkdWxlTWFjcm9UYXNrKFxuICAgICAgICAgICdab25lTWFjcm9UYXNrV3JhcHBlci5zdWJzY3JpYmUnLCBvbkNvbXBsZXRlLCB7fSwgKCkgPT4gbnVsbCwgY2FuY2VsVGFzayk7XG4gICAgICBzY2hlZHVsZVRhc2soX3Rhc2spO1xuXG4gICAgICByZXR1cm4gKCkgPT4ge1xuICAgICAgICBpZiAoc2NoZWR1bGVkICYmIHRhc2spIHtcbiAgICAgICAgICB0YXNrLnpvbmUuY2FuY2VsVGFzayh0YXNrKTtcbiAgICAgICAgICBzY2hlZHVsZWQgPSBmYWxzZTtcbiAgICAgICAgfVxuICAgICAgICBpZiAoc3ViKSB7XG4gICAgICAgICAgc3ViLnVuc3Vic2NyaWJlKCk7XG4gICAgICAgICAgc3ViID0gbnVsbDtcbiAgICAgICAgfVxuICAgICAgfTtcbiAgICB9KTtcbiAgfVxuXG4gIHByb3RlY3RlZCBhYnN0cmFjdCBkZWxlZ2F0ZShyZXF1ZXN0OiBTKTogT2JzZXJ2YWJsZTxSPjtcbn1cblxuZXhwb3J0IGNsYXNzIFpvbmVNYWNyb1Rhc2tDb25uZWN0aW9uIGV4dGVuZHMgWm9uZU1hY3JvVGFza1dyYXBwZXI8UmVxdWVzdCwgUmVzcG9uc2U+IGltcGxlbWVudHNcbiAgICBDb25uZWN0aW9uIHtcbiAgcmVzcG9uc2U6IE9ic2VydmFibGU8UmVzcG9uc2U+O1xuICAvLyBUT0RPKGlzc3VlLzI0NTcxKTogcmVtb3ZlICchJy5cbiAgbGFzdENvbm5lY3Rpb24gITogQ29ubmVjdGlvbjtcblxuICBjb25zdHJ1Y3RvcihwdWJsaWMgcmVxdWVzdDogUmVxdWVzdCwgcHJpdmF0ZSBiYWNrZW5kOiBYSFJCYWNrZW5kKSB7XG4gICAgc3VwZXIoKTtcbiAgICB2YWxpZGF0ZVJlcXVlc3RVcmwocmVxdWVzdC51cmwpO1xuICAgIHRoaXMucmVzcG9uc2UgPSB0aGlzLndyYXAocmVxdWVzdCk7XG4gIH1cblxuICBkZWxlZ2F0ZShyZXF1ZXN0OiBSZXF1ZXN0KTogT2JzZXJ2YWJsZTxSZXNwb25zZT4ge1xuICAgIHRoaXMubGFzdENvbm5lY3Rpb24gPSB0aGlzLmJhY2tlbmQuY3JlYXRlQ29ubmVjdGlvbihyZXF1ZXN0KTtcbiAgICByZXR1cm4gdGhpcy5sYXN0Q29ubmVjdGlvbi5yZXNwb25zZSBhcyBPYnNlcnZhYmxlPFJlc3BvbnNlPjtcbiAgfVxuXG4gIGdldCByZWFkeVN0YXRlKCk6IFJlYWR5U3RhdGUge1xuICAgIHJldHVybiAhIXRoaXMubGFzdENvbm5lY3Rpb24gPyB0aGlzLmxhc3RDb25uZWN0aW9uLnJlYWR5U3RhdGUgOiBSZWFkeVN0YXRlLlVuc2VudDtcbiAgfVxufVxuXG5leHBvcnQgY2xhc3MgWm9uZU1hY3JvVGFza0JhY2tlbmQgaW1wbGVtZW50cyBDb25uZWN0aW9uQmFja2VuZCB7XG4gIGNvbnN0cnVjdG9yKHByaXZhdGUgYmFja2VuZDogWEhSQmFja2VuZCkge31cblxuICBjcmVhdGVDb25uZWN0aW9uKHJlcXVlc3Q6IGFueSk6IFpvbmVNYWNyb1Rhc2tDb25uZWN0aW9uIHtcbiAgICByZXR1cm4gbmV3IFpvbmVNYWNyb1Rhc2tDb25uZWN0aW9uKHJlcXVlc3QsIHRoaXMuYmFja2VuZCk7XG4gIH1cbn1cblxuZXhwb3J0IGNsYXNzIFpvbmVDbGllbnRCYWNrZW5kIGV4dGVuZHNcbiAgICBab25lTWFjcm9UYXNrV3JhcHBlcjxIdHRwUmVxdWVzdDxhbnk+LCBIdHRwRXZlbnQ8YW55Pj4gaW1wbGVtZW50cyBIdHRwQmFja2VuZCB7XG4gIGNvbnN0cnVjdG9yKHByaXZhdGUgYmFja2VuZDogSHR0cEJhY2tlbmQpIHsgc3VwZXIoKTsgfVxuXG4gIGhhbmRsZShyZXF1ZXN0OiBIdHRwUmVxdWVzdDxhbnk+KTogT2JzZXJ2YWJsZTxIdHRwRXZlbnQ8YW55Pj4geyByZXR1cm4gdGhpcy53cmFwKHJlcXVlc3QpOyB9XG5cbiAgcHJvdGVjdGVkIGRlbGVnYXRlKHJlcXVlc3Q6IEh0dHBSZXF1ZXN0PGFueT4pOiBPYnNlcnZhYmxlPEh0dHBFdmVudDxhbnk+PiB7XG4gICAgcmV0dXJuIHRoaXMuYmFja2VuZC5oYW5kbGUocmVxdWVzdCk7XG4gIH1cbn1cblxuZXhwb3J0IGZ1bmN0aW9uIGh0dHBGYWN0b3J5KHhockJhY2tlbmQ6IFhIUkJhY2tlbmQsIG9wdGlvbnM6IFJlcXVlc3RPcHRpb25zKSB7XG4gIGNvbnN0IG1hY3JvQmFja2VuZCA9IG5ldyBab25lTWFjcm9UYXNrQmFja2VuZCh4aHJCYWNrZW5kKTtcbiAgcmV0dXJuIG5ldyBIdHRwKG1hY3JvQmFja2VuZCwgb3B0aW9ucyk7XG59XG5cbmV4cG9ydCBmdW5jdGlvbiB6b25lV3JhcHBlZEludGVyY2VwdGluZ0hhbmRsZXIoYmFja2VuZDogSHR0cEJhY2tlbmQsIGluamVjdG9yOiBJbmplY3Rvcikge1xuICBjb25zdCByZWFsQmFja2VuZDogSHR0cEJhY2tlbmQgPSBuZXcgSHR0cEludGVyY2VwdGluZ0hhbmRsZXIoYmFja2VuZCwgaW5qZWN0b3IpO1xuICByZXR1cm4gbmV3IFpvbmVDbGllbnRCYWNrZW5kKHJlYWxCYWNrZW5kKTtcbn1cblxuZXhwb3J0IGNvbnN0IFNFUlZFUl9IVFRQX1BST1ZJREVSUzogUHJvdmlkZXJbXSA9IFtcbiAge3Byb3ZpZGU6IEh0dHAsIHVzZUZhY3Rvcnk6IGh0dHBGYWN0b3J5LCBkZXBzOiBbWEhSQmFja2VuZCwgUmVxdWVzdE9wdGlvbnNdfSxcbiAge3Byb3ZpZGU6IEJyb3dzZXJYaHIsIHVzZUNsYXNzOiBTZXJ2ZXJYaHJ9LCB7cHJvdmlkZTogWFNSRlN0cmF0ZWd5LCB1c2VDbGFzczogU2VydmVyWHNyZlN0cmF0ZWd5fSxcbiAge3Byb3ZpZGU6IFhockZhY3RvcnksIHVzZUNsYXNzOiBTZXJ2ZXJYaHJ9LCB7XG4gICAgcHJvdmlkZTogSHR0cEhhbmRsZXIsXG4gICAgdXNlRmFjdG9yeTogem9uZVdyYXBwZWRJbnRlcmNlcHRpbmdIYW5kbGVyLFxuICAgIGRlcHM6IFtIdHRwQmFja2VuZCwgSW5qZWN0b3JdXG4gIH1cbl07XG4iXX0=