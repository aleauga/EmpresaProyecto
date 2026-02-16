CREATE DATABASE IF NOT EXISTS EmpresaProyecto;

USE EmpresaProyecto;

CREATE TABLE IF NOT EXISTS Cliente (
    IdCliente VARCHAR(36) PRIMARY KEY,
    Nombre VARCHAR(200) NOT NULL,
    ApellidoPaterno VARCHAR(200) NOT NULL,
    ApellidoMaterno VARCHAR(200),
    Correo VARCHAR(500) NOT NULL,
    Telefono VARCHAR(20) NOT NULL
);

CREATE TABLE IF NOT EXISTS Suscripcion (
    IdSuscripcion BIGINT AUTO_INCREMENT PRIMARY KEY,
    IdCliente VARCHAR(36) NOT NULL,
    FechaCreacion DATETIME NOT NULL,
    FechaPago DATETIME,
    UltimaFechaModificacion DATETIME NOT NULL,
    Plan VARCHAR(20) NOT NULL,
    Estado VARCHAR(20) NOT NULL,
    CONSTRAINT FK_Suscripcion_Cliente FOREIGN KEY (IdCliente) REFERENCES Cliente (IdCliente)
);
