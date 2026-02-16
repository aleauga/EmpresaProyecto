import React, { createContext, useContext, useEffect, useState, ReactNode } from "react";
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";

interface SignalRContextType {
  connection: HubConnection | null;
  processing: boolean;
  setProcessing: (processing: boolean) => void;
}

interface SignalRProviderProps {
  userId: string;
  children: ReactNode;
}

const SignalRContext = createContext<SignalRContextType | undefined>(undefined);

export const useSignalR = (): SignalRContextType => {
  const context = useContext(SignalRContext);
  if (!context) {
    throw new Error("useSignalR must be used within a SignalRProvider");
  }
  return context;
};

export const SignalRProvider: React.FC<SignalRProviderProps> = ({ userId, children }) => {
  const [connection, setConnection] = useState<HubConnection | null>(null);
  const [processing, setProcessing] = useState(false);

  useEffect(() => {
    const conn = new HubConnectionBuilder()
      .withUrl("http://localhost:5002/notificaciones", { withCredentials: true })
      .withAutomaticReconnect()
      .build();

    conn.on("PagoCompletado", (idSuscripcion: string) => {
      console.log("Evento recibido:", idSuscripcion);
      setProcessing(false);
    });

    conn.start()
      .then(async () => {
        console.log("Conectado a SignalR (contexto global)");
        await conn.invoke("ClientRegister", userId.toString());
      })
      .catch((err: Error) => console.error("Error al conectar:", err));

    setConnection(conn);

    return () => {
      conn.stop();
    };
  }, [userId]);

  return (
    <SignalRContext.Provider value={{ connection, processing, setProcessing }}>
      {children}
    </SignalRContext.Provider>
  );
};
