domain = require 'domain'

exports.context = (context, currentDomain = domain.active) ->
  throw new Error('no active domain') unless currentDomain?
  currentDomain.__context__ = if context? then context() else {}

exports.cleanup = (cleanup, context = null, currentDomain = domain.active) ->
  context = context or currentDomain.__context__
  cleanup(context) if cleanup? and context?
  currentDomain.__context__ = null if currentDomain?

exports.onError = (err, onError, context = null, currentDomain = domain.active) ->
  context = context or currentDomain.__context__
  onError(err, context) if onError?
  currentDomain.__context__ = null

exports.get = (key, currentDomain = domain.active) ->
  throw new Error('no active domain') unless currentDomain?
  currentDomain.__context__[key]

exports.set = (key, value, currentDomain = domain.active) ->
  throw new Error('no active domain') unless currentDomain?
  currentDomain.__context__[key] = value

exports.run = (options, func) ->
  if not func
    func = options
    options = {}

  {context, cleanup, onError} = options

  currentDomain = options.domain or domain.active
  throw new Error('no active domain') unless currentDomain

  currentDomain.on 'dispose', ->
    exports.cleanup(cleanup, null, currentDomain)

  currentDomain.on 'error', (err) ->
    if onError?
      exports.onError(err, onError, null, currentDomain)
    else
      exports.cleanup(cleanup, null, currentDomain)

  exports.context(context, currentDomain)

  try
    currentDomain.bind(func, true)()
  catch err
    currentDomain.emit 'error', err

  currentDomain

exports.runInNewDomain = (options, func) ->
  if not func
    func = options
    options = {}

  currentDomain = domain.active
  options.domain = domain.create()

  if not options.detach and currentDomain
    currentDomain.add(options.domain)

    options.domain.on 'error', (err) ->
      currentDomain.emit 'error', err

    currentDomain.on 'dispose', ->
      options.domain.dispose()

  exports.run(options, func)

exports.middleware = (context, cleanup) ->
  (req, res, next) ->
    {context, cleanup} = context if typeof context != 'function'
    currentDomain = domain.active

    exports.context(context, currentDomain)

    res.on 'finish', ->
      exports.cleanup(cleanup, null, currentDomain)

    req.__context__ = currentDomain.__context__
    next()

exports.middlewareOnError = (onError) ->
  (err, req, res, next) ->
    {onError} = onError if typeof onError != 'function'
    if onError?
      exports.onError(err, onError, req.__context__)
    else
      exports.cleanup(onError, req.__context__)

    req.__context__ = null
    next(err)
