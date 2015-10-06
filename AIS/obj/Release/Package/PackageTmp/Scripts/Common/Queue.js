$.queueEvent = {
    _timer: null,
    _queue: [],
    add: function (fn, context, time) {
        var setTimer = function (time) {
            $.queueEvent._timer = setTimeout(function () {
                time = $.queueEvent.add();
                if ($.queueEvent._queue.length) {
                    setTimer(time);
                }
            }, time || 2);
        }

        if (fn) {
            $.queueEvent._queue.push([fn, context, time]);
            if ($.queueEvent._queue.length == 1) {
                setTimer(time);
            }
            return;
        }

        var next = $.queueEvent._queue.shift();
        if (!next) {
            return 0;
        }
        next[0].call(next[1] || window);
        return next[2];
    },
    clear: function () {
        clearTimeout($.queueEvent._timer);
        $.queueEvent._queue = [];
    }
};

//#### Example Usage ####
//    $(document).ready(function () {
//        // a lot of li's, lets say 500
//        $('li').each(function () {
//            var self = this, doBind = function () {
//                $(self).bind('click', function () {
//                    alert('Yeah you clicked me');
//                });
//            };
//            $.queue.add(doBind, this);
//        });
//    });
//#######################