namespace ExceptionHandlingInNET.ExceptionHandler
{
    public class AsyncExceptionExample
    {
        private readonly ILogger<AsyncExceptionExample> _logger;
        public async Task<string> ProcessDataAsync()
        {
            try
            {
                //async operation that might throw
                var result = await SomeAsyncOperation();
                return result;
            }
            catch (HttpRequestException ex)
            {
                //handle specific exception
                _logger.LogError(ex, "HTTP request failed");
                throw new Exception("External service unavailable", ex);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(ex, "Operation was cancelled");
                throw new OperationCanceledException("Operation timed out", ex);
            }
        }

        //fire and forget with proper exception handling
        public void StartBackgroundWork()
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await DoBackgroundWorkAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Background work failed");
                    //handle or report error appropriately
                }
            });
        }

        private async Task<string> DoBackgroundWorkAsync()
        {
            //simulate async operation
            await Task.Delay(1000);
            return "Background process completed successfully";
        }

        private async Task<string> SomeAsyncOperation()
        {
            //simulate async operation
            await Task.Delay(1000);
            return "Operation completed successfully";
        }
    }

}
