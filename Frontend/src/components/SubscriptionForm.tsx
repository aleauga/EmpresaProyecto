import React, {
  useState,
  ChangeEvent,
  FormEvent,
  useEffect,
  useCallback,
} from "react";
import CryptoJS from "crypto-js";
import InputMask from "react-input-mask";
import "./SubscriptionForm.css";
import { useSignalR } from "../contexts/SignalRContext";
import { useSubscription } from "../hooks/useSubscription";
import { SubscriptionPayload, SubscriptionResponse } from "../services/subscriptionService";

interface FormData {
  Nombre: string;
  ApellidoPaterno: string;
  ApellidoMaterno: string;
  Correo: string;
  Telefono: string;
  Plan: "Basic" | "Premium";
  NumeroTarjeta: string;
  Expiracion: string;
  Cvv: string;
  IdCliente: string;
}

interface SubscriptionFormProps {
  userId: string;
}

interface FormErrors {
  [key: string]: string;
}

interface SubscriptionData {
  Nombre: string;
  Plan: "Basic" | "Premium";
  Estado: string;
}

function SubscriptionForm({ userId }: SubscriptionFormProps) {
  const { processing, setProcessing } = useSignalR();
  const {
    error: apiError,
    fetchSubscription,
    createSubscription,
  } = useSubscription();
  const [subscriptionData, setSubscriptionData] = useState<SubscriptionData | null>(null);


  const [formData, setFormData] = useState<FormData>({
    Nombre: "",
    ApellidoPaterno: "",
    ApellidoMaterno: "",
    Correo: "",
    Telefono: "",
    Plan: "Basic",
    NumeroTarjeta: "",
    Expiracion: "",
    Cvv: "",
    IdCliente: userId,
  });

  const [errors, setErrors] = useState<FormErrors>({});
  const [submitted, setSubmitted] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

const fetchSubscriptionData = useCallback(async () => {
  try {
    const response: SubscriptionResponse = await fetchSubscription(formData.IdCliente);
    console.log("Datos de suscripción:", response);

    // Mapear al tipo que espera el frontend
    const mapped: SubscriptionData = {
      Nombre: response.nombre ,
      Plan: response.plan ,
      Estado: response.estado 
    };

    setSubscriptionData(mapped);
  } catch (error) {
    console.error("Error en la petición:", error);
  }
}, [formData.IdCliente, fetchSubscription]);



  useEffect(() => {
    if (!processing && submitted) {
      fetchSubscriptionData();
    }
  }, [processing, submitted, fetchSubscriptionData]);

  const validateField = (name: string, value: string): string => {
    let error = "";
    switch (name) {
      case "Correo":
        const emailRegex = /^[a-zA-Z0-9._%-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
        if (!emailRegex.test(value)) error = "Correo inválido";
        break;
      case "Telefono":
        if (value.replace(/\D/g, "").length !== 10)
          error = "Teléfono debe tener 10 dígitos";
        break;
      case "NumeroTarjeta":
        const digits = value.replace(/\D/g, "");
        if (digits.length < 13 || digits.length > 19)
          error = "Tarjeta debe tener entre 13 y 19 dígitos";
        break;
      case "Expiracion":
        const expiryRegex = /^(0[1-9]|1[0-2])\/\d{2}$/;
        if (!expiryRegex.test(value)) error = "Formato inválido (MM/AA)";
        break;
      case "Cvv":
        const cvvRegex = /^[0-9]{3,4}$/;
        if (!cvvRegex.test(value)) error = "CVV debe tener 3 o 4 dígitos";
        break;
      default:
        if (!value.trim()) error = "Campo obligatorio";
        break;
    }
    return error;
  };

  const handleChange = (
    e: ChangeEvent<HTMLInputElement | HTMLSelectElement>,
  ) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));

    const error = validateField(name, value);
    setErrors((prev) => {
      const newErrors = { ...prev, [name]: error };
      if (prev.general) {
        delete newErrors.general;
      }
      return newErrors;
    });
  };

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    const requiredFields: (keyof FormData)[] = [
      "Nombre",
      "ApellidoPaterno",
      "ApellidoMaterno",
      "Correo",
      "Telefono",
      "NumeroTarjeta",
      "Expiracion",
      "Cvv",
    ];

    const hasFieldErrors = requiredFields.some((field) => errors[field]);
    if (hasFieldErrors) return;

    setIsSubmitting(true);
    setErrors((prev) => {
      const { general, ...rest } = prev;
      return rest;
    });

    const publicKey = "mi-clave-secreta";
    const encryptedCard = CryptoJS.AES.encrypt(
      formData.NumeroTarjeta,
      publicKey,
    ).toString();
    const encryptedExpiry = CryptoJS.AES.encrypt(
      formData.Expiracion,
      publicKey,
    ).toString();
    const encryptedCvv = CryptoJS.AES.encrypt(
      formData.Cvv,
      publicKey,
    ).toString();

    const payload: SubscriptionPayload = {
      ...formData,
      Tarjeta: {
        NumeroTarjeta: encryptedCard,
        Expiracion: encryptedExpiry,
        Cvv: encryptedCvv,
      },
    };

    try {
      await createSubscription(payload);
      setSubmitted(true);
      setErrors({});
      setProcessing(true);
    } catch (error) {
      setErrors((prev) => ({
        ...prev,
        general: apiError || "Error al registrar la suscripción",
      }));
      setSubmitted(false);
    } finally {
      setIsSubmitting(false);
    }
  };

  const getInputClass = (fieldName: keyof FormData): string => {
    if (errors[fieldName]) return "input-error";
    if (formData[fieldName] && !errors[fieldName]) return "input-valid";
    return "";
  };

  const isFormValid = (): boolean => {
    const requiredFields: (keyof FormData)[] = [
      "Nombre",
      "ApellidoPaterno",
      "ApellidoMaterno",
      "Correo",
      "Telefono",
      "NumeroTarjeta",
      "Expiracion",
      "Cvv",
    ];

    const allFieldsFilled = requiredFields.every(
      (field) => formData[field].toString().trim() !== "",
    );
    const noFieldErrors = requiredFields.every((field) => !errors[field]);

    return allFieldsFilled && noFieldErrors;
  };

  return (
    <div className="form-container">
      {!submitted ? (
        <form onSubmit={handleSubmit} className="subscription-form">
          <label>Nombre</label>
          <input
            type="text"
            name="Nombre"
            value={formData.Nombre}
            onChange={handleChange}
            className={getInputClass("Nombre")}
          />
          {errors.Nombre && <p className="error">{errors.Nombre}</p>}

          <label>Apellido Paterno</label>
          <input
            type="text"
            name="ApellidoPaterno"
            value={formData.ApellidoPaterno}
            onChange={handleChange}
            className={getInputClass("ApellidoPaterno")}
          />
          {errors.ApellidoPaterno && (
            <p className="error">{errors.ApellidoPaterno}</p>
          )}

          <label>Apellido Materno</label>
          <input
            type="text"
            name="ApellidoMaterno"
            value={formData.ApellidoMaterno}
            onChange={handleChange}
            className={getInputClass("ApellidoMaterno")}
          />
          {errors.ApellidoMaterno && (
            <p className="error">{errors.ApellidoMaterno}</p>
          )}

          <label>Correo</label>
          <input
            type="email"
            name="Correo"
            value={formData.Correo}
            onChange={handleChange}
            className={getInputClass("Correo")}
          />
          {errors.Correo && <p className="error">{errors.Correo}</p>}

          <label>Teléfono</label>
          <InputMask
            mask="999-999-9999"
            value={formData.Telefono}
            onChange={handleChange}
          >
            {(inputProps: any) => (
              <input
                {...inputProps}
                type="text"
                name="Telefono"
                placeholder="555-123-4567"
                className={getInputClass("Telefono")}
              />
            )}
          </InputMask>
          {errors.Telefono && <p className="error">{errors.Telefono}</p>}

          <label>Plan</label>
          <select name="Plan" value={formData.Plan} onChange={handleChange}>
            <option value="Basic">Básico</option>
            <option value="Premium">Premium</option>
          </select>

          <h3>Método de Pago</h3>

          <label>Número de tarjeta</label>
          <InputMask
            mask="9999 9999 9999 9999"
            value={formData.NumeroTarjeta}
            onChange={handleChange}
          >
            {(inputProps: any) => (
              <input
                {...inputProps}
                type="text"
                name="NumeroTarjeta"
                placeholder="1234 5678 9012 3456"
                className={getInputClass("NumeroTarjeta")}
              />
            )}
          </InputMask>
          {errors.NumeroTarjeta && (
            <p className="error">{errors.NumeroTarjeta}</p>
          )}

          <label>Expiración</label>
          <InputMask
            mask="99/99"
            value={formData.Expiracion}
            onChange={handleChange}
          >
            {(inputProps: any) => (
              <input
                {...inputProps}
                type="text"
                name="Expiracion"
                placeholder="MM/AA"
                className={getInputClass("Expiracion")}
              />
            )}
          </InputMask>
          {errors.Expiracion && <p className="error">{errors.Expiracion}</p>}

          <label>CVV</label>
          <input
            type="text"
            name="Cvv"
            value={formData.Cvv}
            onChange={handleChange}
            placeholder="CVV (3-4 dígitos)"
            maxLength={4}
            inputMode="numeric"
            className={getInputClass("Cvv")}
          />
          {errors.Cvv && <p className="error">{errors.Cvv}</p>}

          {errors.general && <p className="error">{errors.general}</p>}

          <button type="submit" disabled={isSubmitting || !isFormValid()}>
            {isSubmitting ? (
              <span
                style={{ display: "flex", alignItems: "center", gap: "8px" }}
              >
                <span className="spinner"></span>
                Procesando...
              </span>
            ) : (
              "Suscribirse"
            )}
          </button>
        </form>
      ) : (
       <div className="confirmation">
          <table className="confirmation-table">
            <thead>
              <tr>
                <th>Nombre</th>
                <th>Plan</th>
                <th>Estatus</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td>{subscriptionData ? subscriptionData.Nombre : formData.Nombre}</td>
                <td>
                  {subscriptionData
                    ? subscriptionData.Plan
                    : formData.Plan === "Basic"
                      ? "Básico"
                      : "Premium"}
                </td>
                <td
                  className={
                    subscriptionData
                      ? subscriptionData.Estado === "Pendiente"
                        ? "status-pending"
                        : subscriptionData.Estado === "Active"
                        ? "status-paid"
                        : "status-error"
                      : "status-pending"
                  }
                >
                  {subscriptionData ? subscriptionData.Estado : "Pendiente"}
                </td>
              </tr>
            </tbody>
          </table>
        </div>




      )}
    </div>
  );
}

export default SubscriptionForm;