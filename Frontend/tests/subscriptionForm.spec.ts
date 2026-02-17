import { test, expect } from '@playwright/test';

test('Usuario completa suscripción correctamente', async ({ page }) => {
   await page.goto('/subscription');

//npx playwright test --ui
  // Llenar datos
  await page.fill('input[name="Nombre"]', 'Alejandra');
  await page.fill('input[name="ApellidoPaterno"]', 'García');
  await page.fill('input[name="ApellidoMaterno"]', 'López');
  await page.fill('input[name="Correo"]', 'alejandra@test.com');
  await page.fill('input[name="Telefono"]', '5551234567');
  await page.selectOption('select[name="Plan"]', 'Premium');
  await page.fill('input[name="NumeroTarjeta"]', '4111111111111111');
  await page.fill('input[name="Expiracion"]', '12/28');
  await page.fill('input[name="Cvv"]', '123');

  // Enviar formulario
  await page.click('button[type="submit"]');

  // Validar confirmación
  await expect(page.locator('.confirmation-table')).toBeVisible();
  await expect(page.locator('.confirmation-table')).toContainText('Alejandra');
  await expect(page.locator('.confirmation-table')).toContainText('Premium');
  //await expect(page.locator('.confirmation-table')).toContainText('Pendiente');

  await page.waitForFunction(() =>
  document.querySelector('.confirmation-table')?.textContent?.includes('Active')
, { timeout: 50000 });

});