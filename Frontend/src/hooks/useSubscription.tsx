import { useState, useCallback } from "react";
import subscriptionService, {
  SubscriptionPayload,
  SubscriptionResponse,
} from "../services/subscriptionService";

interface UseSubscriptionReturn {
  loading: boolean;
  error: string | null;
  fetchSubscription: (clientId: string) => Promise<SubscriptionResponse>;
  createSubscription: (
    payload: SubscriptionPayload,
  ) => void;
  clearError: () => void;
}

/**
 * Hook personalizado para manejar la lógica de suscripciones
 * Separa la lógica de negocio del componente
 */
export const useSubscription = (): UseSubscriptionReturn => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchSubscription = useCallback(async (clientId: string) => {
    setLoading(true);
    setError(null);
    try {
      const data = await subscriptionService.getClientSubscription(clientId);
      return data;
    } catch (err) {
      const message = err instanceof Error ? err.message : "Error desconocido";
      setError(message);
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  const createSubscription = useCallback(
    async (payload: SubscriptionPayload) => {
      setLoading(true);
      setError(null);
      try {
        const data = await subscriptionService.createSubscription(payload);
        return data;
      } catch (err) {
        const message =
          err instanceof Error ? err.message : "Error desconocido";
        setError(message);
        throw err;
      } finally {
        setLoading(false);
      }
    },
    [],
  );

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  return {
    loading,
    error,
    fetchSubscription,
    createSubscription,
    clearError,
  };
};