using System;

namespace TestingDotnetDllCascading
{
    public class ErrorTestService
    {
        public ErrorTestService()
        {
        }

        public ErrorTestService(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentException("Constructor parameter cannot be null or empty", nameof(message));
            }
        }

        /// <summary>
        /// Throws a simple ArgumentException with a specified message
        /// </summary>
        /// <param name="message">The error message to throw</param>
        /// <returns>Never returns, always throws</returns>
        public string ThrowArgumentException(string message)
        {
            throw new ArgumentException(message ?? "Test argument exception from DLL service");
        }

        /// <summary>
        /// Throws a NullReferenceException
        /// </summary>
        /// <returns>Never returns, always throws</returns>
        public string ThrowNullReferenceException()
        {
            throw new NullReferenceException("Test null reference exception from DLL service");
        }

        /// <summary>
        /// Throws an InvalidOperationException with custom message
        /// </summary>
        /// <param name="operation">The operation that failed</param>
        /// <returns>Never returns, always throws</returns>
        public string ThrowInvalidOperationException(string operation)
        {
            throw new InvalidOperationException($"Invalid operation: {operation ?? "Unknown"}");
        }

        /// <summary>
        /// Throws a custom exception based on the error type parameter
        /// </summary>
        /// <param name="errorType">Type of error to throw: "argument", "null", "operation", "generic"</param>
        /// <param name="message">Custom message for the exception</param>
        /// <returns>Never returns, always throws</returns>
        public string ThrowCustomException(string errorType, string message)
        {
            var errorMessage = message ?? "Test exception from DLL service";
            
            switch (errorType?.ToLowerInvariant())
            {
                case "argument":
                    throw new ArgumentException(errorMessage);
                case "null":
                    throw new NullReferenceException(errorMessage);
                case "operation":
                    throw new InvalidOperationException(errorMessage);
                case "generic":
                default:
                    throw new Exception(errorMessage);
            }
        }

        /// <summary>
        /// Method that succeeds - for testing workflows that should not trigger error handling
        /// </summary>
        /// <param name="input">Input value to return</param>
        /// <returns>The input value with "Success: " prefix</returns>
        public string SuccessfulMethod(string input)
        {
            return $"Success: {input ?? "No input provided"}";
        }

        /// <summary>
        /// Method that conditionally throws based on input
        /// </summary>
        /// <param name="shouldThrow">If true, throws an exception; if false, returns success message</param>
        /// <param name="message">Message to use in exception or success response</param>
        /// <returns>Success message if shouldThrow is false</returns>
        public string ConditionalError(bool shouldThrow, string message)
        {
            if (shouldThrow)
            {
                throw new Exception($"Conditional error: {message ?? "Default error message"}");
            }
            
            return $"Conditional success: {message ?? "Default success message"}";
        }

        /// <summary>
        /// Throws an exception after some processing - useful for testing error handling in workflows with multiple steps
        /// </summary>
        /// <param name="processingSteps">Number of processing steps to simulate before throwing</param>
        /// <param name="errorMessage">Error message to throw</param>
        /// <returns>Never returns, always throws after processing</returns>
        public string ProcessThenThrow(int processingSteps, string errorMessage)
        {
            // Simulate some processing
            for (int i = 0; i < Math.Max(1, processingSteps); i++)
            {
                // Simulate work
                var temp = DateTime.Now.Ticks;
            }
            
            throw new Exception($"Error after {processingSteps} processing steps: {errorMessage ?? "Processing failed"}");
        }
    }
}