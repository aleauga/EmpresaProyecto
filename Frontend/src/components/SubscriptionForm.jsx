import React, { useState, useEffect } from "react";
import CryptoJS from "crypto-js";
import InputMask from "react-input-mask";
import "./SubscriptionForm.css";
import { useSignalR } from "../contexts/SignalRContext";


function SubscriptionForm({ userId  }) {
  const { connection, processing, setProcessing } = useSignalR();

  const [formData, setFormData] = useState({
    Nombre: "",
    ApellidoPaterno: "",
    ApellidoMaterno: "",
    Correo: "",
    Telefono: "",
    Plan: "Basic",
    NumeroTarjeta: "",
    Expiracion: "",
    Cvv: "",
    IdCliente:userId
  });
  const [errors, setErrors] = useState({});
  const [submitted, setSubmitted] = useState(false);

  const validateField = (name, value) => {
    let error = "";
    switch (name) {
      case "Correo":
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(value)) error = "Correo inválido";
        break;
      case "Telefono":
        if (value.replace(/\D/g, "").length !== 10) error = "Teléfono debe tener 10 dígitos";
        break;
      case "NumeroTarjeta":
        const digits = value.replace(/\D/g, "");
        if (digits.length < 13 || digits.length > 19) error = "Tarjeta debe tener entre 13 y 19 dígitos";
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

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));

    const error = validateField(name, value);
    setErrors((prev) => {
      const newErrors = { ...prev, [name]: error };
      // 🔹 Si había un error general, lo quitamos al modificar cualquier campo
      if (prev.general) {
        delete newErrors.general;
      }
      return newErrors;
    });
  };
 
  const handleSubmit = async (e) => {
    e.preventDefault();
    // 🔹 Limpiar error general antes de validar
    setErrors((prev) => {
      const { general, ...rest } = prev;
      return rest;
    });

    const hasErrors = Object.values(errors).some((err) => err);
    if (hasErrors) return;

    const secretKey = "mi-clave-secreta";
    const encryptedCard = CryptoJS.AES.encrypt(formData.NumeroTarjeta, secretKey).toString();
    const encryptedExpiry = CryptoJS.AES.encrypt(formData.Expiracion, secretKey).toString();
    const encryptedCvv = CryptoJS.AES.encrypt(formData.Cvv, secretKey).toString();

    const payload = {
      ...formData,
      Tarjeta: {
        NumeroTarjeta: encryptedCard,
        Expiracion: encryptedExpiry,
        Cvv: encryptedCvv
      }
    };

    try {
      const response = await fetch("http://localhost:5000/client/subscription", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload)
      });

      if (response.ok) {
        setSubmitted(true); // 🔹 Solo en éxito
        setErrors({});
        setProcessing(true);
      } else {
        setErrors((prev) => ({ ...prev, general: "Error al registrar la suscripción" }));
        setSubmitted(false); // 🔹 Mantener formulario visible
      }
    } catch {
      setErrors((prev) => ({ ...prev, general: "No se pudo conectar con el servidor" }));
      setSubmitted(false); // 🔹 Mantener formulario visible
    }
  };

  const getInputClass = (fieldName) => {
    if (errors[fieldName]) return "input-error";
    if (formData[fieldName] && !errors[fieldName]) return "input-valid";
    return "";
  };

  return (
    <div className="form-container">
      {!submitted ? (
        <form onSubmit={handleSubmit} className="subscription-form">
          <label>Nombre</label>
          <input type="text" name="Nombre" value={formData.Nombre} onChange={handleChange} className={getInputClass("Nombre")} />
          {errors.Nombre && <p className="error">{errors.Nombre}</p>}

          <label>Apellido Paterno</label>
          <input type="text" name="ApellidoPaterno" value={formData.ApellidoPaterno} onChange={handleChange} className={getInputClass("ApellidoPaterno")} />
          {errors.ApellidoPaterno && <p className="error">{errors.ApellidoPaterno}</p>}

          <label>Apellido Materno</label>
          <input type="text" name="ApellidoMaterno" value={formData.ApellidoMaterno} onChange={handleChange} className={getInputClass("ApellidoMaterno")} />
          {errors.ApellidoMaterno && <p className="error">{errors.ApellidoMaterno}</p>}

          <label>Correo</label>
          <input type="email" name="Correo" value={formData.Correo} onChange={handleChange} className={getInputClass("Correo")} />
          {errors.Correo && <p className="error">{errors.Correo}</p>}

          <label>Teléfono</label>
          <InputMask mask="999-999-9999" value={formData.Telefono} onChange={handleChange}>
            {(inputProps) => (
              <input {...inputProps} type="text" name="Telefono" placeholder="555-123-4567" className={getInputClass("Telefono")} />
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
          <InputMask mask="9999 9999 9999 9999" value={formData.NumeroTarjeta} onChange={handleChange}>
            {(inputProps) => (
              <input {...inputProps} type="text" name="NumeroTarjeta" placeholder="1234 5678 9012 3456" className={getInputClass("NumeroTarjeta")} />
            )}
          </InputMask>
          {errors.NumeroTarjeta && <p className="error">{errors.NumeroTarjeta}</p>}

          <label>Expiración</label>
          <InputMask mask="99/99" value={formData.Expiracion} onChange={handleChange}>
            {(inputProps) => (
              <input {...inputProps} type="text" name="Expiracion" placeholder="MM/AA" className={getInputClass("Expiracion")} />
            )}
          </InputMask>
          {errors.Expiracion && <p className="error">{errors.Expiracion}</p>}

          <label>CVV</label>
          <InputMask mask="9999" value={formData.Cvv} onChange={handleChange}>
            {(inputProps) => (
              <input {...inputProps} type="text" name="Cvv" placeholder="CVV" className={getInputClass("Cvv")} />
            )}
          </InputMask>
          {errors.Cvv && <p className="error">{errors.Cvv}</p>}

          {errors.general && <p className="error">{errors.general}</p>}

          <button type="submit">Suscribirse</button>
        </form>
      ) : (
        <div className="confirmation">
            {processing ? (
              <div>
                <h3>Procesando tu suscripción, espera un momento...</h3>
                <div className="spinner"></div> 
              </div>
            ) : (
              <h3>¡Gracias {formData.Nombre}, tu suscripción fue registrada con éxito!</h3>
            )}
          </div>

      )}
    </div>
  );
}

export default SubscriptionForm;