import { QueryCache, QueryClient } from '@tanstack/react-query';
import logger from '../shared/utils/logger';

const queryClient = new QueryClient({
  queryCache: new QueryCache({
    onError: (error) => {
      const message = error instanceof Error ? error.message : 'Unknown API error';
      logger.error('API query failed', { error: message });
    }
  })
});

export default queryClient;
