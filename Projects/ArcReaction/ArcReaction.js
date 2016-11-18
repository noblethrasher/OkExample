if (typeof String.prototype.startsWith != 'function') 
    String.prototype.startsWith = function (str)
    {
        return this.slice(0, str.length) == str;
    };


if (typeof String.prototype.endsWith != 'function')
    String.prototype.endsWith = function (str)
    {
        return this.length >= str.length && (this.slice(this.length - str.length) == str);
    };


if (HTMLElement.prototype.addEventListener)
{
    HTMLElement.prototype.addListener = function (name, fn)
    {
        if (name.slice(0, 2).toUpperCase() == 'ON')
            name = name.substring(2);

        this.addEventListener(name, fn);
    }


    HTMLElement.prototype.removeListener = function (name, fn)
    {
        if (name.slice(0, 2).toUpperCase() == 'ON')
            name = name.substring(2);

        this.removeEventListener(name, fn);
    }
}
else
{
    HTMLElement.prototype.attachEvent = function (name, fn)
    {
        if (name.slice(0, 2).toUpperCase() != 'ON')
            name = 'on' + name;

        this.attachEvent(name, fn);
    }

    HTMLElement.prototype.detachEvent = function (name, fn)
    {
        if (name.slice(0, 2).toUpperCase() != 'ON')
            name = 'on' + name;

        this.detachEvent(name, fn);
    }
}

(function ()
{
    Object.defineProperty(Object.prototype, 'constructorName',
        {
            writeable: false,
            configurable: false,
            enumerable: false,

            value: function ()
            {
                var s = this.constructor.toString();
                var len = s.length;
                var state = 0;

                var xs = [];

                for (var i = 0; i < len; i++)
                {
                    var c = s[i];
                    
                    switch (state)
                    {
                        case 0:
                        {
                            if (c == ' ')
                                state = 1;

                            break;
                        }

                        case 1:
                        {
                            if (c == '(')
                            {
                                state = 2;
                            }
                            else
                                xs.push(c);

                            break;
                        }
                    }
                }

                return xs.join('');
            }
        });




    Object.defineProperty(document, 'ready',
    {
        writeable: false,
        configurable: false,
        enumerable: false,

        value: function (fn)
        {
            var cb = function (e)
            {
                if (this.readyState == 'complete')
                    fn(e);

            };


            if (document.addEventListener)
                document.addEventListener('readystatechange', cb);
            else
                document.attachEvent('onreadystatechange', cb);
        }
    });


})();


(function ()
{
    var append = HTMLElement.prototype.appendChild;

    HTMLElement.prototype.appendChild = function (node)
    {
        if (node.constructorName() == 'HTMLElementEx')
            append.call(this, node.elem);
        else
            append.call(this, node);

        return node;
    }

    HTMLElement.prototype.appendChildren = function (xs)
    {
        if (xs.length)
        {
            var len = xs.length;

            for (var i = 0; i < len; i++)
                append.call(this, xs[i]);
        }
        else
        {
            if (xs.constructorName() == 'Enumerator')
            {
                while (xs.moveNext())
                    append.call(this, xs.current());
            }
        }
    }

    var auto_suggest_created = false;

    window.autosuggest =

        function (id, source, sink)
        {
            var input = null;

            if (typeof (id) == 'string')
                input = document.getElementById(id);
            else
                input = id;
            
            var container = document.createElement('span');
            container.style.display = 'inline-block';

            input.parentNode.replaceChild(container, input);

            container.appendChild(input);

            if (typeof (source) == 'string' && source.toUpperCase().startsWith('HTTP'))
            {
                var url = source;

                if (source.endsWith('='))
                {
                    source = function (q, fn)
                    {
                        new Xhr(url + q, 'GET', function (data)
                        {
                            fn(data);
                        }).exec();
                    }
                }
                else
                {
                    source = function (q, fn)
                    {
                        var xhr = new Xhr(url, 'POST', function (data)
                        {
                            fn(data);
                        });

                        xhr[input.name || input.id] = q;

                        xhr.exec();
                    }
                }
            }

            if (!auto_suggest_created)
            {
                var style = document.createElement('style');

                style.appendChild(document.createTextNode('.__autosuggest__ { position : absolute; }'));

                document.head.appendChild(style);
            }

            function createSuggestions()
            {
                var ul = make('div').elem;

                ul.className = '__autosuggest__';

                return ul;
            }

            function got_focus()
            {
                var interval = null;
                var lost_focus = null;
                var old_value = null;
                var ul = null;

                var focused = true;

                var set_focused = function () { focused = true; };

                input.addListener('focus', set_focused);

                function clearSuggestions(node)
                {
                    var old = node.querySelectorAll('.__autosuggest__');

                    console.log(old.length);

                    var len = old.length;

                    for (var i = 0; i < len; i++)
                        container.removeChild(old[i]);
                }

                function maybe_tear_down()
                {
                    focused = false;

                    setTimeout(function ()
                    {
                        if (!focused)
                        {
                            clearInterval(interval);

                            input.removeListener('blur', lost_focus);
                            input.removeListener('focus', set_focused);

                            if (ul && ul.parentNode)
                                ul.parentNode.removeChild(ul);                            

                            input.addListener('focus', got_focus);
                        }

                    }, 10);
                }

                var callback = function ()
                {
                    if (input.value != old_value)
                    {

                        if (input.value.length < 3)
                        {
                            clearSuggestions(container);
                            return;
                        }

                        source(old_value = input.value, function (data)
                        {
                            clearSuggestions(container);

                            var len = data.length;

                            if (len > 0)
                            {
                                container.appendChild(ul = createSuggestions());

                                input.removeListener('focus', got_focus);

                                var xs = [];

                                for (var i = 0; i < len; i++)
                                {
                                    var elem = sink(data[i]);

                                    xs.push(elem);

                                    elem.addListener('blur', maybe_tear_down);

                                    elem.addListener('focus', set_focused);

                                    elem.addListener('keydown', (function (n)
                                    {
                                        return function (e)
                                        {
                                            switch (e.which)
                                            {
                                                case 38:
                                                {
                                                    (n == 0 ? input : xs[(n - 1) % len].elem).focus();
                                                    e.preventDefault();
                                                    break;
                                                }

                                                case 40:
                                                {
                                                    xs[(n + 1) % len].elem.focus();
                                                    e.preventDefault();
                                                    break;
                                                }
                                            }
                                        }

                                    }(i)));

                                    make('li').appendChild(elem).appendTo(ul);
                                }

                                input.addListener('keydown', function (e)
                                {
                                    console.log(xs.length);

                                    if (e.which == 40)
                                    {
                                        xs[0].elem.focus();
                                        e.preventDefault();
                                    }
                                });
                            }                            
                        });
                    }
                };

                interval = setInterval(callback, 200);

                callback();

                input.addListener('blur', maybe_tear_down);
            }

            input.addListener('focus', got_focus);
        }
})();


function Enumerator(move, current)
{
    this.moveNext = function ()
    {
        return move();
    }

    this.current = function ()
    {
        return current();
    }
}

Object.defineProperty(Object.prototype, '$getEnumerator', {
    value: function ()
    {
        if (this.hasOwnProperty('getEnumerator'))
            return this['getEnumerator'];

        if (this.length)
        {
            var len = this.length;

            var len = this.length;
            var curr = -1;

            var that = this;

            var move = function ()
            {
                return curr++ < len;
            }

            var current = function ()
            {
                return that[curr];
            }

            return new Enumerator(move, current);
        }
        else
        {
            var completed = false;
            var that = this;

            return new Enumerator(function () { return !completed; }, function () { completed = true; return that; });
        }
    }, enumerable: false, writeable: true, configurable: true
});

Stack = function (xs)
{
    var __top = null;

    var _count = 0;

    this.count = function () { return _count; }

    this.push = function (x)
    {
        if (__top == null)
            __top = { value: x, next: null }
        else
        {
            __top = { value: x, next: __top }
        }

        _count++;
    }

    this.pop = function ()
    {
        if (__top == null)
            throw 'Stack is empty';
        else
        {
            var cur = __top;

            __top = cur.next;

            _count--;

            return cur.value;
        }
    }

    this.getEnumerator = function ()
    {
        var curr = __top;

        var move = function ()
        {
            return curr != null;
        }

        var current = function ()
        {
            var temp = curr;
            xs
            curr = curr.next;

            return temp.value;
        }

        return new Enumerator(move, current);
    }
}

var Xhr = function (url, method, success, fail, options)
{
    method = method ? method.toUpperCase() : 'POST';

    var xhr = new XMLHttpRequest();
    var that = this;

    var listeners = null;

    this.addListener = function (type, fn)
    {
        listeners = listeners ? listeners : {};

        if (!listeners[type])
            listeners[type] = [];

        listeners[type].push(fn);
    }

    if (success)
    {
        if (success.constructorName() == 'Array')
            listeners = { 'success': success };
        else
            if (typeof (success) == 'function')
                this.onsuccess = success;
            else
                throw "'success' is not a function!";
    }

    this.exec = function (opts)
    {
        opts = opts ? opts : options;

        var async = true;
        var user = null;
        var pass = null;

        if (opts)
        {
            if (opts.async)
                async = opts.async;

            if (opts.user)
                user = opts.user;

            if (opts.pass)
                pass = opts.pass;
        }

        xhr.addEventListener('readystatechange', function (e)
        {
            if (xhr.readyState == 4)
            {
                if (that.onsuccess || (listeners != null && listeners['success']))
                {
                    var fns = that.onsuccess ? [that.onsuccess] : listeners['success'];

                    var len = fns.length;

                    var data = null;

                    var type = xhr.getResponseHeader('Content-Type');

                    if (type && type.length > 0)
                        type = type.split(';')[0];

                    switch (type)
                    {
                        case 'text/xml':
                            {
                                data = xhr.responseXML;
                                break;
                            }

                        case 'text/html':
                            {
                                data = document.createDocumentFragment();
                                var temp = document.createElement('div');

                                temp.innerHTML = xhr.responseText;

                                while (temp.firstChild)
                                    data.appendChild(temp.firstChild);

                                break;
                            }

                        case 'text/json':
                        case 'application/json':
                            {
                                eval('data = ' + xhr.responseText);
                                break;
                            }
                        default:
                            {
                                data = xhr.responseText;
                                break;
                            }
                    }

                    for (var i = 0; i < len; i++)
                        fns[i](data);
                }

                if (that.onfail || (listeners != null && listeners['fail']))
                {

                }
            }
        });

        xhr.open(method, url, async, user, pass);

        var payload = [];

        for (var x in this)
            if (this.hasOwnProperty(x) && x != 'addListener' && x != 'exec')
            {
                var obj = this[x];

                if (obj.constructor.name = 'Array')
                {
                    var len = obj.length;

                    for (var i = 0; i < len; i++)
                        payload.push(x + '=' + encodeURIComponent(obj[x]));
                }
                else
                    payload.push(x + '=' + encodeURIComponent(that[x]));
            }

        payload = payload.length > 0 ? payload.join('&') : null;

        xhr.send(payload);
    }
}

function HTMLElementEx(elem)
{
    this.elem = elem;

    this.appendChild = function (node)
    {
        this.elem.appendChild(node);
        return this;
    },

    this.appendTo = function (node)
    {
        node.appendChild(this.elem);
        return this;
    }

    this.addListener = function (type, fn)
    {
        this.elem.addListener(type, fn);
    }

    this.removeListener = function (type, fn)
    {
        this.elem.removeListener(type, fn);
    }
}

function find(query, scope)
{
    if (scope == null)
        scope = document;

    return scope.querySelectorAll(query);
}

function make(tag)
{
    var elem = document.createElement(tag);

    switch (elem.tagName)
    {
        case 'A':
            {
                if (arguments[1] && typeof (arguments[1]) == 'string')
                    elem.href = arguments[1];

                if (arguments[2] && typeof (arguments[2]) == 'string')
                    elem.innerHTML = arguments[2];

                break;
            }

        case 'INPUT':
        case 'BUTTON':
        case 'SELECT':
        case 'OPTION':
        case 'TEXTAREA':
            {
                if (arguments[1] && typeof (arguments[1]) == 'string')
                    elem.name = arguments[1];

                if (arguments[2] && typeof (arguments[2]) == 'string')
                {
                    switch (elem.tagName)
                    {
                        case 'INPUT':
                        case 'BUTTON':
                        case 'OPTION':
                        case 'TEXTAREA':
                            {
                                elem.value = arguments[2];
                                break;
                            }
                        default:
                            {
                                break;
                            }

                    }
                }

                if (arguments[3] && typeof (arguments[3]) == 'string' && elem.tagName == 'INPUT')
                {
                    elem.type == arguments[3];
                }

                break;
            }
    }

    return new HTMLElementEx(elem);
}

function Animation(obj, prop)
{
    var start = null;
    var end = null;
    var time = null;

    if (arguments.length >= 5)
    {
        start = parseFloat(arguments[2]);
        end = parseFloat(arguments[3]);
        time = parseFloat(arguments[4]);
    }
    else
    {
        if (arguments.length < 3)
            throw 'Not enough arguments';

        start = obj[prop];
        
        end = arguments[2];
        time = arguments[3];
    }

    var xs = [start, end, time]

    for (var i = 0; i < 3; i++)
        if (isNaN(xs[i]))
            throw 'Parameter ' + (i + 3) + ' must be a number.'


    var steps = 0;
    var step = (start - end) / time;
    var interval = null;

    this.start = function ()
    {
        interval = setInterval(function ()
        {
            var x = Math.min(obj[prop], end);
            var y = Math.max(obj[prop], end);

            if (Math.abs(y - x) < 0.01)
            {
                this.stop();
            }
            else
                obj[prop] = (start += step) + 'px'

            console.log(obj[prop]);

        }, 1);
    }

    this.stop = function ()
    {
        if (interval)
        {
            clearInterval(interval);
            interval = null;
        }
    }
}




