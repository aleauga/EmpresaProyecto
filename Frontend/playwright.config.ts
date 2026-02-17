import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './tests', // carpeta donde estarán tus pruebas
  use: {
    baseURL: 'http://localhost:3000', // tu app corriendo en dev
    headless: true,                   // corre sin abrir navegador (puedes poner false para ver la UI)
    viewport: { width: 1280, height: 720 },
    ignoreHTTPSErrors: true,
    video: 'retain-on-failure',       // guarda video si falla la prueba
    screenshot: 'only-on-failure',    // captura pantalla si falla
  },
});
