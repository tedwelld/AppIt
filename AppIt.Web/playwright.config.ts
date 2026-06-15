import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  timeout: 60_000,
  expect: {
    timeout: 15_000
  },
  use: {
    baseURL: 'http://localhost:4200',
    trace: 'on-first-retry'
  },
  webServer: [
    {
      command: 'dotnet run --project ../AppIt.Api/AppIt.Api.csproj --urls http://127.0.0.1:5175',
      url: 'http://127.0.0.1:5175/health',
      reuseExistingServer: true,
      timeout: 180_000,
      cwd: '..'
    },
    {
      command: 'npm run start -- --host 127.0.0.1 --port 4200',
      url: 'http://localhost:4200',
      reuseExistingServer: true,
      timeout: 180_000
    }
  ],
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] }
    }
  ]
});
