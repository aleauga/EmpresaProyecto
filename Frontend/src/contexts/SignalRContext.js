// src/contexts/SignalRContext.js
import React, { createContext, useContext, useEffect, useState } from "react";
import { HubConnectionBuilder } from "@microsoft/signalr";

const SignalRContext = createContext(null);

export const useSignalR = () => useContext(SignalRContext);

export const SignalRProvider = ({ userId, children }) => {
  const [connection, setConnection] = useState(null);
  const [processing, setProcessing] = useState(false);

  useEffect(() => {
    const conn = new HubConnectionBuilder()
      .withUrl("http://localhost:5002/notificaciones", { withCredentials: true })
      .withAutomaticReconnect()
      .build();

    conn.on("PagoCompletado", (idSuscripcion) => {
      console.log("Evento recibido:", idSuscripcion);
      //toast.success(`✅ Pago completado para tu suscripción ${idSuscripcion}`);
      setProcessing(false);
    });

    conn.start()
      .then(async () => {
        console.log("Conectado a SignalR (contexto global)");
        await conn.invoke("ClientRegister", userId.toString());
      })
      .catch(err => console.error("Error al conectar:", err));

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