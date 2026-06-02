@EasyAuthMiddleware
@mstest:donotparallelize
Feature: EasyAuth Redirect Middleware
    In order to protect Warewolf workflows from unauthenticated access
    As a Warewolf platform operator
    I want the EasyAuth redirect middleware to enforce access control at the edge

    Background:
        Given the lightweight Warewolf server is running

    Scenario: Public route is accessible without any credentials
        When an API client requests "/public/apis.json" with no authentication
        Then the HTTP response status code should be 200

    Scenario: The apis.json discovery endpoint on the secure route requires authentication
        When an API client requests "/secure/apis.json" with an Easy Auth principal header
        Then the HTTP response status code should be 200

    Scenario: The apis.json discovery endpoint on the services route requires authentication
        When an API client requests "/services/apis.json" with an Easy Auth principal header
        Then the HTTP response status code should be 200

    @EasyAuthUnauthenticated
    Scenario: Secure route returns 401 and a JSON error body for unauthenticated API clients
        When an API client requests "/secure/ProtectedWorkflow" with no authentication
        Then the HTTP response status code should be 401
        And the response body should contain "unauthorized"
        And the response should include a "WWW-Authenticate" header

    @EasyAuthUnauthenticated
    Scenario: Services route returns 401 for unauthenticated API clients
        When an API client requests "/services/ProtectedWorkflow" with no authentication
        Then the HTTP response status code should be 401
        And the response body should contain "unauthorized"

    @EasyAuthBrowserRedirect
    Scenario: Browser navigation to a secure route without authentication redirects to the AAD login page
        When a browser navigates to "/secure/ProtectedWorkflow" without authentication
        Then the HTTP response status code should be 302
        And the "Location" header should contain "/.auth/login/aad"
        And the "Location" header should contain "post_login_redirect_uri"

    @EasyAuthAuthenticated
    Scenario: Secure route passes through to the next middleware when a valid Bearer token is present
        When an API client requests "/secure/apis.json" with a Bearer token
        Then the HTTP response status code should not be 401

    @EasyAuthAuthenticated
    Scenario: Secure route passes through to the next middleware when an Easy Auth principal header is present
        When an API client requests "/secure/apis.json" with an Easy Auth principal header
        Then the HTTP response status code should not be 401
