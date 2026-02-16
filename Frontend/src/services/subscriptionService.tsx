const API_URL = process.env.REACT_APP_API_URL || "http://localhost:5000";

export interface SubscriptionPayload {
  Nombre: string;
  ApellidoPaterno: string;
  ApellidoMaterno: string;
  Correo: string;
  Telefono: string;
  Plan: "Basic" | "Premium";
  Tarjeta: {
    NumeroTarjeta: string;
    Expiracion: string;
    Cvv: string;
  };
  IdCliente: string;
}

export interface SubscriptionResponse {
  // Define según tu API
  [key: string]: any;
}

class SubscriptionService {
  /**
   * Obtiene los datos de suscripción de un cliente
   */
  async getClientSubscription(clientId: string): Promise<SubscriptionResponse> {
    try {
      const response = await fetch(`${API_URL}/client/${clientId}/subscription`);

      if (!response.ok) {
        throw new Error(
          `Error ${response.status}: No se pudo obtener la suscripción`,
        );
      }

      return await response.json();
    } catch (error) {
      console.error("Error fetching subscription:", error);
      throw error; 
  }
 }

  /**
   * Crea una nueva suscripción
   */
  async createSubscription(
    payload: SubscriptionPayload,
  ) {
    try {
      const response = await fetch(`${API_URL}/client/subscription`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(payload),
      });

      if (!response.ok) {
        throw new Error(
          `Error ${response.status}: No se pudo crear la suscripción`,
        );
      }

    } catch (error) {
      console.error("Error creating subscription:", error);
      throw error;
    }
  }
}

export default new SubscriptionService();