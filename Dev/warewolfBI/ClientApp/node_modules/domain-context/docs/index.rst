domain-context
==============

Globally accessible domain-bound contexts, connect/express middleware included


This module uses Node.js domains_ to store and query data, thus mimicing
thread-local storage in other languages (e.g. in Python).

.. _domains: http://nodejs.org/api/domain.html

Getting started
---------------

Install ``domain-context`` with ``npm``::

  % npm install domain-context

And ``require('domain-context')`` in your code.

Rationale
---------

You need to have an access to some data inside your functions — usually you
would pass such data as an argument.

But when you need to have an access "everywhere", e.g. access to a currently
active database connection through the duration of HTTP request, you probably
don't want to "poison" your function definitions and function calls with
boilerplate arguments.

That the place where ``domain-context`` library comes handy. It allows basically
to database connection in the currently active domain and automatically perform
cleanup actions on the domain's disposal. Your code could have access to the
stored database connection as long as it is running inside the domain.

Connect/express middleware
--------------------------

The middleware can only be used inside an active domain, use
connect-domain_ middleware to create one::

    var connectDomain = require('connect-domain'),
        domainContext = require('domain-context'),
        express = require('express');

    var lifecycle = {
      context: function() {
        return {db: new pg.Client(...)}
      },
      cleanup: function(context) {
        context.db.query('commit');
        context.db.end();
      },
      onError: function(err, context) {
        context.db.query('rollback');
        context.db.end();
      }
    };

    app = express();
    app.use(connectDomain());
    app.use(domainContext.middleware(lifecycle));
    // your applicaiton's middleware goes here
    app.use(domainContext.middlewareOnError(lifecycle));

Note that because of connect/express design you are required to place two
middlewares around your application — ``domainContext.middleware()`` and
``domainContext.middlewareOnError()``.

Now you can use ``domainContext.get()`` to query data from the currently active
domain::

    var domainContext = require('connect-reqcontext');

    function getUserById(id, cb) {
      domainContext.get('db').query("select ...", cb);
    }

.. _connect-domain: https://github.com/baryshev/connect-domain
