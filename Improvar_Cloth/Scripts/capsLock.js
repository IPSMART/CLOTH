

var on = (function (attachEvent) {
    return window.addEventListener || attachEvent;
}(function (event, listener) {
    this.attachEvent("on" + event, listener);
}));

var done = (function (detachEvent) {
    return window.removeEventListener || detachEvent;
}(function (event, listener) {
    this.detachEvent("on" + event, listener);
}));

var capsLock = (function (status, observers, apple, on, done) {
    var capsLock = typeof exports === "object" ? exports : {};

    capsLock.status = status;

    capsLock.observe = function (observer) {
        observers.push(observer);
    };

    on("focus", function () {
        on("keypress", correctStatus);
    });

    if (apple) {
        on("keydown", function (event) {
            var e = event || window.event;
            if (e.keyCode === 20) setStatus(true);
        });

        on("keyup", function (event) {
            var e = event || window.event;
            if (e.keyCode === 20) setStatus(false);
        });
    } else {
        var pressed = false;

        on("keydown", function (event) {
            var e = event || window.event;

            if (!pressed && e.keyCode === 20) {
                setStatus(!status);
                pressed = true;
            }
        });

        on("keyup", function (event) {
            var e = event || window.event;
            if (pressed && e.keyCode === 20) {
                setStatus(!status);
                pressed = false;
            }
        });
    }

    return capsLock;

    function correctStatus(event) {
        var e = event || window.event;
        var code = e.charCode || e.keyCode;
        var shift = e.shiftKey;

        if (code < 123 && code > 96) {
            done("keypress", correctStatus);
            setStatus(shift);
        } else if (code < 91 && code > 64 && !(apple && shift)) {
            done("keypress", correctStatus);
            setStatus(!shift);
        }
    }

    function setStatus(flag) {
        if (status !== flag) {
            var length = observers.length, index = 0;
            while (index < length) observers[index++](flag);
            capsLock.status = status = flag;
        }
    }
}(false, [], /Mac|iPad|iPhone|iPod/.test(navigator.platform), on, done));